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
using WebApi.OutputCache.Core;
using WebApi.OutputCache.Core.Cache;

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
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_c100_s100"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(100)), null), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_c100_s100:application/json"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(100)), It.Is<string>(x => x == "sample-get_c100_s100")), Times.Once());
        }

        [Test]
        public void set_cache_to_predefined_value_c100_s0()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_c100_s0").Result;
            
            // NOTE: Should we expect the _cache to not be called at all if the ServerTimeSpan is 0?
            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_c100_s0:application/json")), Times.Once());
            // NOTE: Server timespan is 0, so there should not have been any Add at all.
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_c100_s0"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(100)), null), Times.Never());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_c100_s0:application/json"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(1)), It.Is<string>(x => x == "sample-get_c100_s0")), Times.Never());
        }

        [Test]
        public void not_cache_when_request_not_succes()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_request_httpResponseException_noCache").Result;

            _cache.Verify(s => s.Contains(It.IsAny<string>()), Times.Once());
            _cache.Verify(s => s.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void not_cache_when_request_exception()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_request_exception_noCache").Result;

            _cache.Verify(s => s.Contains(It.IsAny<string>()), Times.Once());
            _cache.Verify(s => s.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void not_cache_add_when_no_content()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_request_noContent").Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_request_nocontent:application/json")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void set_cache_to_predefined_value_respect_formatter_through_accept_header()
        {
            var client = new HttpClient(_server);
            var req = new HttpRequestMessage(HttpMethod.Get, _url + "Get_c100_s100");
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
            var result = client.SendAsync(req).Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_c100_s100:text/xml")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_c100_s100"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(100)), null), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_c100_s100:text/xml"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(100)), It.Is<string>(x => x == "sample-get_c100_s100")), Times.Once());
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
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_c100_s100"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(100)), null), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_c100_s100:text/xml"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(100)), It.Is<string>(x => x == "sample-get_c100_s100")), Times.Exactly(1));
        }

        [Test]
        public void set_cache_dont_exclude_querystring()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_s50_exclude_false/1?xxx=2").Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_s50_exclude_false-id=1&xxx=2:application/json")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_false"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(50)), null), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_false-id=1&xxx=2:application/json"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(50)), It.Is<string>(x => x == "sample-get_s50_exclude_false")), Times.Once());
        }

        [Test]
        public void set_cache_dont_exclude_querystring_duplicate_action_arg_in_querystring_is_still_excluded()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_s50_exclude_false/1?id=1").Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_s50_exclude_false-id=1:application/json")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_false"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(50)), null), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_false-id=1:application/json"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(50)), It.Is<string>(x => x == "sample-get_s50_exclude_false")), Times.Once());
        }

        [Test]
        public void set_cache_do_exclude_querystring()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_s50_exclude_true/1?xxx=1").Result;

            //check
            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_s50_exclude_true-id=1:application/json")), Times.Exactly(2));

            //base
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_true"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(50)), null), Times.Once());

            //actual
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_true-id=1:application/json"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(50)), It.Is<string>(x => x == "sample-get_s50_exclude_true")), Times.Once());
        }

        [Test]
        public void set_cache_do_exclude_querystring_do_not_exclude_action_arg_even_if_passed_as_querystring()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_s50_exclude_true?id=1").Result;

            //check
            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_s50_exclude_true-id=1:application/json")), Times.Exactly(2));

            //base
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_true"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(50)), null), Times.Once());

            //actual
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_true-id=1:application/json"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(50)), It.Is<string>(x => x == "sample-get_s50_exclude_true")), Times.Once());
        }

        [Test]
        public void callback_at_the_end_is_excluded_querystring()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_s50_exclude_fakecallback?id=1&callback=abc").Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback-id=1:application/json")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(50)), null), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback-id=1:application/json"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(50)), It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback")), Times.Once());
        }

        [Test]
        public void callback_at_the_beginning_is_excluded_querystring()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_s50_exclude_fakecallback?callback=abc&id=1").Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback-id=1:application/json")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(50)), null), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback-id=1:application/json"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(50)), It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback")), Times.Once());
        }

        [Test]
        public void callback_in_the_middle_is_excluded_querystring()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_s50_exclude_fakecallback?de=xxx&callback=abc&id=1").Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback-id=1&de=xxx:application/json")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(50)), null), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback-id=1&de=xxx:application/json"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(50)), It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback")), Times.Once());
        }

        [Test]
        public void callback_alone_is_excluded_querystring()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_s50_exclude_fakecallback?callback=abc").Result;

            _cache.Verify(s => s.Contains(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback:application/json")), Times.Exactly(2));
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(50)), null), Times.Once());
            _cache.Verify(s => s.Add(It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback:application/json"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(50)), It.Is<string>(x => x == "sample-get_s50_exclude_fakecallback")), Times.Once());
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
                  .Returns(@"""abc""");

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


        //[Test]
        //public void must_add_querystring_to_cache_params()
        //{
        //    var client = new HttpClient(_server);
        //    var result = client.GetAsync(_url + "cachekey/get_custom_key").Result;

        //    _cache.Verify(s => s.Contains(It.Is<string>(x => x == "custom_key")), Times.Exactly(2));
        //    _cache.Verify(s => s.Add(It.Is<string>(x => x == "custom_key"), It.IsAny<byte[]>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(100)), It.Is<string>(x => x == "cachekey-get_custom_key")), Times.Once());
        //    _cache.Verify(s => s.Add(It.Is<string>(x => x == "custom_key:response-ct"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x <= DateTime.Now.AddSeconds(100)), It.Is<string>(x => x == "cachekey-get_custom_key")), Times.Once());
        //}

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