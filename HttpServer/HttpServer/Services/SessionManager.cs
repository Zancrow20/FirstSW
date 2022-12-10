using System.Collections.Concurrent;
using HttpServer.Models;
using Microsoft.Extensions.Caching.Memory;
namespace HttpServer;

public class SessionManager
{
    private static MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private static ConcurrentDictionary<object, SemaphoreSlim> _locks = new ();
 
    public static async Task<Session?> GetOrAdd(string email, int id, Guid guid)
    {
        if (!_cache.TryGetValue(guid, out Session? cacheEntry))
        {
            var myLock = _locks.GetOrAdd(id, new SemaphoreSlim(1, 1));
 
            await myLock.WaitAsync();
            try
            {
                if (!_cache.TryGetValue(guid, out cacheEntry))
                {
                    cacheEntry = new Session(guid, id, email);
                    var cacheEntryOptions =
                        new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromDays(3))
                            .SetAbsoluteExpiration(TimeSpan.FromDays(1));
                    _cache.Set(guid, cacheEntry, cacheEntryOptions);
                }
            }
            finally
            {
                myLock.Release();
            }
        }
        return cacheEntry;
    }

    public static async Task<bool> CheckSession(Guid guid)
    {
        var contains = _cache.TryGetValue(guid, out Session session);
        return await (contains ? Task.FromResult(contains) : throw new KeyNotFoundException("Couldn't find this email"));
    }

    public static async Task<Session?> GetInfo(Guid guid)
        => (Session)(await Task.FromResult(_cache.Get(guid)))!;
    
}