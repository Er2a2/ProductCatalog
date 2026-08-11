namespace ProductCatalog.Application.Interfaces.Caching;

public interface ICacheLock
{
    SemaphoreSlim GetLock(string key);
}
