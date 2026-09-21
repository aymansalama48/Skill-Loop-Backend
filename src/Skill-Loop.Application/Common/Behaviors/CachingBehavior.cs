using MediatR;
using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Behaviors;

public sealed class CachingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery // 👈 التعديل هنا: شلنا الـ <TResponse>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        ICacheService cacheService,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Checking cache for key {CacheKey}", request.CacheKey);

        var response = await _cacheService.GetOrCreateAsync(
            key: request.CacheKey,

            factory: async ct =>
            {
                _logger.LogDebug("Cache miss for key {CacheKey}. Executing handler.", request.CacheKey);
                return await next();
            },

            shouldCache: response =>
            {
                return response is not Result { IsSuccess: false };
            },

            slidingExpiration: request.SlidingExpiration,
            absoluteExpiration: request.AbsoluteExpiration,
            cancellationToken: cancellationToken);

        _logger.LogDebug("Caching completed for key {CacheKey}", request.CacheKey);

        return response!;
    }
}