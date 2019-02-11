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
using System.Collections.Generic;
using System.Linq;

namespace WebApi.OutputCache.V2.Tests
{
    [TestFixture]
    public class CustomHeadersTests
    {
        private HttpServer _server;
        private string _url = "http://www.strathweb.com/api/customheaders/";
        private IApiOutputCache _cache;

        [SetUp]
        public void init()
        {
            Thread.CurrentPrincipal = null;

            _cache = new SimpleCacheForTests();

            var conf = new HttpConfiguration();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(_cache);

            conf.DependencyResolver = new AutofacWebApiDependencyResolver(builder.Build());
            conf.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            _server = new HttpServer(conf);
        }

        [Test]
        public void cache_custom_content_header() {
            var client = new HttpClient(_server);
            var req = new HttpRequestMessage(HttpMethod.Get, _url + "Cache_Custom_Content_Header");
            var result = client.SendAsync(req).Result;

            var req2 = new HttpRequestMessage(HttpMethod.Get, _url + "Cache_Custom_Content_Header");
            var result2 = client.SendAsync(req2).Result;

            Assert.That(result.Content.Headers.ContentDisposition.DispositionType, Is.EqualTo("attachment"));
            Assert.That(result2.Content.Headers.ContentDisposition.DispositionType, Is.EqualTo("attachment"));
        }

        [Test]
        public void cache_custom_content_header_with_multiply_values()
        {
            var client = new HttpClient(_server);
            var req = new HttpRequestMessage(HttpMethod.Get, _url + "Cache_Custom_Content_Header_Multiply_Values");
            var result = client.SendAsync(req).Result;

            var req2 = new HttpRequestMessage(HttpMethod.Get, _url + "Cache_Custom_Content_Header_Multiply_Values");
            var result2 = client.SendAsync(req2).Result;

            Assert.That(result.Content.Headers.ContentEncoding.Count, Is.EqualTo(2));
            Assert.That(result.Content.Headers.ContentEncoding.First(), Is.EqualTo("deflate"));
            Assert.That(result.Content.Headers.ContentEncoding.Last(), Is.EqualTo("gzip"));

            Assert.That(result2.Content.Headers.ContentEncoding.Count, Is.EqualTo(2));
            Assert.That(result2.Content.Headers.ContentEncoding.First(), Is.EqualTo("deflate"));
            Assert.That(result2.Content.Headers.ContentEncoding.Last(), Is.EqualTo("gzip"));
        }

        [Test]
        public void cache_custom_response_header()
        {
            var client = new HttpClient(_server);
            var req = new HttpRequestMessage(HttpMethod.Get, _url + "Cache_Custom_Response_Header");
            var result = client.SendAsync(req).Result;

            var req2 = new HttpRequestMessage(HttpMethod.Get, _url + "Cache_Custom_Response_Header");
            var result2 = client.SendAsync(req2).Result;

            Assert.That(result.Headers.GetValues("RequestHeader1").First(), Is.EqualTo("value1"));
            Assert.That(result2.Headers.GetValues("RequestHeader1").First(), Is.EqualTo("value1"));
        }

        [Test]
        public void cache_custom_response_header_with_multiply_values()
        {
            var client = new HttpClient(_server);
            var req = new HttpRequestMessage(HttpMethod.Get, _url + "Cache_Custom_Response_Header_Multiply_Values");
            var result = client.SendAsync(req).Result;

            var req2 = new HttpRequestMessage(HttpMethod.Get, _url + "Cache_Custom_Response_Header_Multiply_Values");
            var result2 = client.SendAsync(req2).Result;

            Assert.That(result.Headers.GetValues("RequestHeader2").Count(), Is.EqualTo(2));
            Assert.That(result.Headers.GetValues("RequestHeader2").First(), Is.EqualTo("value2"));
            Assert.That(result.Headers.GetValues("RequestHeader2").Last(), Is.EqualTo("value3"));

            Assert.That(result2.Headers.GetValues("RequestHeader2").Count(), Is.EqualTo(2));
            Assert.That(result2.Headers.GetValues("RequestHeader2").First(), Is.EqualTo("value2"));
            Assert.That(result2.Headers.GetValues("RequestHeader2").Last(), Is.EqualTo("value3"));
        }

