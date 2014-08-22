using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using NUnit.Framework;
using WebApi.OutputCache.Core.Time;

namespace WebAPI.OutputCache.Tests
{
    [TestFixture]
    public class ClientSideTests
    {
        private HttpServer _server;
        private string _url = "http://www.strathweb.com/api/sample/";

        [TestFixtureSetUp]
        public void fixture_init()
        {
            var conf = new HttpConfiguration();
            conf.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            _server = new HttpServer(conf);
        }

        [Test]
        public void maxage_mustrevalidate_false_headers_correct()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_c100_s100").Result;

            Assert.AreEqual(TimeSpan.FromSeconds(100), result.Headers.CacheControl.MaxAge);
            Assert.IsFalse(result.Headers.CacheControl.MustRevalidate);
        }

        [Test]
        public void no_cachecontrol_when_clienttimeout_is_zero()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_c0_s100").Result;

            Assert.IsNull(result.Headers.CacheControl);
        }

        [Test]
        public void no_cachecontrol_when_request_not_succes()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_request_httpResponseException_noCache").Result;

            Assert.IsNull(result.Headers.CacheControl);
        }

        [Test]
        public void no_cachecontrol_when_request_exception()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_request_exception_noCache").Result;

            Assert.IsNull(result.Headers.CacheControl);
        }
        [Test]
        public void maxage_cachecontrol_when_no_content()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_request_noContent").Result;

            Assert.IsNotNull(result.Headers.CacheControl);
            Assert.AreEqual(TimeSpan.FromSeconds(50), result.Headers.CacheControl.MaxAge);
        }


        [Test]
        public void maxage_mustrevalidate_headers_correct_with_clienttimeout_zero_with_must_revalidate()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_c0_s100_mustR").Result;

            Assert.IsTrue(result.Headers.CacheControl.MustRevalidate);
            Assert.AreEqual(TimeSpan.Zero, result.Headers.CacheControl.MaxAge);
        }


	    [Test]
	    public void nocache_headers_correct()
	    {
			var client = new HttpClient(_server);
			var result = client.GetAsync(_url + "Get_nocache").Result;

			Assert.IsTrue(result.Headers.CacheControl.NoCache,
				"NoCache in result headers was expected to be true when CacheOutput.NoCache=true.");
		    Assert.IsTrue(result.Headers.Contains("Pragma"),
				"result headers does not contain expected Pragma.");
			Assert.IsTrue(result.Headers.GetValues("Pragma").Contains("no-cache"),
				"expected no-cache Pragma was not found");
	    }

	    [Test]
        public void maxage_mustrevalidate_true_headers_correct()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_c50_mustR").Result;

            Assert.AreEqual(TimeSpan.FromSeconds(50), result.Headers.CacheControl.MaxAge);
            Assert.IsTrue(result.Headers.CacheControl.MustRevalidate);
        }

        [Test]
        public void maxage_private_true_headers_correct()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_c50_private").Result;

            Assert.AreEqual(TimeSpan.FromSeconds(50), result.Headers.CacheControl.MaxAge);
            Assert.IsTrue(result.Headers.CacheControl.Private);
        }

        [Test]
        public void maxage_mustrevalidate_headers_correct_with_cacheuntil()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_until25012015_1700").Result;
            var clientTimeSpanSeconds = new SpecificTime(2015, 01, 25, 17, 0, 0).Execute(DateTime.Now).ClientTimeSpan.TotalSeconds;
            var resultCacheControlSeconds = ((TimeSpan) result.Headers.CacheControl.MaxAge).TotalSeconds;
            Assert.IsTrue(Math.Round(clientTimeSpanSeconds - resultCacheControlSeconds) == 0);
            Assert.IsFalse(result.Headers.CacheControl.MustRevalidate);
        }

        [Test]
        public void maxage_mustrevalidate_headers_correct_with_cacheuntil_today()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_until2355_today").Result;

            Assert.IsTrue(Math.Round(new ThisDay(23,55,59).Execute(DateTime.Now).ClientTimeSpan.TotalSeconds - ((TimeSpan)result.Headers.CacheControl.MaxAge).TotalSeconds) == 0);
            Assert.IsFalse(result.Headers.CacheControl.MustRevalidate);
        }

        [Test]
        public void maxage_mustrevalidate_headers_correct_with_cacheuntil_this_month()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_until27_thismonth").Result;

            Assert.IsTrue(Math.Round(new ThisMonth(27,0,0,0).Execute(DateTime.Now).ClientTimeSpan.TotalSeconds - ((TimeSpan)result.Headers.CacheControl.MaxAge).TotalSeconds) == 0);
            Assert.IsFalse(result.Headers.CacheControl.MustRevalidate);
        }

        [Test]
        public void maxage_mustrevalidate_headers_correct_with_cacheuntil_this_year()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_until731_thisyear").Result;

            Assert.IsTrue(Math.Round(new ThisYear(7, 31, 0, 0, 0).Execute(DateTime.Now).ClientTimeSpan.TotalSeconds - ((TimeSpan)result.Headers.CacheControl.MaxAge).TotalSeconds) == 0);
            Assert.IsFalse(result.Headers.CacheControl.MustRevalidate);
        }

        [Test]
        public void maxage_mustrevalidate_headers_correct_with_cacheuntil_this_year_with_revalidate()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_until731_thisyear_mustrevalidate").Result;

            Assert.IsTrue(Math.Round(new ThisYear(7, 31, 0, 0, 0).Execute(DateTime.Now).ClientTimeSpan.TotalSeconds - ((TimeSpan)result.Headers.CacheControl.MaxAge).TotalSeconds) == 0);
            Assert.IsTrue(result.Headers.CacheControl.MustRevalidate);
        }

        [Test]
        public void private_true_headers_correct()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_private").Result;

            Assert.IsTrue(result.Headers.CacheControl.Private);
        }

        [TestFixtureTearDown]
        public void fixture_dispose()
        {
            if (_server != null) _server.Dispose();
        }
    }
}