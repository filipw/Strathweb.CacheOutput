using System.Web.Http;

namespace WebAPI.OutputCache.Tests.TestControllers
{
    public class CacheKeyController : ApiController
    {
        [CacheOutput(CacheKeyGenerator = typeof(CacheKeyGeneratorTests.CustomCacheKeyGenerator), ClientTimeSpan = 100, ServerTimeSpan = 100)]
        public string Get_custom_key()
        {
            return "test";
        }
    }
}
