using System;

using NUnit.Framework;
using WebApi.OutputCache.Core.Cache;

namespace WebApi.OutputCache.Core.Tests
{
    [TestFixture]
    public class MemoryCacheDefaultTests
    {
        private IApiOutputCache _cache;

        [SetUp]
        public void Setup()
        {
            this._cache = new MemoryCacheDefault();
            EmptyCache();
        }

        [Test]
        public void returns_all_keys_in_cache()
        {
            this._cache.Add("base", "abc", DateTime.Now.AddSeconds(60));
            this._cache.Add("key1", "abc", DateTime.Now.AddSeconds(60), "base");
            this._cache.Add("key2", "abc", DateTime.Now.AddSeconds(60), "base");
            this._cache.Add("key3", "abc", DateTime.Now.AddSeconds(60), "base");

            var result = this._cache.AllKeys;

            CollectionAssert.AreEquivalent(new[] { "base", "key1", "key2", "key3" }, result);
        }

        [Test]
        public void remove_startswith_cascades_to_all_dependencies()
        {
            this._cache.Add("base", "abc", DateTime.Now.AddSeconds(60));
            this._cache.Add("key1","abc", DateTime.Now.AddSeconds(60), "base");
            this._cache.Add("key2", "abc", DateTime.Now.AddSeconds(60), "base");
            this._cache.Add("key3", "abc", DateTime.Now.AddSeconds(60), "base");
            Assert.IsNotNull(this._cache.Get("key1"));
            Assert.IsNotNull(this._cache.Get("key2"));
            Assert.IsNotNull(this._cache.Get("key3"));

            this._cache.RemoveStartsWith("base");

            Assert.IsNull(this._cache.Get("base"));
            Assert.IsNull(this._cache.Get("key1"));
            Assert.IsNull(this._cache.Get("key2"));
            Assert.IsNull(this._cache.Get("key3"));
        }

        [Test]
        public void find_keys_starting_with_a_prefix()
        {
            this._cache.Add("abc1", "abc", DateTime.Now.AddSeconds(60));
            this._cache.Add("abc2", "abc", DateTime.Now.AddSeconds(60));
            this._cache.Add("abc3", "abc", DateTime.Now.AddSeconds(60));
            this._cache.Add("edf1", "abc", DateTime.Now.AddSeconds(60));
            this._cache.Add("edf2", "abc", DateTime.Now.AddSeconds(60));

            var keys = this._cache.FindKeysStartingWith("abc");
            
            CollectionAssert.AreEquivalent(new[] { "abc1", "abc2", "abc3" }, keys);
        }

        private void EmptyCache()
        {
            foreach (var key in this._cache.AllKeys)
            {
                this._cache.Remove(key);
            }
        }
    }
}
