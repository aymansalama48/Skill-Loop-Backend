using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Constants;

namespace Skill_Loop.Api.Middlewares;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IHostEnvironment _env;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private const string InternalServerErrorMessage = "حدث خطأ غير متوقع في الخادم، يرجى المحاولة لاحقاً.";

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
        _logger.LogError(exception, "Unhandled Exception: {Message}", exception.Message);

        var correlationId = httpContext.Items[CorrelationConstants.HeaderKey]?.ToString()
                            ?? httpContext.TraceIdentifier;

        // 1. معالجة تعارض التزامن (RowVersion / DbUpdateConcurrencyException)
        if (exception is DbUpdateConcurrencyException)
        {
            httpContext.Response.ContentType = "application/problem+json";
            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

            var conflictDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = "تم تعديل هذه البيانات بواسطة مستخدم آخر في نفس الوقت، يرجى إعادة تحميل الصفحة والمحاولة مجدداً.",
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
                Type = "https://httpstatuses.com/409"
            };

            conflictDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
            conflictDetails.Extensions["correlationId"] = correlationId;

            await httpContext.Response.WriteAsJsonAsync(conflictDetails, cancellationToken);
            return true;
        }

        // 2. معالجة أي خطأ عام غير متوقع (500)
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