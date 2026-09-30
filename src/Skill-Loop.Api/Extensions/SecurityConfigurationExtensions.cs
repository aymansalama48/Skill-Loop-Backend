namespace Skill_Loop.Api.Extensions;

/// <summary>
/// Fail-fast validation of security-critical configuration.
///
/// Why this exists: every secret in this application previously had a working default
/// committed to <c>appsettings.json</c>. The application started happily, so nothing
/// surfaced the problem until someone read the repository and used the key. Config that
/// is required for security must be required at startup, not discovered during an incident.
///
/// This runs before any listener is opened, so a misconfigured deployment fails on
/// startup instead of serving traffic with a predictable signing key.
/// </summary>
public static class SecurityConfigurationExtensions
{
    /// <summary>
    /// Values that are known-bad placeholders. If any of these reach production, the
    /// application is signing tokens with a public string.
    /// </summary>
    private static readonly string[] KnownCompromisedPlaceholders =
    [
        "YourSuper46854SecretOtpHashingSec56645retKeyHere_Minimum465645132Characters!",
        "CHANGE_ME",
        "CHANGEME",
        "your-secret-key",
        "supersecret",
        "secret",
        "password"
    ];

    public static IServiceCollection AddValidatedSecurityConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var failures = new List<string>();

        // ---------------------------------------------------------------
        // 1. JWT signing key — the single most damaging secret to get wrong
        // ---------------------------------------------------------------
        var jwtKey = configuration["Jwt:Key"];
        ValidateSecret(
            failures,
            "Jwt:Key",
            jwtKey,
            minimumLength: 32,
            environment,
            instructions:
                "dotnet user-secrets set \"Jwt:Key\" \"$(openssl rand -base64 48)\" " +
                "(development) or set the Jwt__Key environment variable (production).");

        if (!string.IsNullOrWhiteSpace(jwtKey))
        {
            // 60 minutes is a wide window for a stolen access token; anything beyond an
            // hour means a leaked token is useful for the rest of the working day.
            var expiry = configuration.GetValue("Jwt:ExpiryMinutes", 60);
            if (expiry <= 0 || expiry > 60)
            {
                failures.Add(
                    $"Jwt:ExpiryMinutes is {expiry} but must be greater than 0 and at most 60. " +
                    "A longer lifetime widens the damage window of a leaked access token.");
            }
        }

        // ---------------------------------------------------------------
        // 2. OTP hashing secret — HMAC key for verification codes
        // ---------------------------------------------------------------
        ValidateSecret(
            failures,
            "OtpSettings:HashingSecret",
            configuration["OtpSettings:HashingSecret"],
            minimumLength: 32,
            environment,
            instructions:
                "dotnet user-secrets set \"OtpSettings:HashingSecret\" \"$(openssl rand -base64 48)\" " +
                "or set OtpSettings__HashingSecret. The previous committed value must be " +
                "treated as public and replaced everywhere.");

        // OTP cooldown guards inbox flooding; a zero/negative value disables it entirely.
        var cooldown = configuration["OtpSettings:ResendCooldown"];
        if (!string.IsNullOrWhiteSpace(cooldown))
        {
            if (!TimeSpan.TryParse(cooldown, out var parsedCooldown))
            {
                failures.Add($"OtpSettings:ResendCooldown ('{cooldown}') is not a valid TimeSpan.");
            }
            else if (parsedCooldown < TimeSpan.FromMinutes(1))
            {
                failures.Add(
                    $"OtpSettings:ResendCooldown is {parsedCooldown} but must be at least 00:01:00. " +
                    "A near-zero cooldown lets one caller flood an inbox and burn SMTP quota.");
            }
        }

        // ---------------------------------------------------------------
        // 3. CORS — a wildcard origin with credentials is a full data leak
        // ---------------------------------------------------------------
        var allowedOrigins = configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
                            ?? [];

        if (allowedOrigins.Length == 0)
        {
            failures.Add(
                "CorsSettings:AllowedOrigins is empty. Without an explicit allowlist the API " +
                "either blocks the frontend or (worse) falls back to a wildcard with credentials.");
        }

