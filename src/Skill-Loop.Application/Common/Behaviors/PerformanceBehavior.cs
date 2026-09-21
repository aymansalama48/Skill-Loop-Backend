using MediatR;
using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.Core;
using System.Diagnostics;

namespace Skill_Loop.Application.Common.Behaviors;


/// <summary>
/// سلوك مراقبة الأداء (Performance Profiling Behavior).
/// يحسب زمن تنفيذ كل طلب ويسجل تحذيراً إذا تجاوز الحدود المقبولة (800ms).
/// </summary>
public sealed class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
    ICorrelationContext correlationContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = correlationContext.CorrelationId;

        var timer = Stopwatch.StartNew();

        var response = await next();

        timer.Stop();

        // التنبيه في حالة بطء استجابة الطلب
        if (timer.ElapsedMilliseconds > 800)
        {
            logger.LogWarning(
                "Slow Request | {Request} | CorrelationId: {CorrelationId} | Duration: {Time}ms",
                requestName,
                correlationId,
                timer.ElapsedMilliseconds);
        }

        return response;
    }
}