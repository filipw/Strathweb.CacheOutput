using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebApi.OutputCache.Core;
using WebApi.OutputCache.Core.Time;

namespace WebAPI.OutputCache
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CacheOutputAttribute : BaseCacheAttribute
    {
        protected static MediaTypeHeaderValue DefaultMediaType = new MediaTypeHeaderValue("application/json");

        /// <summary>
        /// Cache enabled only for requests when Thread.CurrentPrincipal is not set
        /// </summary>
        public bool AnonymousOnly { get; set; }

        /// <summary>
        /// Corresponds to MustRevalidate HTTP header - indicates whether the origin server requires revalidation of a cache entry on any subsequent use when the cache entry becomes stale
        /// </summary>
        public bool MustRevalidate { get; set; }

        /// <summary>
        /// Do not vary cache by querystring values
        /// </summary>
        public bool ExcludeQueryStringFromCacheKey { get; set; }
        
        /// <summary>
        /// How long response should be cached on the server side (in seconds)
        /// </summary>
        public int ServerTimeSpan { get; set; }

        /// <summary>
        /// Corresponds to CacheControl MaxAge HTTP header (in seconds)
        /// </summary>
        public int ClientTimeSpan { get; set; }

        /// <summary>
        /// Corresponds to CacheControl NoCache HTTP header
        /// </summary>
		public bool NoCache { get; set; }

        /// <summary>
        /// Corresponds to CacheControl Private HTTP header. Response can be cached by browser but not by intermediary cache
        /// </summary>
        public bool Private { get; set; }

        /// <summary>
        /// Class used to generate caching keys
        /// </summary>
        public Type CacheKeyGenerator { get; set; }

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
            if (CacheTimeQuery == null) ResetCacheTimeQuery();
        }

        protected void ResetCacheTimeQuery()
        {
            CacheTimeQuery = new ShortTime( ServerTimeSpan, ClientTimeSpan );
        }

        protected virtual MediaTypeHeaderValue GetExpectedMediaType(HttpConfiguration config, HttpActionContext actionContext)
        {
            MediaTypeHeaderValue responseMediaType = null;

            var negotiator = config.Services.GetService(typeof(IContentNegotiator)) as IContentNegotiator;
            var returnType = actionContext.ActionDescriptor.ReturnType;

            if (negotiator != null && returnType != typeof(HttpResponseMessage))
            {
                var negotiatedResult = negotiator.Negotiate(returnType, actionContext.Request, config.Formatters);
                responseMediaType = negotiatedResult.MediaType;
                responseMediaType.CharSet = Encoding.UTF8.HeaderName;
            }
            else
            {
                if (actionContext.Request.Headers.Accept != null)
                {
                    responseMediaType = actionContext.Request.Headers.Accept.FirstOrDefault();
                    if (responseMediaType == null ||
                        !config.Formatters.Any(x => x.SupportedMediaTypes.Contains(responseMediaType)))
                    {
                        DefaultMediaType.CharSet = Encoding.UTF8.HeaderName;
                        return DefaultMediaType;
                    }
                }
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

            var cacheKeyGenerator = config.CacheOutputConfiguration().GetCacheKeyGenerator(actionContext.Request, CacheKeyGenerator);

            _responseMediaType = GetExpectedMediaType(config, actionContext);
            var cachekey = cacheKeyGenerator.MakeCacheKey(actionContext, _responseMediaType, ExcludeQueryStringFromCacheKey);

            if (!WebApiCache.Contains(cachekey)) return;

            if (actionContext.Request.Headers.IfNoneMatch != null)
            {
                var etag = WebApiCache.Get(cachekey + Constants.EtagKey) as string;
                if (etag != null)
                {
                    if (actionContext.Request.Headers.IfNoneMatch.Any(x => x.Tag ==  etag))
                    {
                        var time = CacheTimeQuery.Execute(DateTime.Now);
                        var quickResponse = actionContext.Request.CreateResponse(HttpStatusCode.NotModified);
                        ApplyCacheHeaders(quickResponse, time);
                        actionContext.Response = quickResponse;
                        return;
                    }
                }
            }

            var val = WebApiCache.Get(cachekey) as byte[];
            if (val == null) return;

            var contenttype = WebApiCache.Get(cachekey + Constants.ContentTypeKey) as string ?? cachekey.Split(':')[1];

            actionContext.Response = actionContext.Request.CreateResponse();
            actionContext.Response.Content = new ByteArrayContent(val);

            actionContext.Response.Content.Headers.ContentType = new MediaTypeHeaderValue(contenttype);
            var responseEtag = WebApiCache.Get(cachekey + Constants.EtagKey) as string;
            if (responseEtag != null) SetEtag(actionContext.Response,  responseEtag);

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
                var config = actionExecutedContext.Request.GetConfiguration().CacheOutputConfiguration();
                var cacheKeyGenerator = config.GetCacheKeyGenerator(actionExecutedContext.Request, CacheKeyGenerator);

                var cachekey = cacheKeyGenerator.MakeCacheKey(actionExecutedContext.ActionContext, _responseMediaType, ExcludeQueryStringFromCacheKey);

                if (!string.IsNullOrWhiteSpace(cachekey) && !(WebApiCache.Contains(cachekey)))
                {
                    SetEtag(actionExecutedContext.Response, Guid.NewGuid().ToString());

                    if (actionExecutedContext.Response.Content != null)
                    {
                        actionExecutedContext.Response.Content.ReadAsByteArrayAsync().ContinueWith(t =>
                            {
                                var baseKey = config.MakeBaseCachekey(actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerName, actionExecutedContext.ActionContext.ActionDescriptor.ActionName);
                                
                                WebApiCache.Add(baseKey, string.Empty, cacheTime.AbsoluteExpiration);
                                WebApiCache.Add(cachekey, t.Result, cacheTime.AbsoluteExpiration, baseKey);

                                WebApiCache.Add(cachekey + Constants.ContentTypeKey,
                                                actionExecutedContext.Response.Content.Headers.ContentType.MediaType,
                                                cacheTime.AbsoluteExpiration, baseKey);

                                WebApiCache.Add(cachekey + Constants.EtagKey,
                                                actionExecutedContext.Response.Headers.ETag.Tag,
                                                cacheTime.AbsoluteExpiration, baseKey);
                            });
                    }
                }
            }

            ApplyCacheHeaders(actionExecutedContext.ActionContext.Response, cacheTime);
        }

        private void ApplyCacheHeaders(HttpResponseMessage response, CacheTime cacheTime)
        {
            if (cacheTime.ClientTimeSpan > TimeSpan.Zero || MustRevalidate || Private)
            {
                var cachecontrol = new CacheControlHeaderValue
                                       {
                                           MaxAge = cacheTime.ClientTimeSpan,
                                           MustRevalidate = MustRevalidate,
                                           Private = Private
                                       };

                response.Headers.CacheControl = cachecontrol;
			}
			else if (NoCache)
			{
				response.Headers.CacheControl = new CacheControlHeaderValue {NoCache = true};
				response.Headers.Add("Pragma", "no-cache");
			}
        }

        private static void SetEtag(HttpResponseMessage message, string etag)
        {
            if (etag != null)
            {
                var eTag = new EntityTagHeaderValue(@"""" + etag.Replace("\"", string.Empty) + @"""");
                message.Headers.ETag = eTag;
            }
        }
    }
} 
