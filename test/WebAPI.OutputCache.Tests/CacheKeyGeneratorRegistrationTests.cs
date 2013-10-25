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
    public class CacheKeyGeneratorRegistrationTests
    {
        private HttpServer _server;
        private string _url = "http://www.strathweb.com/api/";
        private Mock<IApiOutputCache> _cache;
        private Mock<ICacheKeyGenerator> _keyGenerator;

        [SetUp]
        public void init()
        {
            Thread.CurrentPrincipal = null;

            _cache = new Mock<IApiOutputCache>();
            _keyGenerator = new Mock<ICacheKeyGenerator>();

            var conf = new HttpConfiguration();
            
            var builder = new ContainerBuilder();
            builder.RegisterInstance(_cache.Object);

            conf.DependencyResolver = new AutofacWebApiDependencyResolver(builder.Build());
            conf.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            _server = new HttpServer(conf);
        }

        [Test]
        public void registered_default_is_used()
        {
            _server.Configuration.CacheOutputConfiguration().RegisterDefaultCacheKeyGeneratorProvider(() => _keyGenerator.Object);

            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "sample/Get_c100_s100").Result;

            _keyGenerator.VerifyAll();
        }

        [Test]
        public void last_registered_default_is_used()
        {
            _server.Configuration.CacheOutputConfiguration().RegisterDefaultCacheKeyGeneratorProvider(() => { 
                                                                                                                Assert.Fail("First registration should have been overwritten");
                                                                                                                return null;
            });
            _server.Configuration.CacheOutputConfiguration().RegisterDefaultCacheKeyGeneratorProvider(() => _keyGenerator.Object);

            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "sample/Get_c100_s100").Result;

            _keyGenerator.VerifyAll();
        }

        [Test]
        public void specific_registration_does_not_affect_default()
        {
            _server.Configuration.CacheOutputConfiguration().RegisterDefaultCacheKeyGeneratorProvider(() => _keyGenerator.Object);
            _server.Configuration.CacheOutputConfiguration().RegisterCacheKeyGeneratorProvider(() => new FailCacheKeyGenerator());

            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "sample/Get_c100_s100").Result;

            _keyGenerator.VerifyAll();
        }

        [Test]
        public void selected_generator_with_internal_registration_is_used()
        {
            _server.Configuration.CacheOutputConfiguration().RegisterCacheKeyGeneratorProvider(() => new InternalRegisteredCacheKeyGenerator("internal"));

            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "cachekey/get_internalregistered").Result;

            _cache.Verify(s => s.Add(It.Is<string>(x => x == "internal"), It.IsAny<byte[]>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(100)), It.Is<string>(x => x == "cachekey-get_internalregistered")), Times.Once());
        }

        [Test]
        public void custom_unregistered_cache_key_generator_called()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "cachekey/get_unregistered").Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "unregistered")), Times.Once());
        }

        #region Helper classes
        private class FailCacheKeyGenerator : ICacheKeyGenerator
        {
            public string MakeCacheKey(HttpActionContext context, MediaTypeHeaderValue mediaType, bool excludeQueryString = false)
            {
                Assert.Fail("This cache key generator should never be invoked");
                return "fail";
            }
        }

        public class InternalRegisteredCacheKeyGenerator : ICacheKeyGenerator
        {
            private readonly string _key;

            public InternalRegisteredCacheKeyGenerator(string key)
            {
                _key = key;
            }

            public string MakeCacheKey(HttpActionContext context, MediaTypeHeaderValue mediaType, bool excludeQueryString = false)
            {
                return _key;
            }
        }
        #endregion
    }
}