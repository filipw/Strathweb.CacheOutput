using System;

namespace WebApi.OutputCache.Core.Cache
{
    public static class CacheExtensions
    {
        public static T GetCachedResult<T>(this IApiOutputCache cache, string key, DateTimeOffset expiry, Func<T> resultGetter, bool bypassCache = true) where T : class
        {
            var result = cache.Get<T>(key);

            if (result == null || bypassCache)
            {
                result = resultGetter();
                if (result != null) cache.Add(key, result, expiry);
            }

            return result;
        }
    }
}