using System.Collections.Concurrent;
using ProductCatalog.Application.Interfaces.Caching;

namespace ProductCatalog.Infrastructure.Caching;

public class ProductCacheLock : ICacheLock
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public SemaphoreSlim GetLock(string key)
    {
        return _locks.GetOrAdd(
            key,
            _ => new SemaphoreSlim(1, 1));
    }
}