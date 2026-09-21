using MediatR;
using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Behaviors;

/// <summary>
/// سلوك تسجيل السجلات (Logging Behavior).
/// يسجل بداية ونهاية كل طلب مع ربط الـ CorrelationId والمستخدم لسهولة التتبع.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentUser currentUser,
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
        var userId = currentUser.UserId.ToString() ?? "Anonymous";

        // نطاق تسجيل يحتوي على معلومات السياق للـ Structured Logging
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["Request"] = requestName,
            ["CorrelationId"] = correlationId,
            ["UserId"] = userId
        });

        logger.LogInformation(
            "Handling {Request} | CorrelationId: {CorrelationId} | User: {UserId}",
            requestName,
            correlationId,
            userId);

        try
        {
            var response = await next();

            // تسجيل تحذير في حالة الفشل المنطقي (Business Failure)
            if (response is Result result && !result.Succeeded)
            {
                logger.LogWarning(
                    "Business Failure | {Request} | CorrelationId: {CorrelationId} | Errors: {Errors}",
                    requestName,
                    correlationId,
                    string.Join(" | ", result.Errors.Select(e => e.Description)));
            }
            else
            {
                logger.LogInformation(
                    "Request Success | {Request} | CorrelationId: {CorrelationId}",
                    requestName,
                    correlationId);
            }

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unhandled Exception | {Request} | CorrelationId: {CorrelationId}",
                requestName,
                correlationId);

            throw;
        }
    }
}