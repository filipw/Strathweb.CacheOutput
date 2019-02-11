using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace WebApi.OutputCache.V2.Tests
{
    public class CustomHeadersContent<T> : OkNegotiatedContentResult<T>
    {
        public string ContentDisposition { get; set; }

        public List<string> ContentEncoding { get; set; }

        public string RequestHeader1 { get; set; }

        public List<string> RequestHeader2 { get; set; }

        public CustomHeadersContent(T content, ApiController controller)
            : base(content, controller) { }

        public CustomHeadersContent(T content, IContentNegotiator contentNegotiator, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
            : base(content, contentNegotiator, request, formatters) { }

        public override async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.ExecuteAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(ContentDisposition))
            {
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(ContentDisposition);
            }
            if (ContentEncoding != null)
            {
                foreach (var contentEncoding in ContentEncoding)
                {
                    response.Content.Headers.ContentEncoding.Add(contentEncoding);
                }
            }

            if (!string.IsNullOrWhiteSpace(RequestHeader1))
            {
                response.Headers.Add("RequestHeader1", RequestHeader1);
            }
            if (RequestHeader2 != null)
            {
                foreach (var requestHeader2Value in RequestHeader2)
                {
                    response.Headers.Add("RequestHeader2", requestHeader2Value);
                }
            }

            return response;
        }
    }
}