        [Test]
        public void cache_multiply_custom_headers()
        {
            var client = new HttpClient(_server);
            var req = new HttpRequestMessage(HttpMethod.Get, _url + "Cache_Multiply_Custom_Headers");
            var result = client.SendAsync(req).Result;

            var req2 = new HttpRequestMessage(HttpMethod.Get, _url + "Cache_Multiply_Custom_Headers");
            var result2 = client.SendAsync(req2).Result;

            Assert.That(result.Content.Headers.ContentDisposition.DispositionType, Is.EqualTo("attachment"));
            Assert.That(result.Content.Headers.ContentEncoding.Count, Is.EqualTo(2));
            Assert.That(result.Content.Headers.ContentEncoding.First(), Is.EqualTo("deflate"));
            Assert.That(result.Content.Headers.ContentEncoding.Last(), Is.EqualTo("gzip"));
            Assert.That(result.Headers.GetValues("RequestHeader1").First(), Is.EqualTo("value1"));
            Assert.That(result.Headers.GetValues("RequestHeader2").Count(), Is.EqualTo(2));
            Assert.That(result.Headers.GetValues("RequestHeader2").First(), Is.EqualTo("value2"));
            Assert.That(result.Headers.GetValues("RequestHeader2").Last(), Is.EqualTo("value3"));

            Assert.That(result2.Content.Headers.ContentDisposition.DispositionType, Is.EqualTo("attachment"));
            Assert.That(result2.Content.Headers.ContentEncoding.Count, Is.EqualTo(2));
            Assert.That(result2.Content.Headers.ContentEncoding.First(), Is.EqualTo("deflate"));
            Assert.That(result2.Content.Headers.ContentEncoding.Last(), Is.EqualTo("gzip"));
            Assert.That(result2.Headers.GetValues("RequestHeader1").First(), Is.EqualTo("value1"));
            Assert.That(result2.Headers.GetValues("RequestHeader2").Count(), Is.EqualTo(2));
            Assert.That(result2.Headers.GetValues("RequestHeader2").First(), Is.EqualTo("value2"));
            Assert.That(result2.Headers.GetValues("RequestHeader2").Last(), Is.EqualTo("value3"));
        }

        [Test]
        public void cache_part_of_custom_headers()
        {
            var client = new HttpClient(_server);
            var req = new HttpRequestMessage(HttpMethod.Get, _url + "Cache_Part_Of_Custom_Headers");
            var result = client.SendAsync(req).Result;

            var req2 = new HttpRequestMessage(HttpMethod.Get, _url + "Cache_Part_Of_Custom_Headers");
            var result2 = client.SendAsync(req2).Result;

            Assert.That(result.Content.Headers.ContentDisposition.DispositionType, Is.EqualTo("attachment"));
            Assert.That(result.Content.Headers.ContentEncoding.Count, Is.EqualTo(2));
            Assert.That(result.Content.Headers.ContentEncoding.First(), Is.EqualTo("deflate"));
            Assert.That(result.Content.Headers.ContentEncoding.Last(), Is.EqualTo("gzip"));
            Assert.That(result.Headers.GetValues("RequestHeader1").First(), Is.EqualTo("value1"));
            Assert.That(result.Headers.GetValues("RequestHeader2").Count(), Is.EqualTo(2));
            Assert.That(result.Headers.GetValues("RequestHeader2").First(), Is.EqualTo("value2"));
            Assert.That(result.Headers.GetValues("RequestHeader2").Last(), Is.EqualTo("value3"));

            Assert.That(result2.Content.Headers.ContentDisposition, Is.Null);
            Assert.That(result2.Content.Headers.ContentEncoding.Count, Is.EqualTo(2));
            Assert.That(result2.Content.Headers.ContentEncoding.First(), Is.EqualTo("deflate"));
            Assert.That(result2.Content.Headers.ContentEncoding.Last(), Is.EqualTo("gzip"));

            IEnumerable<string> headerValue = null;
            Assert.That(result2.Headers.TryGetValues("RequestHeader1", out headerValue), Is.False);
            Assert.That(result2.Headers.TryGetValues("RequestHeader2", out headerValue), Is.False);
        }

        [TearDown]
        public void fixture_dispose()
        {
            if (_server != null) _server.Dispose();
        }
    }
}