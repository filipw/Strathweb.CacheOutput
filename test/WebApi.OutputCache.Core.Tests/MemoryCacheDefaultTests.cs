using System;
using System.Dynamic;

using NUnit.Framework;
using WebApi.OutputCache.Core.Cache;

namespace WebApi.OutputCache.Core.Tests
{
    [TestFixture]
    public class MemoryCacheDefaultTests
    {
        private IApiOutputCache cache;
        [SetUp]
        public void Setup()
        {
            cache = new MemoryCacheDefault();
            this.EmptyCache(cache);
        }

        [Test]
        public void returns_all_keys_in_cache()
        {
            cache.Add("base", "abc", DateTime.Now.AddSeconds(60));
            cache.Add("key1", "abc", DateTime.Now.AddSeconds(60), "base");
            cache.Add("key2", "abc", DateTime.Now.AddSeconds(60), "base");
            cache.Add("key3", "abc", DateTime.Now.AddSeconds(60), "base");

            var result = cache.AllKeys;

            CollectionAssert.AreEquivalent(new[] { "base", "key1", "key2", "key3" }, result);
        }

        [Test]
        public void remove_startswith_cascades_to_all_dependencies()
        {
            cache.Add("base", "abc", DateTime.Now.AddSeconds(60));
            cache.Add("key1","abc", DateTime.Now.AddSeconds(60), "base");
            cache.Add("key2", "abc", DateTime.Now.AddSeconds(60), "base");
            cache.Add("key3", "abc", DateTime.Now.AddSeconds(60), "base");
            Assert.IsNotNull(cache.Get("key1"));
            Assert.IsNotNull(cache.Get("key2"));
            Assert.IsNotNull(cache.Get("key3"));

            cache.RemoveStartsWith("base");

            Assert.IsNull(cache.Get("base"));
            Assert.IsNull(cache.Get("key1"));
            Assert.IsNull(cache.Get("key2"));
            Assert.IsNull(cache.Get("key3"));
        }

        [Test]
        public void find_keys_starting_with_a_prefix()
        {
            cache.Add("abc1", "abc", DateTime.Now.AddSeconds(60));
            cache.Add("abc2", "abc", DateTime.Now.AddSeconds(60));
            cache.Add("abc3", "abc", DateTime.Now.AddSeconds(60));
            cache.Add("edf1", "abc", DateTime.Now.AddSeconds(60));
            cache.Add("edf2", "abc", DateTime.Now.AddSeconds(60));

            var keys = cache.FindKeysStartingWith("abc");
            
            CollectionAssert.AreEquivalent(new[] { "abc1", "abc2", "abc3" }, keys);
        }

        private void EmptyCache(IApiOutputCache cache)
        {
            foreach (var key in cache.AllKeys)
            {
                cache.Remove(key);
            }
        }
    }
}
