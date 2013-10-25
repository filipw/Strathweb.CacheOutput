using System;
using System.Net.Http;
using System.Web.Http;
using Moq;
using NUnit.Framework;
using WebApi.OutputCache.Core.Cache;

namespace WebApi.OutputCache.V2.Tests
{
    [TestFixture]
    public class ConfigurationTests
    {
        private HttpServer _server;
        private string _url = "http://www.strathweb.com/api/sample/";
        private Mock<IApiOutputCache> _cache;

        [Test]
        public void cache_singleton_in_pipeline()
        {
            _cache = new Mock<IApiOutputCache>();

            var conf = new HttpConfiguration();
            conf.CacheOutputConfiguration().RegisterCacheOutputProvider(() => _cache.Object);

            conf.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            _server = new HttpServer(conf);

            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_c100_s100").Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_c100_s100:application/json")), Times.Exactly(2));

            var result2 = client.GetAsync(_url + "Get_c100_s100").Result;
            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_c100_s100:application/json")), Times.Exactly(4));

            _server.Dispose();
        }

        [Test]
        public void cache_singleton()
        {
            var cache = new MemoryCacheDefault();

            var conf = new HttpConfiguration();
            conf.CacheOutputConfiguration().RegisterCacheOutputProvider(() => cache);

            object cache1;
            conf.Properties.TryGetValue(typeof(IApiOutputCache), out cache1);

            object cache2;
            conf.Properties.TryGetValue(typeof(IApiOutputCache), out cache2);

            Assert.AreSame(((Func<IApiOutputCache>)cache1)(), ((Func<IApiOutputCache>)cache2)());
        }

        [Test]
        public void cache_instance()
        {
            var conf = new HttpConfiguration();
            conf.CacheOutputConfiguration().RegisterCacheOutputProvider(() => new MemoryCacheDefault());

            object cache1;
            conf.Properties.TryGetValue(typeof(IApiOutputCache), out cache1);

            object cache2;
            conf.Properties.TryGetValue(typeof(IApiOutputCache), out cache2);

            Assert.AreNotSame(((Func<IApiOutputCache>)cache1)(), ((Func<IApiOutputCache>)cache2)());
        }
    }
}