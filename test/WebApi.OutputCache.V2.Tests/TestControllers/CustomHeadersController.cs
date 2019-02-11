using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApi.OutputCache.V2.Tests.TestControllers
{
    public class CustomHeadersController : ApiController
    {
        [HttpGet]
        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100, IncludeCustomHeaders = "Content-Disposition")]
        public IHttpActionResult Cache_Custom_Content_Header()
        {
            var result = new CustomHeadersContent<string>("test", this)
            {
                ContentDisposition = "attachment"
            };

            return result;
        }

        [HttpGet]
        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100, IncludeCustomHeaders = "Content-Encoding")]
        public IHttpActionResult Cache_Custom_Content_Header_Multiply_Values()
        {
            var result = new CustomHeadersContent<string>("test", this)
            {
                ContentEncoding = new List<string> { "deflate", "gzip" }
            };

            return result;
        }

        [HttpGet]
        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100, IncludeCustomHeaders = "RequestHeader1")]
        public IHttpActionResult Cache_Custom_Response_Header()
        {
            var result = new CustomHeadersContent<string>("test", this)
            {
                RequestHeader1 = "value1"
            };

            return result;
        }

        [HttpGet]
        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100, IncludeCustomHeaders = "RequestHeader2")]
        public IHttpActionResult Cache_Custom_Response_Header_Multiply_Values()
        {
            var result = new CustomHeadersContent<string>("test", this)
            {
                RequestHeader2 = new List<string> { "value2", "value3" }
            };

            return result;
        }

        [HttpGet]
        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100, IncludeCustomHeaders = "Content-Disposition,Content-Encoding,RequestHeader2,RequestHeader1")]
        public IHttpActionResult Cache_Multiply_Custom_Headers()
        {
            var result = new CustomHeadersContent<string>("test", this)
            {
                ContentDisposition = "attachment",
                ContentEncoding = new List<string> { "deflate", "gzip" },
                RequestHeader1 = "value1",
                RequestHeader2 = new List<string> { "value2", "value3" }
            };

            return result;
        }

        [HttpGet]
        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100, IncludeCustomHeaders = "Content-Encoding,NotExistingHeader")]
        public IHttpActionResult Cache_Part_Of_Custom_Headers()
        {
            var result = new CustomHeadersContent<string>("test", this)
            {
                ContentDisposition = "attachment",
                ContentEncoding = new List<string> { "deflate", "gzip" },
                RequestHeader1 = "value1",
                RequestHeader2 = new List<string> { "value2", "value3" }
            };

            return result;
        }
    }
}