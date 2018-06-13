using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http.Controllers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace WebApi.OutputCache.V2.Tests
{
    public class DefaultCacheKeyGeneratorPostRequestTests
    {
        private readonly Uri _requestUri = new Uri("http://localhost:8080/cacheKeyGeneration");
        private MediaTypeHeaderValue _mediaType;
        private ICacheKeyGenerator _cacheKeyGenerator;
        private string _baseCacheKey;
        private HttpActionContext PrepareContext(HttpContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _requestUri.AbsoluteUri) { Content = content };
            return PrepareContext(request);
        }
        private HttpActionContext PrepareContext(HttpRequestMessage request)
        {
            var controllerType = typeof(TestControllers.CacheKeyGenerationController);
            var actionMethodInfo = controllerType.GetMethod("Post");
            var controllerDescriptor = new HttpControllerDescriptor { ControllerType = controllerType };
            var actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, actionMethodInfo);

            return new HttpActionContext(new HttpControllerContext { ControllerDescriptor = controllerDescriptor, Request = request }, actionDescriptor);
        }
        [SetUp]
        public void Setup()
        {
            _mediaType = new MediaTypeHeaderValue("application/json");
            _cacheKeyGenerator = new DefaultCacheKeyGenerator();
            _baseCacheKey = new CacheOutputConfiguration(null).MakeBaseCachekey((TestControllers.CacheKeyGenerationController c) => c.Post(null));
        }
        private void AssertCacheKeysBasicFormat(string cacheKey)
        {
            Assert.IsNotNull(cacheKey);
            StringAssert.StartsWith(_baseCacheKey, cacheKey, "Key does not start with BaseKey");
            StringAssert.EndsWith(_mediaType.ToString(), cacheKey, "Key does not end with MediaType");
        }

        [Test]
        public void POST_NoParametersExcludeQueryString_ShouldReturnBaseKey()
        {
            var cacheKey = _cacheKeyGenerator.MakeCacheKey(PrepareContext(content: null), _mediaType, true);
            AssertCacheKeysBasicFormat(cacheKey);
            var baseKey = $"{_baseCacheKey}:{_mediaType}";
            Assert.AreEqual(baseKey, cacheKey, "Key does not match expected <BaseKey>-<QueryString>:<MediaType>");
        }

        [Test]
        public void POST_IncludeQueryString_ShouldReturnBaseKey()
        {
            var cacheKey = _cacheKeyGenerator.MakeCacheKey(PrepareContext(content: null), _mediaType);
            AssertCacheKeysBasicFormat(cacheKey);
        }

        [Test]
        public void POST_IncludeQueryString_ShouldReturnAddPostParametersToKey()
        {
            var content = new
            {
                key1 = "value1",
                key2 = 33,
            };
            var cacheKey = _cacheKeyGenerator.MakeCacheKey(PrepareContext(new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json")), _mediaType);
            AssertCacheKeysBasicFormat(cacheKey);
            Assert.IsTrue(cacheKey.Contains("key1=value1"));
            Assert.IsTrue(cacheKey.Contains("key2=33"));
        }

        [Test]
        public void POST_ShouldIncludeQueryAndPostBody()
        {
            var content = new
            {
                key1 = "value1",
                key2 = 33,
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _requestUri + "?getParameter=true")
            {
                Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json")
            };
            var cacheKey = _cacheKeyGenerator.MakeCacheKey(PrepareContext(request), _mediaType);
            AssertCacheKeysBasicFormat(cacheKey);
            Assert.IsTrue(cacheKey.Contains("getParameter=true"));
            Assert.IsTrue(cacheKey.Contains("key1=value1"));
            Assert.IsTrue(cacheKey.Contains("key2=33"));
        }

        [Test]
        public void PUT_NoParametersExcludeQueryString_ShouldReturnBaseKey()
        {
            var request = new HttpRequestMessage(HttpMethod.Put, _requestUri);
            var cacheKey = _cacheKeyGenerator.MakeCacheKey(PrepareContext(request), _mediaType, true);
            AssertCacheKeysBasicFormat(cacheKey);
            var baseKey = $"{_baseCacheKey}:{_mediaType}";
            Assert.AreEqual(baseKey, cacheKey, "Key does not match expected <BaseKey>-<QueryString>:<MediaType>");
        }

        [Test]
        public void PUT_IncludeQueryString_ShouldReturnBaseKey()
        {
            var request = new HttpRequestMessage(HttpMethod.Put, _requestUri);
            var cacheKey = _cacheKeyGenerator.MakeCacheKey(PrepareContext(request), _mediaType);
            AssertCacheKeysBasicFormat(cacheKey);
        }

        [Test]
        public void PUT_IncludeQueryString_ShouldReturnAddPostParametersToKey()
        {
            var content = new
            {
                key1 = "value1",
                key2 = 33,
            };
            var request = new HttpRequestMessage(HttpMethod.Put, _requestUri)
            {
                Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json")
            };
            var cacheKey = _cacheKeyGenerator.MakeCacheKey(PrepareContext(request), _mediaType);
            AssertCacheKeysBasicFormat(cacheKey);
            Assert.IsTrue(cacheKey.Contains("key1=value1"));
            Assert.IsTrue(cacheKey.Contains("key2=33"));
        }

        [Test]
        public void PUT_ShouldIncludeQueryAndPostBody()
        {
            var content = new
            {
                key1 = "value1",
                key2 = 33,
            };
            var request = new HttpRequestMessage(HttpMethod.Put, _requestUri + "?id=1")
            {
                Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json")
            };
            var cacheKey = _cacheKeyGenerator.MakeCacheKey(PrepareContext(request), _mediaType);
            AssertCacheKeysBasicFormat(cacheKey);
            Assert.IsTrue(cacheKey.Contains("id=1"));
            Assert.IsTrue(cacheKey.Contains("key1=value1"));
            Assert.IsTrue(cacheKey.Contains("key2=33"));
        }
    }
}