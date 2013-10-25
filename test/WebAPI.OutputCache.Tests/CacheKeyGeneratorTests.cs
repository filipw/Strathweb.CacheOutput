using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using Autofac;
using Autofac.Integration.WebApi;
using Moq;
using NUnit.Framework;
using WebApi.OutputCache.Core.Cache;

namespace WebAPI.OutputCache.Tests
{
    [TestFixture]
    class CacheKeyGeneratorTests
    {
        public class CustomCacheKeyGenerator : ICacheKeyGenerator
        {
            public string MakeCacheKey(HttpActionContext context, MediaTypeHeaderValue mediaType, bool excludeQueryString = false)
            {
                return "custom_key";
            }
        }

        private HttpServer _server;
        private string _url = "http://www.strathweb.com/api/";
        private Mock<IApiOutputCache> _cache;
        private Mock<ICacheKeyGenerator> _keyGeneratorA;
        private CustomCacheKeyGenerator _keyGeneratorB;

        [SetUp]
        public void init()
        {
            Thread.CurrentPrincipal = null;

            _cache = new Mock<IApiOutputCache>();
            _keyGeneratorA = new Mock<ICacheKeyGenerator>();
            _keyGeneratorB = new CustomCacheKeyGenerator();

            var conf = new HttpConfiguration();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(_cache.Object);
            // this should become the default cache key generator
            builder.RegisterInstance(_keyGeneratorA.Object).As<ICacheKeyGenerator>();
            builder.RegisterInstance(_keyGeneratorB);

            conf.DependencyResolver = new AutofacWebApiDependencyResolver(builder.Build());
            conf.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            _server = new HttpServer(conf);
        }

        [Test]
        public void custom_default_cache_key_generator_called_and_key_used()
        {
            var client = new HttpClient(_server);
            _keyGeneratorA.Setup(k => k.MakeCacheKey(It.IsAny<HttpActionContext>(), It.IsAny<MediaTypeHeaderValue>(), It.IsAny<bool>()))
                .Returns("keykeykey")
                .Verifiable("Key generator was never called");
            // use the samplecontroller to show that no changes are required to existing code
            var result = client.GetAsync(_url + "sample/Get_c100_s100").Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "keykeykey")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "keykeykey"), It.IsAny<byte[]>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(100)), It.Is<string>(x => x == "sample-get_c100_s100")), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "keykeykey:response-ct"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(100)), It.Is<string>(x => x == "sample-get_c100_s100")), Times.Once());
            
            _keyGeneratorA.VerifyAll();
        }

        [Test]
        public void custom_cache_key_generator_called()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "cachekey/get_custom_key").Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "custom_key")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "custom_key"), It.IsAny<byte[]>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(100)), It.Is<string>(x => x == "cachekey-get_custom_key")), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "custom_key:response-ct"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(100)), It.Is<string>(x => x == "cachekey-get_custom_key")), Times.Once());
        }
    }
}
