using System;
using System.Collections;

namespace WebAPI.OutputCache.Cache
{
    public interface IApiOutputCache
    {
        void RemoveStartsWith(string key);
        T Get<T>(string key) where T : class;
        object Get(string key);
        void Remove(string key);
        bool Contains(string key);
        void Add(string key, object o, DateTimeOffset expiration, string dependsOnKey = null);
    }
}