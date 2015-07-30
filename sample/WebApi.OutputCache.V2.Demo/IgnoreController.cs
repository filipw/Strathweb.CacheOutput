using System;
using System.Web.Http;

namespace WebApi.OutputCache.V2.Demo
{
    [CacheOutput(ClientTimeSpan = 50, ServerTimeSpan = 50)]
    [RoutePrefix("ignore")]
    public class IgnoreController : ApiController
    {
        [Route("cached")]
        public string GetCached()
        {
            return DateTime.Now.ToString();
        }

        [IgnoreCacheOutput]
        [Route("uncached")]
        public string GetUnCached()
        {
            return DateTime.Now.ToString();
        }
    }
}