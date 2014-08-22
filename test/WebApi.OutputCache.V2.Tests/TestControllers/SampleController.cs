using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.OutputCache.V2.TimeAttributes;

namespace WebApi.OutputCache.V2.Tests.TestControllers
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

        [CacheOutput(NoCache=true)]
        public string Get_nocache()
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

        [CacheOutput(ClientTimeSpan = 50, Private = true)]
        public string Get_c50_private()
        {
            return "test";
        }

        [CacheOutput(Private = true)]
        public string Get_private()
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
        public string Get_request_exception_noCache()
        {
            throw new System.Exception("Fault shouldn't cache");
        }

        [CacheOutput(ClientTimeSpan = 50, ServerTimeSpan = 50)]
        public string Get_request_httpResponseException_noCache()
        {
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Conflict){ReasonPhrase = "Fault shouldn't cache"});
        }

        [CacheOutput(ClientTimeSpan = 50, ServerTimeSpan = 50)]
        public HttpResponseMessage Get_request_noContent()
        {
            return Request.CreateResponse(HttpStatusCode.Accepted);
        }

        [InvalidateCacheOutput("Get_c100_s100")]
        public void Post()
        {
            //do nothing
        }

        [InvalidateCacheOutput("Get_c100_s100")]
        [InvalidateCacheOutput("Get_s50_exclude_fakecallback")]
        public void Post_2_invalidates()
        {
            //do nothing
        }
    }
}
