using System.Collections.Generic;
using System.Web.Http;

namespace WebApi.OutputCache.V2.Tests.TestControllers
{
    [AutoInvalidateCacheOutput(TryMatchType = true)]
    public class AutoInvalidateWithTypeController : ApiController
    {
        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100)]
        public string Get_c100_s100()
        {
            return "test";
        }

        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100)]
        public List<string> Get_c100_s100_array()
        {
            return new List<string> {"test"};
        }

        [CacheOutput(ServerTimeSpan = 50)]
        public int Get_s50_exclude_fakecallback(int id = 0, string callback = null, string de = null)
        {
            return 7;
        }

        public void Post()
        {
            //this should not invalidate
        }

        public void PostString([FromBody]string x)
        {
            //this should invalidate string & ienumerable<string>
        }
    }
}