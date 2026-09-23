using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Skill_Loop.Infrastructure.External.Cache;

public sealed class RedisCacheService : ICacheService
{
    private static readonly TimeSpan DefaultSlidingExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan DefaultAbsoluteExpiration = TimeSpan.FromMinutes(30);

    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _keyLocks = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _database = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var value = await _database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>((byte[])value!, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis error while retrieving key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? slidingExpiration = null,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var expiry = absoluteExpiration ?? slidingExpiration ?? DefaultAbsoluteExpiration;
            var json = JsonSerializer.Serialize(value, JsonOptions);
            await _database.StringSetAsync(key, json, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis error while setting key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis error while removing key: {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefixKey);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            foreach (var endpoint in _redis.GetEndPoints())
            {
                var server = _redis.GetServer(endpoint);
                if (!server.IsConnected) continue;

                var keys = server.Keys(pattern: $"{prefixKey}*").ToArray();
                if (keys.Length > 0)
                {
                    await _database.KeyDeleteAsync(keys);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis error while removing prefix: {Prefix}", prefixKey);
        }
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        Func<T, bool>? shouldCache = null,
        TimeSpan? slidingExpiration = null,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);
        cancellationToken.ThrowIfCancellationRequested();

        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var semaphore = _keyLocks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            cached = await GetAsync<T>(key, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }

            var value = await factory(cancellationToken);
            var canCache = value is not null && (shouldCache?.Invoke(value) ?? true);

            if (canCache)
            {
                await SetAsync(key, value, slidingExpiration, absoluteExpiration, cancellationToken);
            }

            return value;
        }
        finally
        {
            semaphore.Release();
        }
    }
}
