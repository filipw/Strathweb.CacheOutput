using NUnit.Framework;
using System;

namespace WebApi.OutputCache.V2.Tests
{
    [TestFixture]
    public class DefaultCacheKeyGeneratorTests : CacheKeyGenerationTestsBase<DefaultCacheKeyGenerator>
    {
        protected override DefaultCacheKeyGenerator BuildCacheKeyGenerator()
        {
            return new DefaultCacheKeyGenerator();
        }

        [Test]
        public void NoParametersIncludeQueryString_ShouldReturnBaseKeyAndQueryStringAndMediaTypeConcatenated()
        {
            var cacheKey = cacheKeyGenerator.MakeCacheKey(context, mediaType, false);

            AssertCacheKeysBasicFormat(cacheKey);
            Assert.AreEqual(String.Format("{0}-{1}:{2}", BaseCacheKey, requestUri.Query.Substring(1), mediaType), cacheKey,
                "Key does not match expected <BaseKey>-<QueryString>:<MediaType>");
        }

        [Test]
        public void NoParametersExcludeQueryString_ShouldReturnBaseKeyAndMediaTypeConcatenated()
        {
            var cacheKey = cacheKeyGenerator.MakeCacheKey(context, mediaType, true);

            AssertCacheKeysBasicFormat(cacheKey);
            Assert.AreEqual(String.Format("{0}:{1}", BaseCacheKey, mediaType), cacheKey,
                "Key does not match expected <BaseKey>:<MediaType>");
        }

        [Test]
        public void WithParametersIncludeQueryString_ShouldReturnBaseKeyAndArgumentsAndQueryStringAndMediaTypeConcatenated()
        {
            AddActionArgumentsToContext();
            var cacheKey = cacheKeyGenerator.MakeCacheKey(context, mediaType, false);

            AssertCacheKeysBasicFormat(cacheKey);
            Assert.AreEqual(String.Format("{0}-{1}&{2}:{3}", BaseCacheKey, FormatActionArgumentsForKeyAssertion(), requestUri.Query.Substring(1), mediaType), cacheKey,
                "Key does not match expected <BaseKey>-<Arguments>&<QueryString>:<MediaType>");
        }

        [Test]
        public void WithParametersExcludeQueryString_ShouldReturnBaseKeyAndArgumentsAndMediaTypeConcatenated()
        {
            AddActionArgumentsToContext();
            var cacheKey = cacheKeyGenerator.MakeCacheKey(context, mediaType, true);

            AssertCacheKeysBasicFormat(cacheKey);
            Assert.AreEqual(String.Format("{0}-{1}:{2}", BaseCacheKey, FormatActionArgumentsForKeyAssertion(), mediaType), cacheKey,
                "Key does not match expected <BaseKey>-<Arguments>:<MediaType>");
        }
    }
}
