namespace Skill_Loop.Application.Common.Abstractions.External.Cache;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    // 👇 الدالة الجديدة لمسح الكاش بالبادئة (Prefix)
    Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default);

    Task<T?> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, Func<T, bool>? shouldCache = null, TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default);
}