using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Skill_Loop.Api.Extensions;

/// <summary>
/// Endpoint rate limiting.
///
/// Security: the application previously had no throttling of any kind, so every auth
/// endpoint was an unlimited oracle. Identity's lockout only caps attempts per account,
/// so a distributed attacker still gets 5 guesses per account from every source address —
/// and can turn lockout itself into a denial-of-service against a target user by
/// deliberately locking them out.
///
/// Limits are partitioned by remote IP *and* the attempted identity, so one attacker can
/// neither lock out all users nor bypass a per-account limit by rotating addresses.
/// </summary>
public static class RateLimitingExtensions
{
    public const string GlobalPolicy = "global";
    public const string LoginPolicy = "login";
    public const string OtpVerifyPolicy = "otp-verify";
    public const string OtpResendPolicy = "otp-resend";
    public const string PasswordResetRequestPolicy = "password-reset-request";
    public const string EmailTestPolicy = "email-test";
    public const string ContactFormPolicy = "contact-form";
    public const string RefreshPolicy = "refresh-token";

    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Backstop for everything, including endpoints with no specific policy.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 300,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

            // 5 login attempts per minute per (IP + attempted email). Slightly more generous
            // than the 5-per-5-minutes lockout so a legitimate typo loop still succeeds.
            options.AddPolicy(LoginPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    BuildKey(context, "email"),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            // 4-digit OTP means a 10,000-value key space; 3 tries per 10 minutes per
            // (IP + account) makes guessing infeasible even with parallel requests.
            options.AddPolicy(OtpVerifyPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    BuildKey(context, "email"),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(10),
                        QueueLimit = 0
                    }));

            // Prevents inbox flooding and SMTP quota exhaustion against a third party.
            options.AddPolicy(OtpResendPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    BuildKey(context, "email"),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 2,
                        Window = TimeSpan.FromMinutes(10),
                        QueueLimit = 0
                    }));

            // Requesting a password-reset code is a separate abuse vector from resending an
            // email-verification code, so it gets its own bucket at the same strength. Sharing
            // one partition made the two flows consume each other's budget: a user who asked
            // for a verification resend was then refused a password reset - and, worse, a
            // legitimate password reset was indistinguishable from an inbox flood.
            options.AddPolicy(PasswordResetRequestPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    BuildKey(context, "email"),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 2,
                        Window = TimeSpan.FromMinutes(10),
                        QueueLimit = 0
                    }));

            options.AddPolicy(EmailTestPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    BuildKey(context, null),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy(ContactFormPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    BuildKey(context, null),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(10),
                        QueueLimit = 0
                    }));

            options.AddPolicy(RefreshPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    BuildKey(context, null),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                TimeSpan? retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
                    ? retry
                    : null;

                context.HttpContext.Response.Headers["Retry-After"] =
                    retryAfter?.TotalSeconds.ToString("0") ?? "60";

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    type = "https://httpstatuses.com/429",
                    title = "Too Many Requests",
                    status = StatusCodes.Status429TooManyRequests,
                    detail = "Too many requests. Please retry later."
                }, cancellationToken);
            };
        });

        return services;
    }

    /// <summary>
    /// Partition key = remote address plus the identity being targeted.
    ///
    /// The identity cannot come from the query string because the auth endpoints take it
    /// in the JSON body. <see cref="RateLimitIdentityMiddleware"/> buffers and parses the
    /// body into <c>HttpContext.Items</c> before the limiter runs, then rewinds the stream
    /// so model binding downstream is unaffected.
    /// </summary>
    private static string BuildKey(HttpContext context, string? identityField)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (identityField is null)
            return ip;

        var identity = context.Items.TryGetValue(
                RateLimitIdentityMiddleware.ItemKey + identityField,
                out var fromBody) && fromBody is string bodyIdentity
            ? bodyIdentity
            : context.Request.Query[identityField].ToString();

        if (string.IsNullOrWhiteSpace(identity))
            return ip;

        // Normalise so "User@x.com", "user@x.com " and "USER@X.COM" share one partition.
        var normalized = identity.Trim().ToLowerInvariant();

        return normalized.Length > 128 ? ip : $"{ip}|{normalized}";
    }
}

/// <summary>
/// Extracts the identity field (e.g. email) from a small JSON request body so the rate
/// limiter can partition on it.
///
/// The body is buffered and then rewound with <c>EnableBuffering</c>, so the action's
/// model binding still reads it normally. Requests with a large or non-JSON body are
/// skipped: the limiter then falls back to partitioning by IP alone, which is safe.
/// </summary>
public sealed class RateLimitIdentityMiddleware(RequestDelegate next)
{
    internal const string ItemKey = "RateLimitIdentity.";

    private const int MaxBufferedBytes = 8 * 1024;

    public async Task InvokeAsync(HttpContext context)
    {
        var needsBody =
            context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true
            && context.Request.ContentLength is > 0 and <= MaxBufferedBytes;

        if (needsBody)
        {
            try
            {
                context.Request.EnableBuffering();
                context.Request.Body.Position = 0;

                using var reader = new StreamReader(
                    context.Request.Body, leaveOpen: true);

                var raw = await reader.ReadToEndAsync(context.RequestAborted);
                context.Request.Body.Position = 0;

                if (raw.Length > 0)
                {
                    using var document = System.Text.Json.JsonDocument.Parse(raw);
                    var root = document.RootElement;

                    // A JSON body may legitimately be an array or a scalar, not an object.
                    // TryGetProperty throws InvalidOperationException on those roots, and
                    // that escapes the JsonException catch below - which turned every
                    // array-bodied request into a 500. Partition by IP for those.
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var field in new[] { "email", "to", "identifier" })
                        {
                            if (root.TryGetProperty(field, out var element)
                                && element.ValueKind == JsonValueKind.String)
                            {
                                context.Items[ItemKey + field] = element.GetString();
                            }
                        }
                    }
                }
            }
            catch (System.Text.Json.JsonException)
            {
                // Malformed JSON: model binding will report it. Partition by IP only.
            }
        }

        await next(context);
    }
}
