using Serilog.Context;
using Skill_Loop.Application.Common.Constants;

namespace Skill_Loop.Api.Middlewares;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationConstants.HeaderKey, out var value)
            ? value.ToString()
            : Guid.NewGuid().ToString();

        context.Items[CorrelationConstants.HeaderKey] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationConstants.HeaderKey] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}