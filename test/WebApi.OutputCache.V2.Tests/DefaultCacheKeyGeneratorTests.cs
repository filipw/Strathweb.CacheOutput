using NUnit.Framework;
using System;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;

namespace WebApi.OutputCache.V2.Tests
{
    [TestFixture]
    public class DefaultCacheKeyGeneratorTests
    {
        private const string ArgumentKey = "filterExpression";
        private const string ArgumentValue = "val";
        private HttpActionContext context;
        private MediaTypeHeaderValue mediaType;
        private Uri requestUri;
        private DefaultCacheKeyGenerator cacheKeyGenerator;
        private string BaseCacheKey;

        [SetUp]
        public void Setup()
        {
            requestUri = new Uri("http://localhost:8080/cacheKeyGeneration?filter=val");
            var controllerType = typeof(TestControllers.CacheKeyGenerationController);
            var actionMethodInfo = controllerType.GetMethod("Get");
            var controllerDescriptor = new HttpControllerDescriptor() { ControllerType = controllerType };
            var actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, actionMethodInfo);
            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, requestUri.AbsoluteUri);

            context = new HttpActionContext(
                new HttpControllerContext() { ControllerDescriptor = controllerDescriptor, Request = request },
                actionDescriptor
            );
            mediaType = new MediaTypeHeaderValue("application/json");

            cacheKeyGenerator = new DefaultCacheKeyGenerator();
            BaseCacheKey = new CacheOutputConfiguration(null).MakeBaseCachekey((TestControllers.CacheKeyGenerationController c) => c.Get(String.Empty));
        }

        private void AssertCacheKeysBasicFormat(string cacheKey)
        {
            Assert.IsNotNull(cacheKey);
            StringAssert.StartsWith(BaseCacheKey, cacheKey, "Key does not start with BaseKey");
            StringAssert.EndsWith(mediaType.ToString(), cacheKey, "Key does not end with MediaType");
        }

        private void AddActionArgumentsToContext() 
        {
            context.ActionArguments.Add(ArgumentKey, ArgumentValue);
        }

        [Test]
        public void NoParametersIncludeQueryString_ShouldReturnBaseKeyAndQueryStringAndMediaTypeConcatenated()
        {
            var cacheKey = cacheKeyGenerator.MakeCacheKey(context, mediaType, false);

            AssertCacheKeysBasicFormat(cacheKey);
            Assert.AreEqual(String.Format("{0}-{1}:{2}", BaseCacheKey, requestUri.Query.Substring(1), mediaType), cacheKey, "Key does not match expected <BaseKey>-<QueryString>:<MediaType>");
        }

        [Test]
        public void NoParametersExcludeQueryString_ShouldReturnBaseKeyAndMediaTypeConcatenated()
        {
            var cacheKey = cacheKeyGenerator.MakeCacheKey(context, mediaType, true);

            AssertCacheKeysBasicFormat(cacheKey);
            Assert.AreEqual(String.Format("{0}:{1}", BaseCacheKey, mediaType), cacheKey, "Key does not match expected <BaseKey>:<MediaType>");
        }

        [Test]
        public void WithParametersIncludeQueryString_ShouldReturnBaseKeyAndArgumentsAndQueryStringAndMediaTypeConcatenated()
        {
            AddActionArgumentsToContext();
            var cacheKey = cacheKeyGenerator.MakeCacheKey(context, mediaType, false);

            AssertCacheKeysBasicFormat(cacheKey);
            Assert.AreEqual(String.Format("{0}-{1}={2}&{3}:{4}", BaseCacheKey, ArgumentKey, ArgumentValue, requestUri.Query.Substring(1), mediaType), cacheKey, "Key does not match expected <BaseKey>-<Arguments>&<QueryString>:<MediaType>");
        }

        [Test]
        public void WithParametersExcludeQueryString_ShouldReturnBaseKeyAndArgumentsAndMediaTypeConcatenated()
        {
            AddActionArgumentsToContext();
            var cacheKey = cacheKeyGenerator.MakeCacheKey(context, mediaType, true);

            AssertCacheKeysBasicFormat(cacheKey);
            Assert.AreEqual(String.Format("{0}-{1}={2}:{3}", BaseCacheKey, ArgumentKey, ArgumentValue, mediaType), cacheKey, "Key does not match expected <BaseKey>-<Arguments>:<MediaType>");
        }
    }
}
