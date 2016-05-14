using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace WebApi.OutputCache.Core.Cache
{
    public class MemoryCacheDefault : IApiOutputCache
    {
        private static readonly MemoryCache Cache = MemoryCache.Default;

        public virtual void RemoveStartsWith(string key)
        {
            lock (Cache)
            {
                Cache.Remove(key);
            }
        }

        public virtual T Get<T>(string key) where T : class
        {
            var o = Cache.Get(key) as T;
            return o;
        }

        [Obsolete("Use Get<T> instead")]
        public virtual object Get(string key)
        {
            return Cache.Get(key);
        }

        public virtual void Remove(string key)
        {
            lock (Cache)
            {
                Cache.Remove(key);
            }
        }

        public virtual bool Contains(string key)
        {
            return Cache.Contains(key);
        }

        public virtual void Add(string key, object o, DateTimeOffset expiration, string dependsOnKey = null)
        {
            var cachePolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = expiration
            };

            if (!string.IsNullOrWhiteSpace(dependsOnKey))
            {
                cachePolicy.ChangeMonitors.Add(
                    Cache.CreateCacheEntryChangeMonitor(new[] { dependsOnKey })
                );
            }
            lock (Cache)
            {
                Cache.Add(key, o, cachePolicy);
            }
        }

        public virtual IEnumerable<string> AllKeys
        {
            get
            {
                return Cache.Select(x => x.Key);
            }
        }
    }
}
