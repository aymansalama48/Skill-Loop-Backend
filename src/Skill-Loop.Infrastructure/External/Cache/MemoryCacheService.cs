using Microsoft.Extensions.Caching.Memory;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using System.Collections.Concurrent;

namespace Skill_Loop.Infrastructure.External.Cache;

public sealed class MemoryCacheService : ICacheService
{
    private static readonly TimeSpan DefaultSlidingExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan DefaultAbsoluteExpiration = TimeSpan.FromMinutes(30);

    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _keyLocks = new();

    // 👇 قاموس جديد لتتبع المفاتيح النشطة
    private readonly ConcurrentDictionary<string, bool> _activeCacheKeys = new();

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();
        _memoryCache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        var options = CreateCacheOptions(slidingExpiration, absoluteExpiration);
        _memoryCache.Set(key, value, options);

        _activeCacheKeys.TryAdd(key, true); // 👈 تسجيل المفتاح

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        _memoryCache.Remove(key);
        _activeCacheKeys.TryRemove(key, out _); // 👈 إزالة المفتاح من المتتبع

        return Task.CompletedTask;
    }

    // 👇 تنفيذ دالة المسح بالبادئة
    public Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefixKey);
        cancellationToken.ThrowIfCancellationRequested();

        // البحث عن كل المفاتيح التي تبدأ بالبادئة المطلوبة
        var keysToRemove = _activeCacheKeys.Keys
            .Where(k => k.StartsWith(prefixKey, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _memoryCache.Remove(key);
            _activeCacheKeys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, Func<T, bool>? shouldCache = null, TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);
        cancellationToken.ThrowIfCancellationRequested();

        if (_memoryCache.TryGetValue(key, out T? cachedValue))
            return cachedValue;

        var semaphore = _keyLocks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            if (_memoryCache.TryGetValue(key, out cachedValue))
                return cachedValue;

            var value = await factory(cancellationToken);
            var cacheValue = value is not null && (shouldCache?.Invoke(value) ?? true);

            if (cacheValue)
            {
                var options = CreateCacheOptions(slidingExpiration, absoluteExpiration);
                _memoryCache.Set(key, value, options);
                _activeCacheKeys.TryAdd(key, true); // 👈 تسجيل المفتاح
            }

            return value;
        }
        finally
        {
            semaphore.Release();
        }
    }

    // 👇 خليناها دالة عادية (مش static) عشان نقدر نربطها بالـ Callback
    private MemoryCacheEntryOptions CreateCacheOptions(TimeSpan? slidingExpiration, TimeSpan? absoluteExpiration)
    {
        var options = new MemoryCacheEntryOptions
        {
            Size = 1,
            SlidingExpiration = slidingExpiration ?? DefaultSlidingExpiration,
            AbsoluteExpirationRelativeToNow = absoluteExpiration ?? DefaultAbsoluteExpiration
        };

        // 👇 تنظيف المتتبع أوتوماتيكياً عند انتهاء صلاحية الكاش
        options.RegisterPostEvictionCallback((evictedKey, _, _, _) =>
        {
            if (evictedKey is string stringKey)
            {
                _activeCacheKeys.TryRemove(stringKey, out _);
            }
        });

        return options;
    }
}
