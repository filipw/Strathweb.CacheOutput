using System;
using System.Runtime.Caching;

namespace WebAPI.OutputCache.Cache
{
    public class MemoryCacheDefault : IApiOutputCache
    {
        private static readonly MemoryCache Cache = MemoryCache.Default;

        public T Get<T>(string key) where T : class
        {
            var o = Cache.Get(key) as T;
            return o;
        }

        public object Get(string key)
        {
            return Cache.Get(key);
        }

        public void Remove(string key)
        {
            Cache.Remove(key);
        }

        public bool Contains(string key)
        {
            return Cache.Contains(key);
        }

        public void Add(string key, object o, DateTimeOffset expiration)
        {
            Cache.Add(key, o, expiration);
        }
    }
}