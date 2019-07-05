using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApi.OutputCache.Core.Cache;

namespace WebApi.OutputCache.V2.Tests
{
    public class SimpleCacheForTests : IApiOutputCache
    {
        private Dictionary<string, object> _cachedItems;

        public SimpleCacheForTests() {
            _cachedItems = new Dictionary<string, object>();
        }

        public virtual void RemoveStartsWith(string key)
        {
            throw new NotImplementedException();
        }

        public virtual T Get<T>(string key) where T : class
        {
            var o = _cachedItems[key] as T;
            return o;
        }

        [Obsolete("Use Get<T> instead")]
        public virtual object Get(string key)
        {
            return _cachedItems[key];
        }

        public virtual void Remove(string key)
        {
            _cachedItems.Remove(key);
        }

        public virtual bool Contains(string key)
        {
            return _cachedItems.ContainsKey(key);
        }

        public virtual void Add(string key, object o, DateTimeOffset expiration, string dependsOnKey = null, TimeSpan slidingExpiration = default(TimeSpan), bool slide = false)
        {
            _cachedItems.Add(key, o);
        }

        public virtual IEnumerable<string> AllKeys
        {
            get
            {
                return _cachedItems.Keys;
            }
        }
    }
}
