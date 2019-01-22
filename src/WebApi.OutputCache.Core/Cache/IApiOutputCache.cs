using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.OutputCache.Core.Cache
{
    public interface IApiOutputCache
    {
        Task RemoveStartsWithAsync(string key);

        Task<T> GetAsync<T>(string key) where T : class;

        Task RemoveAsync(string key);

        Task<bool> ContainsAsync(string key);

        Task AddAsync(string key, object value, DateTimeOffset expiration, string dependsOnKey = null);

        Task<IEnumerable<string>> AllKeysAsync { get; }
    }
}