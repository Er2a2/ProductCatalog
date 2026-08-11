using System.Text.Json;
using ProductCatalog.Application.Interfaces.Caching;
using StackExchange.Redis;

namespace ProductCatalog.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var db = _redis.GetDatabase();

        var cachedValue = await db.StringGetAsync(key);

        if (!cachedValue.HasValue)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(cachedValue!);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiry = null)
    {
        var db = _redis.GetDatabase();

        var json = JsonSerializer.Serialize(value);

        await db.StringSetAsync(
            key,
            json,
            expiry,
            when: When.Always);
    }

    public async Task RemoveAsync(string key)
    {
        var db = _redis.GetDatabase();

        await db.KeyDeleteAsync(key);
    }
}
