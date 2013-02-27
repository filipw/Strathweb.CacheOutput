using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Routing;
using WebAPI.OutputCache.Time;

namespace WebAPI.OutputCache
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CacheOutputAttribute : BaseCacheAttribute
    {
        public bool AnonymousOnly { get; set; }
        public bool MustRevalidate { get; set; }
        public bool ExcludeQueryStringFromCacheKey { get; set; }
        public int ServerTimeSpan { get; set; }
        public int ClientTimeSpan { get; set; }
        private MediaTypeHeaderValue _responseMediaType;

        internal IModelQuery<DateTime, CacheTime> CacheTimeQuery;

        readonly Func<HttpActionContext, bool, bool> _isCachingAllowed = (ac, anonymous) =>
        {
            if (anonymous)
                if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
                    return false;

            return ac.Request.Method == HttpMethod.Get;
        };

        protected virtual void EnsureCacheTimeQuery()
        {
            if (CacheTimeQuery == null) CacheTimeQuery = new ShortTime(ServerTimeSpan, ClientTimeSpan);
        }

        protected virtual MediaTypeHeaderValue GetExpectedMediaType(HttpConfiguration config, HttpActionContext actionContext)
        {
            var responseMediaType = actionContext.Request.Headers.Accept != null
                                        ? actionContext.Request.Headers.Accept.FirstOrDefault()
                                        : new MediaTypeHeaderValue("application/json");
            
            var negotiator = config.Services.GetService(typeof (IContentNegotiator)) as IContentNegotiator;

            if (negotiator != null)
            {
                var negotiatedResult = negotiator.Negotiate(actionContext.ActionDescriptor.ReturnType, actionContext.Request, config.Formatters);
                responseMediaType = negotiatedResult.MediaType;
            }

            return responseMediaType;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext == null) throw new ArgumentNullException("actionContext");

            if (!_isCachingAllowed(actionContext, AnonymousOnly)) return;

            var config = actionContext.Request.GetConfiguration();

            EnsureCacheTimeQuery();
            EnsureCache(config, actionContext.Request);

            _responseMediaType = GetExpectedMediaType(config, actionContext);
            var cachekey = MakeCachekey(actionContext, _responseMediaType, ExcludeQueryStringFromCacheKey);

            if (!WebApiCache.Contains(cachekey)) return;

            if (actionContext.Request.Headers.IfNoneMatch != null)
            {
                var etag = WebApiCache.Get(cachekey + Constants.EtagKey) as EntityTagHeaderValue;
                if (etag != null)
                {
                    if (actionContext.Request.Headers.IfNoneMatch.Any(x => x.Tag ==  etag.Tag))
                    {
                        var time = CacheTimeQuery.Execute(DateTime.Now);
                        var quickResponse = actionContext.Request.CreateResponse(HttpStatusCode.NotModified);
                        ApplyCacheHeaders(quickResponse, time);
                        actionContext.Response = quickResponse;
                        return;
                    }
                }
            }

            var val = WebApiCache.Get(cachekey) as string;
            if (val == null) return;

            var contenttype = (MediaTypeHeaderValue)WebApiCache.Get(cachekey + Constants.ContentTypeKey) ??
                              new MediaTypeHeaderValue(cachekey.Split(':')[1]);

            actionContext.Response = actionContext.Request.CreateResponse();
            actionContext.Response.Content = new StringContent(val);

            actionContext.Response.Content.Headers.ContentType = contenttype;
            var responseEtag = WebApiCache.Get(cachekey + Constants.EtagKey) as EntityTagHeaderValue;
            if (responseEtag != null) SetEtag(actionContext.Response, responseEtag.Tag);

            var cacheTime = CacheTimeQuery.Execute(DateTime.Now);
            ApplyCacheHeaders(actionContext.Response, cacheTime);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.ActionContext.Response == null || !actionExecutedContext.ActionContext.Response.IsSuccessStatusCode) return;

            if (!_isCachingAllowed(actionExecutedContext.ActionContext, AnonymousOnly)) return;

            var cacheTime = CacheTimeQuery.Execute(DateTime.Now);
            if (cacheTime.AbsoluteExpiration > DateTime.Now)
            {
                var cachekey = MakeCachekey(actionExecutedContext.ActionContext, _responseMediaType, ExcludeQueryStringFromCacheKey);

                if (!string.IsNullOrWhiteSpace(cachekey) && !(WebApiCache.Contains(cachekey)))
                {
                    SetEtag(actionExecutedContext.Response, Guid.NewGuid().ToString());

                    actionExecutedContext.Response.Content.ReadAsStringAsync().ContinueWith(t =>
                        {
                            var baseKey = actionExecutedContext.Request.GetConfiguration().CacheOutputConfiguration().MakeBaseCachekey(actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerName, actionExecutedContext.ActionContext.ActionDescriptor.ActionName);
                            WebApiCache.Add(baseKey, string.Empty, cacheTime.AbsoluteExpiration);
                            WebApiCache.Add(cachekey, t.Result, cacheTime.AbsoluteExpiration, baseKey);

                            WebApiCache.Add(cachekey + Constants.ContentTypeKey,
                                            actionExecutedContext.Response.Content.Headers.ContentType,
                                            cacheTime.AbsoluteExpiration, baseKey);

                            WebApiCache.Add(cachekey + Constants.EtagKey,
                                            actionExecutedContext.Response.Headers.ETag,
                                            cacheTime.AbsoluteExpiration, baseKey);
                        });
                }
            }

            ApplyCacheHeaders(actionExecutedContext.ActionContext.Response, cacheTime);
        }

        private void ApplyCacheHeaders(HttpResponseMessage response, CacheTime cacheTime)
        {
            if (cacheTime.ClientTimeSpan > TimeSpan.Zero || MustRevalidate)
            {
                var cachecontrol = new CacheControlHeaderValue
                                       {
                                           MaxAge = cacheTime.ClientTimeSpan,
                                           MustRevalidate = MustRevalidate
                                       };

                response.Headers.CacheControl = cachecontrol;
            }
        }

        private static void SetEtag(HttpResponseMessage message, string etag)
        {
            var eTag = new EntityTagHeaderValue(@"""" + etag.Replace("\"", string.Empty) + @"""");
            message.Headers.ETag = eTag;
        }
    }
} 