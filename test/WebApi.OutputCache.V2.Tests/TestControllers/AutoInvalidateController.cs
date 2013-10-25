using System.Web.Http;

namespace WebApi.OutputCache.V2.Tests.TestControllers
{
    [AutoInvalidateCacheOutput]
    public class AutoInvalidateController : ApiController
    {
        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100)]
        public string Get_c100_s100()
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
            //do nothing
        }

        public void Put()
        {
            //do nothing
        }

        public void Delete()
        {
            //do nothing
        }
    }
}