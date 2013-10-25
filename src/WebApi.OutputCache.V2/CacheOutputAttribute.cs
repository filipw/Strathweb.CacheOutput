using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebApi.OutputCache.Core;
using WebApi.OutputCache.Core.Cache;
using WebApi.OutputCache.Core.Time;

namespace WebApi.OutputCache.V2
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CacheOutputAttribute : FilterAttribute, IActionFilter
    {
        public bool AnonymousOnly { get; set; }
        public bool MustRevalidate { get; set; }
        public bool ExcludeQueryStringFromCacheKey { get; set; }
        public int ServerTimeSpan { get; set; }
        public int ClientTimeSpan { get; set; }
        public bool NoCache { get; set; }
        public Type CacheKeyGenerator { get; set; }
        
        private MediaTypeHeaderValue _responseMediaType;
        
        // cache repository
        private IApiOutputCache _webApiCache;

        protected virtual void EnsureCache(HttpConfiguration config, HttpRequestMessage req)
        {
            _webApiCache = config.CacheOutputConfiguration().GetCacheOutputProvider(req);
        }

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

        private void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext == null) throw new ArgumentNullException("actionContext");

            if (!_isCachingAllowed(actionContext, AnonymousOnly)) return;

            var config = actionContext.Request.GetConfiguration();

            EnsureCacheTimeQuery();
            EnsureCache(config, actionContext.Request);

            var cacheKeyGenerator = config.CacheOutputConfiguration().GetCacheKeyGenerator(actionContext.Request, CacheKeyGenerator);

            _responseMediaType = GetExpectedMediaType(config, actionContext);
            var cachekey = cacheKeyGenerator.MakeCacheKey(actionContext, _responseMediaType, ExcludeQueryStringFromCacheKey);

            if (!_webApiCache.Contains(cachekey)) return;

            if (actionContext.Request.Headers.IfNoneMatch != null)
            {
                var etag = _webApiCache.Get(cachekey + Constants.EtagKey) as string;
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

            var val = _webApiCache.Get(cachekey) as byte[];
            if (val == null) return;

            var contenttype = _webApiCache.Get(cachekey + Constants.ContentTypeKey) as string ?? cachekey.Split(':')[1];

            actionContext.Response = actionContext.Request.CreateResponse();
            actionContext.Response.Content = new ByteArrayContent(val);

            actionContext.Response.Content.Headers.ContentType = new MediaTypeHeaderValue(contenttype);
            var responseEtag = _webApiCache.Get(cachekey + Constants.EtagKey) as string;
            if (responseEtag != null) SetEtag(actionContext.Response,  responseEtag);

            var cacheTime = CacheTimeQuery.Execute(DateTime.Now);
            ApplyCacheHeaders(actionContext.Response, cacheTime);
        }

        private async Task OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.ActionContext.Response == null || !actionExecutedContext.ActionContext.Response.IsSuccessStatusCode) return;

            if (!_isCachingAllowed(actionExecutedContext.ActionContext, AnonymousOnly)) return;

            var cacheTime = CacheTimeQuery.Execute(DateTime.Now);
            if (cacheTime.AbsoluteExpiration > DateTime.Now)
            {
                var config = actionExecutedContext.Request.GetConfiguration().CacheOutputConfiguration();
                var cacheKeyGenerator = config.GetCacheKeyGenerator(actionExecutedContext.Request, CacheKeyGenerator);

                var cachekey = cacheKeyGenerator.MakeCacheKey(actionExecutedContext.ActionContext, _responseMediaType, ExcludeQueryStringFromCacheKey);

                if (!string.IsNullOrWhiteSpace(cachekey) && !(_webApiCache.Contains(cachekey)))
                {
                    SetEtag(actionExecutedContext.Response, Guid.NewGuid().ToString());

                    if (actionExecutedContext.Response.Content != null)
                    {
                        var content = await actionExecutedContext.Response.Content.ReadAsByteArrayAsync();
                        var baseKey = config.MakeBaseCachekey(actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerName, actionExecutedContext.ActionContext.ActionDescriptor.ActionName);

                        _webApiCache.Add(baseKey, string.Empty, cacheTime.AbsoluteExpiration);
                        _webApiCache.Add(cachekey, content, cacheTime.AbsoluteExpiration, baseKey);

                        _webApiCache.Add(cachekey + Constants.ContentTypeKey,
                                        actionExecutedContext.Response.Content.Headers.ContentType.MediaType,
                                        cacheTime.AbsoluteExpiration, baseKey);

                        _webApiCache.Add(cachekey + Constants.EtagKey,
                                        actionExecutedContext.Response.Headers.ETag.Tag,
                                        cacheTime.AbsoluteExpiration, baseKey);
                    }
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
            else if (NoCache)
            {
                response.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
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

        Task<HttpResponseMessage> IActionFilter.ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            if (continuation == null)
            {
                throw new ArgumentNullException("continuation");
            }

            OnActionExecuting(actionContext);

            if (actionContext.Response != null)
            {
                return Task.FromResult(actionContext.Response);
            }

            return CallOnActionExecutedAsync(actionContext, cancellationToken, continuation);
        }

        private async Task<HttpResponseMessage> CallOnActionExecutedAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            cancellationToken.ThrowIfCancellationRequested();

            HttpResponseMessage response = null;
            Exception exception = null;
            try
            {
                response = await continuation();
            }
            catch (Exception e)
            {
                exception = e;
            }

            try
            {
                var executedContext = new HttpActionExecutedContext(actionContext, exception) { Response = response };
                await OnActionExecuted(executedContext);

                if (executedContext.Response != null)
                {
                    return executedContext.Response;
                }

                if (executedContext.Exception != null)
                {
                    throw executedContext.Exception;
                }
            }
            catch
            {
                actionContext.Response = null;
                throw;
            }

            throw new InvalidOperationException(GetType().Name);
        }

    }
} 
