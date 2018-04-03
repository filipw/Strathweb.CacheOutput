using NUnit.Framework;
using System;
using System.Security.Principal;

namespace WebApi.OutputCache.V2.Tests
{
    [TestFixture]
    public class PerUserCacheKeyGeneratorTests : CacheKeyGenerationTestsBase<PerUserCacheKeyGenerator>
    {
        private const string UserIdentityName = "SomeUserIDon'tMind";

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            context.RequestContext.Principal = new GenericPrincipal(new GenericIdentity(UserIdentityName), new string[0]);
        }

        protected override PerUserCacheKeyGenerator BuildCacheKeyGenerator()
        {
            return new PerUserCacheKeyGenerator();
        }

        private string FormatUserIdentityForAssertion() 
        {
            return UserIdentityName.ToLower();
        }

        [Test]
        public void NoParametersIncludeQueryString_ShouldReturnBaseKeyAndQueryStringAndUserIdentityAndMediaTypeConcatenated()
        {
            var cacheKey = cacheKeyGenerator.MakeCacheKey(context, mediaType, false);

            AssertCacheKeysBasicFormat(cacheKey);
            Assert.AreEqual(String.Format("{0}-{1}:{2}:{3}", BaseCacheKey, requestUri.Query.Substring(1), FormatUserIdentityForAssertion(), mediaType), cacheKey, 
                "Key does not match expected <BaseKey>-<QueryString>:<UserIdentity>:<MediaType>");
        }

        [Test]
        public void NoParametersExcludeQueryString_ShouldReturnBaseKeyAndUserIdentityAndMediaTypeConcatenated()
        {
            var cacheKey = cacheKeyGenerator.MakeCacheKey(context, mediaType, true);

            AssertCacheKeysBasicFormat(cacheKey);
            Assert.AreEqual(String.Format("{0}:{1}:{2}", BaseCacheKey, FormatUserIdentityForAssertion(), mediaType), cacheKey,
                "Key does not match expected <BaseKey>:<UserIdentity>:<MediaType>");
        }

        [Test]
        public void WithParametersIncludeQueryString_ShouldReturnBaseKeyAndArgumentsAndQueryStringAndUserIdentityAndMediaTypeConcatenated()
        {
            AddActionArgumentsToContext();
            var cacheKey = cacheKeyGenerator.MakeCacheKey(context, mediaType, false);

            AssertCacheKeysBasicFormat(cacheKey);
            Assert.AreEqual(String.Format("{0}-{1}&{2}:{3}:{4}", BaseCacheKey, FormatActionArgumentsForKeyAssertion(), requestUri.Query.Substring(1), FormatUserIdentityForAssertion(), mediaType), cacheKey,
                "Key does not match expected <BaseKey>-<Arguments>&<QueryString>:<UserIdentity>:<MediaType>");
        }

        [Test]
        public void WithParametersExcludeQueryString_ShouldReturnBaseKeyAndArgumentsAndUserIdentityAndMediaTypeConcatenated()
        {
            AddActionArgumentsToContext();
            var cacheKey = cacheKeyGenerator.MakeCacheKey(context, mediaType, true);

            AssertCacheKeysBasicFormat(cacheKey);
            Assert.AreEqual(String.Format("{0}-{1}:{2}:{3}", BaseCacheKey, FormatActionArgumentsForKeyAssertion(), FormatUserIdentityForAssertion(), mediaType), cacheKey,
                "Key does not match expected <BaseKey>-<Arguments>:<UserIdentity>:<MediaType>");
        }
    }
}
