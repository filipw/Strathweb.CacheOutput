using System;
using System.Linq;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Threading;
using ActionFilterAttribute = System.Web.Http.Filters.ActionFilterAttribute;

namespace WebApi.OutputCache
{
    public class OutputCacheAttribute: ActionFilterAttribute
    {
        //// cache length in seconds
        protected TimeSpan Timespan;

        // client cache length in seconds
        protected TimeSpan ClientTimeSpan;

        protected DateTimeOffset AbsoluteExpiration;

        // cache for anonymous users only?
        private readonly bool anonymousOnly;

        private readonly bool mustRevalidate;

        // cache key
        private string cachekey = string.Empty;

        // cache repository
        private static readonly ObjectCache WebApiCache = MemoryCache.Default;

        readonly Func<TimeSpan, HttpActionContext, bool, bool> isCachingTimeValid = (timespan, ac, anonymous) =>
        {
            if (timespan <= TimeSpan.FromSeconds(0)) 
                return false;

            if (anonymous)
                if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
                    return false;

            return ac.Request.Method == HttpMethod.Get;
        };


        private CacheControlHeaderValue SetClientCache()
        {
            var cachecontrol = new CacheControlHeaderValue
                                {
                                    MaxAge = ClientTimeSpan, 
                                    MustRevalidate = mustRevalidate
                                };
            return cachecontrol;
        }

        public OutputCacheAttribute(int timespan = 1, int clientTimeSpan = 1, bool anonymousOnly = false, bool mustRevalidate = false)
        {
            Timespan = TimeSpan.FromSeconds(timespan);
            ClientTimeSpan = TimeSpan.FromSeconds(clientTimeSpan);
           
            this.anonymousOnly = anonymousOnly;
            this.mustRevalidate = mustRevalidate;

            AbsoluteExpiration = DateTime.Now.Add(Timespan);
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            if (!isCachingTimeValid(Timespan, actionContext, anonymousOnly)) return;

            cachekey = string.Join(":", new[]
                                            {
                                                actionContext.Request.RequestUri.PathAndQuery,
                                                Convert.ToString(actionContext.Request.Headers.Accept.FirstOrDefault())
                                            });

            if (!WebApiCache.Contains(cachekey)) return;

            var val = WebApiCache.Get(cachekey) as string;

            if (val == null) return;

            var contenttype = (MediaTypeHeaderValue) WebApiCache.Get(cachekey + ":response-ct") ??
                              new MediaTypeHeaderValue(cachekey.Split(':')[1]);

            actionContext.Response = actionContext.Request.CreateResponse();
            actionContext.Response.Content = new StringContent(val);

            actionContext.Response.Content.Headers.ContentType = contenttype;
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (!(WebApiCache.Contains(cachekey)) && !string.IsNullOrWhiteSpace(cachekey))
            {
                SetEtag(actionExecutedContext);

                var body = actionExecutedContext.Response.Content.ReadAsStringAsync().Result;
                
                WebApiCache.Add(cachekey, body, AbsoluteExpiration);
                WebApiCache.Add(cachekey + ":response-ct", actionExecutedContext.Response.Content.Headers.ContentType, AbsoluteExpiration);
            }

            if (isCachingTimeValid(ClientTimeSpan, actionExecutedContext.ActionContext, anonymousOnly))
                actionExecutedContext.ActionContext.Response.Headers.CacheControl = SetClientCache();
        }

        private static void SetEtag(HttpActionExecutedContext actionExecutedContext)
        {
            var eTag = new EntityTagHeaderValue(@"""" + Guid.NewGuid() + @"""");
            actionExecutedContext.Response.Headers.ETag = eTag;
        }
    }
}