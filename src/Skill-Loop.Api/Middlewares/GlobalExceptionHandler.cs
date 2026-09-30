using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Constants;

namespace Skill_Loop.Api.Middlewares;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IHostEnvironment _env;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private const string InternalServerErrorMessage = "An unexpected server error occurred. Please try again later.";

    public GlobalExceptionHandler(IHostEnvironment env, ILogger<GlobalExceptionHandler> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.Items[CorrelationConstants.HeaderKey]?.ToString()
                            ?? httpContext.TraceIdentifier;

        // 1. طلب مُصادَق عليه بدون User Id claim.
        //
        // Raised by BaseApiController.RequireUserId(). This is an authentication failure,
        // not a server fault, so it must not be reported as 500: a 500 tells the client the
        // request was well-formed and the problem is ours, which both hides the real cause
        // and pollutes error dashboards. It is logged at Warning because a spike here
        // usually means a broken token or a middleware ordering bug, not an attack.
        if (exception is UnauthorizedAccessException)
        {
            _logger.LogWarning(
                exception,
                "Authenticated request without a valid user id claim. {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            httpContext.Response.ContentType = "application/problem+json";
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

            var unauthorizedDetails = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "Authentication is required and the supplied credentials are not valid.",
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
                Type = "https://httpstatuses.com/401"
            };

            unauthorizedDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
            unauthorizedDetails.Extensions["correlationId"] = correlationId;

            await httpContext.Response.WriteAsJsonAsync(unauthorizedDetails, cancellationToken);
            return true;
        }

        _logger.LogError(exception, "Unhandled Exception: {Message}", exception.Message);

        // 2. معالجة تعارض التزامن (RowVersion / DbUpdateConcurrencyException)
        if (exception is DbUpdateConcurrencyException)
        {
            httpContext.Response.ContentType = "application/problem+json";
            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

            var conflictDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = "This record has been modified by another user concurrently. Please reload and try again.",
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
                Type = "https://httpstatuses.com/409"
            };

            conflictDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
            conflictDetails.Extensions["correlationId"] = correlationId;

            await httpContext.Response.WriteAsJsonAsync(conflictDetails, cancellationToken);
            return true;
        }

        // 3. معالجة أي خطأ عام غير متوقع (500)
        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Detail = _env.IsDevelopment() ? exception.Message : InternalServerErrorMessage,
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
            Type = "https://httpstatuses.com/500"
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["correlationId"] = correlationId;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}