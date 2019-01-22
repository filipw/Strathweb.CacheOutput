﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Moq;
using NUnit.Framework;
using WebApi.OutputCache.Core.Cache;

namespace WebApi.OutputCache.V2.Tests
{
    [TestFixture]
    public class ConnegTests
    {
        private HttpServer _server;
        private string _url = "http://www.strathweb.com/api/sample/";
        private Mock<IApiOutputCache> _cache;

        [SetUp]
        public void init()
        {
            _cache = new Mock<IApiOutputCache>();
            _cache.Setup(cache => cache.ContainsAsync(It.IsAny<string>())).Returns(Task.FromResult(It.IsAny<bool>()));
            _cache.Setup(cache => cache.AddAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>())).Returns(Task.FromResult(default(object)));

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
        public void subsequent_xml_request_is_not_cached()
        {
            var client = new HttpClient(_server);
            var result = client.GetAsync(_url + "Get_c100_s100").Result;

            _cache.Verify(s => s.ContainsAsync(It.Is<string>(x => x == "webapi.outputcache.v2.tests.testcontrollers.samplecontroller-get_c100_s100:application/json; charset=utf-8")), Times.Exactly(2));
            _cache.Verify(s => s.AddAsync(It.Is<string>(x => x == "webapi.outputcache.v2.tests.testcontrollers.samplecontroller-get_c100_s100:application/json; charset=utf-8"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(100)), It.Is<string>(x => x == "webapi.outputcache.v2.tests.testcontrollers.samplecontroller-get_c100_s100")), Times.Once());

            var req = new HttpRequestMessage(HttpMethod.Get, _url + "Get_c100_s100");
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

            var result2 = client.SendAsync(req).Result;
            _cache.Verify(s => s.ContainsAsync(It.Is<string>(x => x == "webapi.outputcache.v2.tests.testcontrollers.samplecontroller-get_c100_s100:text/xml; charset=utf-8")), Times.Exactly(2));
            _cache.Verify(s => s.AddAsync(It.Is<string>(x => x == "webapi.outputcache.v2.tests.testcontrollers.samplecontroller-get_c100_s100:text/xml; charset=utf-8"), It.IsAny<object>(), It.Is<DateTimeOffset>(x => x < DateTime.Now.AddSeconds(100)), It.Is<string>(x => x == "webapi.outputcache.v2.tests.testcontrollers.samplecontroller-get_c100_s100")), Times.Once());

        }
            
        [TearDown]
        public void fixture_dispose()
        {
            if (_server != null) _server.Dispose();
        }
    }
}
