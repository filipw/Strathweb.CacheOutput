using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebAPI.OutputCache.Cache;
using WebAPI.OutputCache.Time;

namespace WebAPI.OutputCache
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CacheOutputAttribute : ActionFilterAttribute
    {
        public bool AnonymousOnly { get; set; }
        public bool MustRevalidate { get; set; }
        public bool ExcludeQueryStringFromCacheKey { get; set; }
        public int ServerTimeSpan { get; set; }
        public int ClientTimeSpan { get; set; }

        // cache repository
        private IApiOutputCache _webApiCache;
        private MediaTypeHeaderValue _responseMediaType;

        internal IModelQuery<DateTime, CacheTime> CacheTimeQuery;

        readonly Func<HttpActionContext, bool, bool> _isCachingAllowed = (ac, anonymous) =>
        {
            if (anonymous)
                if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
                    return false;

            return ac.Request.Method == HttpMethod.Get;
        };

        protected virtual void EnsureCache(HttpRequestMessage req)
        {
            _webApiCache = req != null ? req.GetDependencyScope().GetService(typeof(IApiOutputCache)) as IApiOutputCache ?? new MemoryCacheDefault() : new MemoryCacheDefault();
        }

        protected virtual void EnsureCacheTimeQuery()
        {
            if (CacheTimeQuery == null) CacheTimeQuery = new ShortTime(ServerTimeSpan, ClientTimeSpan);
        }

        protected virtual MediaTypeHeaderValue GetExpectedMediaType(HttpActionContext actionContext)
        {
            var responseMediaType = actionContext.Request.Headers.Accept != null
                                        ? actionContext.Request.Headers.Accept.FirstOrDefault()
                                        : new MediaTypeHeaderValue("application/json");
            
            var config = actionContext.Request.GetConfiguration();
            var negotiator = config.Services.GetService(typeof (IContentNegotiator)) as IContentNegotiator;

            if (negotiator != null)
            {
                var negotiatedResult = negotiator.Negotiate(actionContext.ActionDescriptor.ReturnType, actionContext.Request, config.Formatters);
                responseMediaType = negotiatedResult.MediaType;
            }
            return responseMediaType;
        }

        protected virtual string MakeCachekey(HttpRequestMessage request, MediaTypeHeaderValue mediaType)
        {
            var uri = request.RequestUri.PathAndQuery;
            if (ExcludeQueryStringFromCacheKey) uri = request.RequestUri.AbsolutePath;

            var cachekey = string.Join(":", new[]
            {
                uri,
                mediaType.MediaType
            });
            return cachekey;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }
            if (!_isCachingAllowed(actionContext, AnonymousOnly)) return;

            EnsureCacheTimeQuery();
            EnsureCache(actionContext.Request);

            _responseMediaType = GetExpectedMediaType(actionContext);
            var cachekey = MakeCachekey(actionContext.Request, _responseMediaType);

            if (!_webApiCache.Contains(cachekey)) return;

            var val = _webApiCache.Get(cachekey) as string;
            if (val == null) return;

            var contenttype = (MediaTypeHeaderValue)_webApiCache.Get(cachekey + Constants.ContentTypeKey) ??
                              new MediaTypeHeaderValue(cachekey.Split(':')[1]);

            actionContext.Response = actionContext.Request.CreateResponse();
            actionContext.Response.Content = new StringContent(val);

            actionContext.Response.Content.Headers.ContentType = contenttype;
            actionContext.Response.Headers.ETag = (EntityTagHeaderValue)_webApiCache.Get(cachekey + Constants.EtagKey);

            var cacheTime = CacheTimeQuery.Execute(DateTime.Now);
            ApplyCacheHeaders(actionContext.Response, cacheTime);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var cacheTime = CacheTimeQuery.Execute(DateTime.Now);
            var cachekey = MakeCachekey(actionExecutedContext.Request, _responseMediaType);

            if (!(_webApiCache.Contains(cachekey)) && !string.IsNullOrWhiteSpace(cachekey))
            {
                SetEtag(actionExecutedContext);

                actionExecutedContext.Response.Content.ReadAsStringAsync().ContinueWith(t =>
                    {
                        _webApiCache.Add(cachekey, t.Result, cacheTime.AbsoluteExpiration);
                        
                        _webApiCache.Add(cachekey + Constants.ContentTypeKey,
                                        actionExecutedContext.Response.Content.Headers.ContentType.MediaType,
                                        cacheTime.AbsoluteExpiration);

                        _webApiCache.Add(cachekey + Constants.EtagKey,
                                      actionExecutedContext.Response.Headers.ETag,
                                      cacheTime.AbsoluteExpiration); 
                    });
            }

            if (!_isCachingAllowed(actionExecutedContext.ActionContext, AnonymousOnly)) return;
            ApplyCacheHeaders(actionExecutedContext.ActionContext.Response, cacheTime);
        }

        private void ApplyCacheHeaders(HttpResponseMessage response, CacheTime cacheTime)
        {
            var cachecontrol = new CacheControlHeaderValue
            {
                MaxAge = cacheTime.ClientTimeSpan,
                MustRevalidate = MustRevalidate
            };

            response.Headers.CacheControl = cachecontrol;
        }

        private static void SetEtag(HttpActionExecutedContext actionExecutedContext)
        {
            var eTag = new EntityTagHeaderValue(@"""" + Guid.NewGuid() + @"""");
            actionExecutedContext.Response.Headers.ETag = eTag;
        }
    }
}