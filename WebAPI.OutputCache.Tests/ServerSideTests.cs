using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Moq;
using NUnit.Framework;
using WebAPI.OutputCache.Cache;

namespace WebAPI.OutputCache.Tests
{
    [TestFixture]
    public class ServerSideTests
    {
        private HttpServer _server;
        private string _url = "http://www.strathweb.com/api/sample/";
        private Mock<IApiOutputCache> _cache;

        [SetUp]
        public void init()
        {
            Thread.CurrentPrincipal = null;

            _cache = new Mock<IApiOutputCache>();

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
        public void set_cache_to_predefined_value()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_c100_s100").Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_c100_s100:application/json")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_c100_s100"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(100)), null), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_c100_s100:application/json"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(100)), It.Is<string>(x => x == "sample-get_c100_s100")), Times.Once());
        }

        [Test]
        public void set_cache_to_predefined_value_respect_formatter_through_accept_header()
        {
            var client = new HttpClient(_server);
            var req = new HttpRequestMessage(HttpMethod.Get, _url + "Get_c100_s100");
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
            var result = client.SendAsync(req).Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_c100_s100:text/xml")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_c100_s100"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(100)), null), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_c100_s100:text/xml"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(100)), It.Is<string>(x => x == "sample-get_c100_s100")), Times.Once());
        }

        [Test]
        public void set_cache_to_predefined_value_respect_formatter_through_content_type()
        {
            var client = new HttpClient(_server);
            var req = new HttpRequestMessage(HttpMethod.Get, _url + "Get_c100_s100");
            req.Content = new StringContent("");
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
            var result = client.SendAsync(req).Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_c100_s100:text/xml")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_c100_s100"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(100)), null), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_c100_s100:text/xml"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(100)), It.Is<string>(x => x == "sample-get_c100_s100")), Times.Exactly(1));
        }

        [Test]
        public void set_cache_dont_exclude_querystring()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_s50_exclude_false?id=1").Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_s50_exclude_false-id=1:application/json")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_false"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(50)), null), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_false-id=1:application/json"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(50)), It.Is<string>(x => x == "sample-get_s50_exclude_false")), Times.Once());
        }

        [Test]
        public void set_cache_do_exclude_querystring()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_s50_exclude_true?id=1").Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_s50_exclude_true:application/json")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_true"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(50)), null), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_true:application/json"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(50)), It.Is<string>(x => x == "sample-get_s50_exclude_true")), Times.Once());
        }

        [Test]
        public void callback_at_the_end_is_excluded_querystring()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_s50_exclude_fakecallback?id=1&callback=abc").Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback-id=1:application/json")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(50)), null), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback-id=1:application/json"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(50)), It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback")), Times.Once());
        }

        [Test]
        public void callback_at_the_beginning_is_excluded_querystring()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_s50_exclude_fakecallback?callback=abc&id=1").Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback-id=1:application/json")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(50)), null), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback-id=1:application/json"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(50)), It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback")), Times.Once());
        }

        [Test]
        public void callback_in_the_middle_is_excluded_querystring()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_s50_exclude_fakecallback?de=xxx&callback=abc&id=1").Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback-id=1&de=xxx:application/json")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(50)), null), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback-id=1&de=xxx:application/json"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(50)), It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback")), Times.Once());
        }

        [Test]
        public void callback_alone_is_excluded_querystring()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_s50_exclude_fakecallback?callback=abc").Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback:application/json")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(50)), null), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback:application/json"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(50)), It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback")), Times.Once());
        }

        [Test]
        public void no_caching_if_user_authenticated_and_flag_set_to_off()
        {
            SetCurrentThreadIdentity("Filip");
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_s50_c50_anonymousonly").Result;

            Assert.True(result.IsSuccessStatusCode);
            Assert.IsNull(result.Headers.CacheControl);
            _cache.Verify(s => s.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void etag_match_304_if_none_match()
        {
            _cache.Setup(x => x.Contains(It.Is<string>(i => i.Contains("etag_match_304")))).Returns(true);
            _cache.Setup(x => x.Get(It.Is<string>(i => i.Contains("etag_match_304") && i.Contains(Constants.EtagKey))))
                  .Returns((object)new EntityTagHeaderValue(@"""abc"""));

            var client = new HttpClient(_server);
            var req = new HttpRequestMessage(HttpMethod.Get, _url + "etag_match_304");
            req.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(@"""abc"""));
            var result = client.SendAsync(req).Result;

            Assert.AreEqual(TimeSpan.FromSeconds(50), result.Headers.CacheControl.MaxAge);
            Assert.IsFalse(result.Headers.CacheControl.MustRevalidate);
            Assert.AreEqual(HttpStatusCode.NotModified, result.StatusCode);
        }

        [Test]
        public void etag_not_match_304_if_none_match()
        {
            _cache.Setup(x => x.Contains(It.Is<string>(i => i.Contains("etag_match_304")))).Returns(true);
            _cache.Setup(x => x.Get(It.Is<string>(i => i.Contains("etag_match_304") && i.Contains(Constants.EtagKey))))
                  .Returns((object)new EntityTagHeaderValue(@"""abcdef"""));

            var client = new HttpClient(_server);
            var req = new HttpRequestMessage(HttpMethod.Get, _url + "etag_match_304");
            req.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(@"""abc"""));
            var result = client.SendAsync(req).Result;

            Assert.AreEqual(TimeSpan.FromSeconds(50), result.Headers.CacheControl.MaxAge);
            Assert.IsFalse(result.Headers.CacheControl.MustRevalidate);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        [TearDown]
        public void fixture_dispose()
        {
            if (_server != null) _server.Dispose();
        }

        private static void SetCurrentThreadIdentity(string username)
        {
            var customIdentity = new Mock<IIdentity>();
            customIdentity.SetupGet(x => x.IsAuthenticated).Returns(true);
            var threadCurrentPrincipal = new GenericPrincipal(customIdentity.Object, new string[] { "CustomUser" });
            Thread.CurrentPrincipal = threadCurrentPrincipal;
        }
    }
}