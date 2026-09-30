using Serilog.Context;
using Skill_Loop.Application.Common.Constants;

namespace Skill_Loop.Api.Middlewares;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    /// <summary>
    /// A correlation ID is client-supplied and ends up in both the response header and every
    /// log line for the request. Accepting it unvalidated gives an attacker:
    /// 1. log forging — a value containing CR/LF can append fake entries to the log file,
    /// 2. header/log bloat — an unbounded value can be megabytes of log per request,
    /// 3. header injection — invalid characters make the response throw a 500.
    /// So an untrusted value is only accepted if it is short and purely alphanumeric.
    /// </summary>
    private const int MaxLength = 64;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);

        context.Items[CorrelationConstants.HeaderKey] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationConstants.HeaderKey] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationConstants.HeaderKey, out var value))
            return Guid.NewGuid().ToString();

        var candidate = value.ToString();

        if (string.IsNullOrWhiteSpace(candidate) || candidate.Length > MaxLength)
            return Guid.NewGuid().ToString();

        foreach (var c in candidate)
        {
            var isSafe = char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.';

            if (!isSafe)
                return Guid.NewGuid().ToString();
        }

        return candidate;
    }
}