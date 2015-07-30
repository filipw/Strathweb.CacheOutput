using System.Web.Http;

namespace WebApi.OutputCache.V2.Tests.TestControllers
{
    [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100)]
    public class IgnoreController : ApiController
    {
        [HttpGet]
        public string Cached()
        {
            return "test";
        }

        [HttpGet]
        [IgnoreCacheOutput]
        public string NotCached()
        {
            return "test";
        }
    }
}