using System;
using NUnit.Framework;
using WebApi.OutputCache.Core.Cache;

namespace WebApi.OutputCache.Core.Tests
{
    [TestFixture]
    public class MemoryCacheDefaultTests
    {
        [Test]
        public async void returns_all_keys_in_cache()
        {
            IApiOutputCache cache = new MemoryCacheDefault();
            await cache.AddAsync("base", "abc" , DateTime.Now.AddSeconds(60));
            await cache.AddAsync("key1", "abc", DateTime.Now.AddSeconds(60), "base");
            await cache.AddAsync("key2", "abc", DateTime.Now.AddSeconds(60), "base");
            await cache.AddAsync("key3", "abc", DateTime.Now.AddSeconds(60), "base");

            var result = await cache.AllKeysAsync;

            CollectionAssert.AreEquivalent(new[] { "base", "key1", "key2", "key3" }, result);
        }

        [Test]
        public async void remove_startswith_cascades_to_all_dependencies()
        {
            IApiOutputCache cache = new MemoryCacheDefault();
            await cache.AddAsync("base", "abc", DateTime.Now.AddSeconds(60));
            await cache.AddAsync("key1","abc", DateTime.Now.AddSeconds(60), "base");
            await cache.AddAsync("key2", "abc", DateTime.Now.AddSeconds(60), "base");
            await cache.AddAsync("key3", "abc", DateTime.Now.AddSeconds(60), "base");
            Assert.IsNotNull(cache.GetAsync<string>("key1"));
            Assert.IsNotNull(cache.GetAsync<string>("key2"));
            Assert.IsNotNull(cache.GetAsync<string>("key3"));

            await cache.RemoveStartsWithAsync("base");

            Assert.IsNull(cache.GetAsync<string>("base").Result);
            Assert.IsNull(cache.GetAsync<string>("key1").Result);
            Assert.IsNull(cache.GetAsync<string>("key2").Result);
            Assert.IsNull(cache.GetAsync<string>("key3").Result);
        }
    }
}
