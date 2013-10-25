using System.Web.Http;

namespace WebApi.OutputCache.V2.Tests.TestControllers
{
    public class InlineInvalidateController : ApiController
    {
        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100)]
        public string Get_c100_s100()
        {
            return "test";
        }

        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100)]
        public string Get_c100_s100_with_param(int id)
        {
            return "test";
        }

        [ActionName("getById")]
        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100)]
        public string Get_c100_s100(int id)
        {
            return "test";
        }

        [CacheOutput(ServerTimeSpan = 50)]
        public string Get_s50_exclude_fakecallback(int id = 0, string callback = null, string de = null)
        {
            return "test";
        }

        [HttpGet]
        [CacheOutput(AnonymousOnly = true, ClientTimeSpan = 50, ServerTimeSpan = 50)]
        public string etag_match_304()
        {
            return "value";
        }

        public void Post()
        {
            var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);
            cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey("InlineInvalidate", "Get_c100_s100"));

            //do nothing
        }

        public void Put()
        {
            var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);
            cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((InlineInvalidateController x) => x.Get_c100_s100()));

            //do nothing
        }

        public void Delete_non_standard_name()
        {
            var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);
            cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((InlineInvalidateController x) => x.Get_c100_s100(7)));            
        }

        public void Delete_parameterized()
        {
            var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);
            cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((InlineInvalidateController x) => x.Get_c100_s100_with_param(7)));

            //do nothing
        }
    }
}