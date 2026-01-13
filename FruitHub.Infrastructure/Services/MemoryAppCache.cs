using FruitHub.ApplicationCore.Interfaces;
using FruitHub.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace FruitHub.Infrastructure.Services; 
 
public class MemoryAppCache : IAppCache
{
    private readonly IMemoryCache _cache;

    public MemoryAppCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        _cache.Set(key, value, ttl);
        return Task.CompletedTask;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        return Task.FromResult(_cache.Get<T>(key));
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}