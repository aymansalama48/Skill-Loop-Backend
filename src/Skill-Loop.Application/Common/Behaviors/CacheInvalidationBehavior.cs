using MediatR;
using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Behaviors;

public sealed class CacheInvalidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheInvalidatorCommand
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheInvalidationBehavior<TRequest, TResponse>> _logger;

    public CacheInvalidationBehavior(
        ICacheService cacheService,
        ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (response is Result { IsSuccess: false })
        {
            _logger.LogDebug("Command failed. Cache invalidation skipped.");
            return response;
        }

        if (request.CacheKeys.Count == 0)
        {
            return response;
        }

        foreach (var cacheKey in request.CacheKeys)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // 👇 التعديل السحري هنا: استخدام المسح بالبادئة
            await _cacheService.RemoveByPrefixAsync(cacheKey, cancellationToken);

            _logger.LogDebug("Invalidated cache keys starting with {CacheKey}", cacheKey);
        }

        return response;
    }
}