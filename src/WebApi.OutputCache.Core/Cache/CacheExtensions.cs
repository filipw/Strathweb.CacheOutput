using System;
using System.Threading.Tasks;

namespace WebApi.OutputCache.Core.Cache
{
    public static class CacheExtensions
    {
        public static async Task<T> GetCachedResultAsync<T>(this IApiOutputCache cache, string key, DateTimeOffset expiry, Func<T> resultGetter, bool bypassCache = true) where T : class
        {
            var result = await cache.GetAsync<T>(key);

            if (result == null || bypassCache)
            {
                result = resultGetter();
                if (result != null) await cache.AddAsync(key, result, expiry);
            }

            return result;
        }
    }
}