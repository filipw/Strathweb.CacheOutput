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

        //Send the X-Test-Key header with your request and it will cache it based on the value of the X-Test-Key
        [CacheOutput(ClientTimeSpan = 50, ServerTimeSpan = 50, IncludeCustomHeaders = "X-Test-Key")]
        [Route("cachedwithheaders")]
        public string GetCachedWithHeaders()
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