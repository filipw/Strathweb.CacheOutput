using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace WebApi.OutputCache.Core.Cache
{
    public class MemoryCacheDefault : IApiOutputCache
    {
        private static readonly MemoryCache Cache = MemoryCache.Default;

        private static void RemoveStartsWith(string key)
        {
            lock (Cache)
            {
                Cache.Remove(key);
            }
        }

        private static T Get<T>(string key) where T : class
        {
            var o = Cache.Get(key) as T;
            return o;
        }

        private static void Remove(string key)
        {
            lock (Cache)
            {
                Cache.Remove(key);
            }
        }

        private static bool Contains(string key)
        {
            return Cache.Contains(key);
        }

        private static void Add(string key, object o, DateTimeOffset expiration, string dependsOnKey = null)
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

        public virtual Task<IEnumerable<string>> AllKeysAsync
        {
            get
            {
                return Task.FromResult(Cache.Select(x => x.Key));
            }
        }

        public virtual Task RemoveStartsWithAsync(string key)
        {
            return Task.Run(() => RemoveStartsWith(key));
        }

        public virtual Task<T> GetAsync<T>(string key) where T : class
        {
            return Task.FromResult(Get<T>(key));
        }

        public virtual Task RemoveAsync(string key)
        {
            return Task.Run(() => Remove(key));
        }

        public virtual Task<bool> ContainsAsync(string key)
        {
            return Task.FromResult(Contains(key));
        }

        public virtual Task AddAsync(string key, object value, DateTimeOffset expiration, string dependsOnKey = null)
        {
            return Task.Run(() => Add(key, value, expiration, dependsOnKey));
        }
    }
}
