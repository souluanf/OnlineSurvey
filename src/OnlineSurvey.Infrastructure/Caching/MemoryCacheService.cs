using Microsoft.Extensions.Caching.Memory;
using OnlineSurvey.Application.Interfaces;

namespace OnlineSurvey.Infrastructure.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private static readonly HashSet<string> CacheKeys = [];
    private static readonly object LockObject = new();

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions();

        if (expiration.HasValue)
            options.AbsoluteExpirationRelativeToNow = expiration;
        else
            options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

        _cache.Set(key, value, options);

        lock (LockObject)
        {
            CacheKeys.Add(key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);

        lock (LockObject)
        {
            CacheKeys.Remove(key);
        }

        return Task.CompletedTask;
    }
}
