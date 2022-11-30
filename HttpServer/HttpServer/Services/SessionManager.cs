using System.Collections.Concurrent;
using HttpServer.Models;
using Microsoft.Extensions.Caching.Memory;
namespace HttpServer;

public class SessionManager
{
    private static MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private static ConcurrentDictionary<object, SemaphoreSlim> _locks = new ();
 
    public static async Task<Guid?> GetOrAdd(string email, int id)
    {
        var session = new Session(Guid.NewGuid(), id, email);
        if (!_cache.TryGetValue(session.Guid, out Session? cacheEntry))
        {
            var myLock = _locks.GetOrAdd(session.Guid, new SemaphoreSlim(1, 1));
 
            await myLock.WaitAsync();
            try
            {
                if (!_cache.TryGetValue(session.Guid, out cacheEntry))
                {
                    cacheEntry = session;
                    var cacheEntryOptions =
                        new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromDays(3))
                            .SetAbsoluteExpiration(TimeSpan.FromMinutes(3));
                    _cache.Set(email, cacheEntry, cacheEntryOptions);
                }
            }
            finally
            {
                myLock.Release();
            }
        }
        return cacheEntry?.Guid;
    }

    public static async Task<bool> CheckSession(Guid guid)
    {
        var contains = _cache.TryGetValue(guid, out Session session);
        return await (contains ? Task.FromResult(contains) : throw new KeyNotFoundException("Couldn't find this email"));
    }

    public static async Task<Session?> GetInfo(Guid guid)
        => await Task.FromResult(_cache.Get<Session>(guid));
    
}