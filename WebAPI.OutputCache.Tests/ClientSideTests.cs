using System;
using System.Diagnostics;
using System.Net.Http;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Moq;
using NUnit.Framework;
using WebAPI.OutputCache.Cache;
using WebAPI.OutputCache.Time;

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
        public void maxage_mustrevalidate_true_headers_correct()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_c50_mustR").Result;

            Assert.AreEqual(TimeSpan.FromSeconds(50), result.Headers.CacheControl.MaxAge);
            Assert.IsTrue(result.Headers.CacheControl.MustRevalidate);
        }

        [Test]
        public void maxage_mustrevalidate_headers_correct_with_cacheuntil()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_until25012013_1700").Result;

            Assert.IsTrue(Math.Round(new SpecificTime(2013, 01, 25, 17, 0, 0).Execute(DateTime.Now).ClientTimeSpan.TotalSeconds - ((TimeSpan)result.Headers.CacheControl.MaxAge).TotalSeconds) == 0);
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

        [TestFixtureTearDown]
        public void fixture_dispose()
        {
            if (_server != null) _server.Dispose();
        }
    }
}