        foreach (var origin in allowedOrigins)
        {
            if (origin == "*")
            {
                failures.Add(
                    "CorsSettings:AllowedOrigins contains '*'. A wildcard origin combined with " +
                    "credentialed requests lets any website read authenticated API responses.");
            }

            if (!Uri.TryCreate(origin, UriKind.Absolute, out var parsedOrigin))
            {
                failures.Add($"CorsSettings:AllowedOrigins entry '{origin}' is not an absolute URL.");
                continue;
            }

            if (parsedOrigin.Scheme != Uri.UriSchemeHttps
                && !environment.IsDevelopment()
                && !IsLoopback(parsedOrigin))
            {
                failures.Add(
                    $"CorsSettings:AllowedOrigins entry '{origin}' uses plain HTTP outside " +
                    "development. Browser CORS does not protect a plaintext connection.");
            }
        }

        // ---------------------------------------------------------------
        // 4. Connection string must be present
        // ---------------------------------------------------------------
        if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("DefaultConnection")))
        {
            failures.Add(
                "ConnectionStrings:DefaultConnection is not set. Startup would otherwise fail " +
                "later during migration, after the process already looks healthy.");
        }

        // ---------------------------------------------------------------
        // 5. Production-only: no localhost CORS, no development seeding
        // ---------------------------------------------------------------
        if (!environment.IsDevelopment())
        {
            foreach (var origin in allowedOrigins.Where(
                         o => Uri.TryCreate(o, UriKind.Absolute, out var uri) && IsLoopback(uri)))
            {
                failures.Add(
                    $"CorsSettings:AllowedOrigins contains the loopback origin '{origin}' in a " +
                    "non-Development environment. This is either a leftover or an attempt to " +
                    "allow a local proxy, and should be replaced with the real frontend origin.");
            }
        }

        if (failures.Count > 0)
        {
            var message =
                "Security configuration validation failed. The application will not start, " +
                "because starting with these values would expose the system:\n" +
                string.Join("\n", failures.Select(f => "  - " + f));

            throw new InvalidOperationException(message);
        }

        return services;
    }

    private static void ValidateSecret(
        ICollection<string> failures,
        string key,
        string? value,
        int minimumLength,
        IWebHostEnvironment environment,
        string instructions)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            failures.Add(
                $"{key} is not set. Supply it via user-secrets or an environment variable. " +
                $"{instructions}");
            return;
        }

        if (value.Length < minimumLength)
        {
            failures.Add(
                $"{key} is only {value.Length} characters but must be at least {minimumLength}. " +
                "HMAC-SHA256 signing keys shorter than 32 bytes can be brute-forced or " +
                "collided; the generator also refuses to sign with them.");
        }
        var normalized = value.Trim().ToLowerInvariant();

        if (KnownCompromisedPlaceholders.Any(p => normalized == p.ToLowerInvariant()))
        {
            failures.Add(
                $"{key} still holds a known placeholder value that was previously committed " +
                "to the repository. It must be treated as public and replaced immediately.");
        }

        if (normalized.StartsWith("your") || normalized.Contains("placeholder", StringComparison.OrdinalIgnoreCase))
        {
            failures.Add(
                $"{key} still looks like a placeholder (\"{Redact(value)}\"). It must be treated " +
                "as public and replaced with a generated secret.");
        }
    }

    private static bool IsLoopback(Uri uri) =>
        uri.IsLoopback
        || uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
        || uri.Host.Equals("127.0.0.1", StringComparison.Ordinal)
        || uri.Host.Equals("::1", StringComparison.Ordinal);

    /// <summary>
    /// Shows the shape of a bad secret without printing the value, so a startup log can
    /// still be pasted into a ticket safely.
    /// </summary>
    private static string Redact(string value) =>
        value.Length <= 4 ? "***" : $"{value[..2]}***{value[^2..]} ({value.Length} chars)";
}
