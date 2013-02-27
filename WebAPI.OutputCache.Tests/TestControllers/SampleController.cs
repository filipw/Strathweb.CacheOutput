using System.Web.Http;
using WebAPI.OutputCache.TimeAttributes;

namespace WebAPI.OutputCache.Tests.TestControllers
{
    public class SampleController : ApiController
    {
        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100)]
        public string Get_c100_s100()
        {
            return "test";
        }

        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 0)]
        public string Get_c100_s0()
        {
            return "test";
        }

        [CacheOutput(ClientTimeSpan = 0, ServerTimeSpan = 100)]
        public string Get_c0_s100()
        {
            return "test";
        }

        [CacheOutput(ClientTimeSpan = 0, ServerTimeSpan = 100, MustRevalidate = true)]
        public string Get_c0_s100_mustR()
        {
            return "test";
        }

        [CacheOutput(ClientTimeSpan = 50, MustRevalidate = true)]
        public string Get_c50_mustR()
        {
            return "test";
        }

        [CacheOutput(ServerTimeSpan = 50)]
        public string Get_s50_exclude_fakecallback(int? id = null, string callback = null, string de = null)
        {
            return "test";
        }

        [CacheOutput(ServerTimeSpan = 50, ExcludeQueryStringFromCacheKey = false)]
        public string Get_s50_exclude_false(int id)
        {
            return "test"+id;
        }

        [CacheOutput(ServerTimeSpan = 50, ExcludeQueryStringFromCacheKey = true)]
        public string Get_s50_exclude_true(int id)
        {
            return "test" + id;
        }

        [CacheOutputUntil(2015,01,25,17,00)]
        public string Get_until25012015_1700()
        {
            return "test";
        }

        [CacheOutputUntilToday(23,55)]
        public string Get_until2355_today()
        {
            return "value";
        }

        [CacheOutputUntilThisMonth(27)]
        public string Get_until27_thismonth()
        {
            return "value";
        }

        [CacheOutputUntilThisYear(7,31)]
        public string Get_until731_thisyear()
        {
            return "value";
        }

        [CacheOutputUntilThisYear(7, 31, MustRevalidate = true)]
        public string Get_until731_thisyear_mustrevalidate()
        {
            return "value";
        }

        [CacheOutput(AnonymousOnly = true, ClientTimeSpan = 50, ServerTimeSpan = 50)]
        public string Get_s50_c50_anonymousonly()
        {
            return "value";
        }

        [HttpGet]
        [CacheOutput(AnonymousOnly = true, ClientTimeSpan = 50, ServerTimeSpan = 50)]
        public string etag_match_304()
        {
            return "value";
        }

        [CacheOutput(ClientTimeSpan = 50, ServerTimeSpan = 50)]
        public string Get_request_not_succesful()
        {
            throw new System.Exception("Fault");
        }

        [InvalidateCacheOutput("Get_c100_s100", null)]
        public void Post()
        {
            //do nothing
        }

        [InvalidateCacheOutput("Get_c100_s100", null)]
        [InvalidateCacheOutput("Get_s50_exclude_fakecallback", null)]
        public void Post_2_invalidates()
        {
            //do nothing
        }
    }
}
