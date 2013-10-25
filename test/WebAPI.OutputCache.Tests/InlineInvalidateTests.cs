using System.Net.Http;
using System.Threading;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Moq;
using NUnit.Framework;
using WebApi.OutputCache.Core.Cache;

namespace WebAPI.OutputCache.Tests
{
    [TestFixture]
    public class InlineInvalidateTests
    {
        private HttpServer _server;
        private string _url = "http://www.strathweb.com/api/inlineinvalidate/";
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
        public void inline_call_to_invalidate_is_correct()
        {
            var client = new HttpClient(_server);

            var result = client.PostAsync(_url + "Post", new StringContent(string.Empty)).Result;

            _cache.Verify(s => s.RemoveStartsWith(It.Is<string>(x => x == "inlineinvalidate-get_c100_s100")), Times.Exactly(1));
        }

        [Test]
        public void inline_call_to_invalidate_using_expression_tree_is_correct()
        {
            var client = new HttpClient(_server);
            var result = client.PutAsync(_url + "Put", new StringContent(string.Empty)).Result;

            _cache.Verify(s => s.RemoveStartsWith(It.Is<string>(x => x == "inlineinvalidate-get_c100_s100")), Times.Exactly(1));
        }

        [Test]
        public void inline_call_to_invalidate_using_expression_tree_with_param_is_correct()
        {
            var client = new HttpClient(_server);
            var result = client.DeleteAsync(_url + "Delete_parameterized").Result;

            _cache.Verify(s => s.RemoveStartsWith(It.Is<string>(x => x == "inlineinvalidate-get_c100_s100_with_param")), Times.Exactly(1));
        }

        [Test]
        public void inline_call_to_invalidate_using_expression_tree_with_custom_action_name_is_correct()
        {
            var client = new HttpClient(_server);
            var result = client.DeleteAsync(_url + "Delete_non_standard_name").Result;

            _cache.Verify(s => s.RemoveStartsWith(It.Is<string>(x => x == "inlineinvalidate-getbyid")), Times.Exactly(1));
        }

        [TearDown]
        public void fixture_dispose()
        {
            if (_server != null) _server.Dispose();
        }
    }
}