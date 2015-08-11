using System.Net.Http;
using System.Threading;
using System.Web.Http;

using Autofac;
using Autofac.Integration.WebApi;

using Moq;

using NUnit.Framework;

using WebApi.OutputCache.Core.Cache;
using WebApi.OutputCache.V2.Tests.TestControllers;

namespace WebApi.OutputCache.V2.Tests
{
    [TestFixture]
    public class InvalidateCacheOutputByPrefixTests
    {        
        public class OnActionExecuting : InvalidateCacheOutputByPrefixTests
        {
            private HttpServer _server;

            private string _url = "http://www.strathweb.com/api/invalidatecacheoutputbyprefix/";

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
                    defaults: new { id = RouteParameter.Optional });

                _server = new HttpServer(conf);
            }

            [Test]
            public void ShouldRemoveKeysStartingWithPrefixForCurrentController()
            {
                // Arrange
                const string Resourcecode = "resourceCode";
                var expectedKeys = new[] { "keyPrefix-1", "keyPrefix-2" };

                var generatedKeyPrefix = new StubPrefixGenerator().Generate(null, "baseCacheKey");
                _cache.Setup(x => x.FindKeysStartingWith(It.Is<string>(s => s == generatedKeyPrefix)))
                    .Returns(expectedKeys);

                // Act
                var client = new HttpClient(_server);
                client.PutAsync(string.Format("{0}{1}/{2}", _url, "Put", Resourcecode), new StringContent(Resourcecode));

                // Assert
                _cache.Verify(cache => cache.Remove(It.Is<string>(s => s == expectedKeys[0])));
                _cache.Verify(cache => cache.Remove(It.Is<string>(s => s == expectedKeys[1])));
            }
        }

    }
}