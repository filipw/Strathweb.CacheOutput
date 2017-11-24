using System.Web.Http;
using System.Web.Http.Controllers;

namespace WebApi.OutputCache.V2.Tests.TestControllers
{
    public class InvalidateCacheOutputByPrefixController : ApiController
    {

        public void Get(string id)
        {
            // do nothing
        }

        [InvalidateCacheOutputByPrefixKey(typeof(StubPrefixGenerator), "Get")]
        public void Put(string id)
        {
            // do nothing
        }

        [InvalidateCacheOutputByPrefixKey(typeof(StubPrefixGenerator), "Get", typeof(SampleController))]
        public void Delete(string id)
        {
            // do nothing
        }
    }

    public class StubPrefixGenerator : IKeyPrefixGenerator
    {
        public string Generate(HttpActionContext actionContext, string baseCacheKey)
        {
            return "keyPrefix";
        }
    }
}
