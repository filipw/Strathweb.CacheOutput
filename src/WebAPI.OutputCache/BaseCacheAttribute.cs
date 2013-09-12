using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebAPI.OutputCache.Cache;

namespace WebAPI.OutputCache
{
    public abstract class BaseCacheAttribute : ActionFilterAttribute
    {
        // cache repository
        protected IApiOutputCache WebApiCache;

        protected virtual string MakeCachekey(HttpActionContext context, MediaTypeHeaderValue mediaType, bool excludeQueryString = false)
        {
            var controller = context.ControllerContext.ControllerDescriptor.ControllerName;
            var action = context.ActionDescriptor.ActionName;
            var key = context.Request.GetConfiguration().CacheOutputConfiguration().MakeBaseCachekey(controller, action);
            var parametersCollections = context.ActionArguments.Where(x => x.Value != null).Select(x => x.Key + "=" + GetValue(x.Value));
            var parameters = "-"+string.Join("&", parametersCollections);

            if (excludeQueryString)
            {
                parameters = string.Empty;
            }
            else
            {
                var callbackValue = GetJsonpCallback(context.Request);
                if (!string.IsNullOrWhiteSpace(callbackValue))
                {
                    var callback = "callback=" + callbackValue;
                    if (parameters.Contains("&" + callback)) parameters = parameters.Replace("&" + callback, string.Empty);
                    if (parameters.Contains(callback + "&")) parameters = parameters.Replace(callback + "&", string.Empty);
                    if (parameters.Contains("-" + callback)) parameters = parameters.Replace("-" + callback, string.Empty);
                    if (parameters.EndsWith("&")) parameters = parameters.TrimEnd('&');
                }
            }

            if (parameters == "-") parameters = string.Empty;

            var cachekey = string.Format("{0}{1}:{2}", key, parameters, mediaType.MediaType);
            return cachekey;
        }

        private string GetValue(object val)
        {
            if (val is IEnumerable && !(val is string))
            {
                var concatValue = string.Empty;
                var paramArray = val as IEnumerable;
                return paramArray.Cast<object>().Aggregate(concatValue, (current, paramValue) => current + (paramValue + ";"));
            }
            return val.ToString();
        }

        protected virtual void EnsureCache(HttpConfiguration config, HttpRequestMessage req)
        {
            WebApiCache = config.CacheOutputConfiguration().GetCacheOutputProvider(req);
        }

        private string GetJsonpCallback(HttpRequestMessage request)
        {
            var callback = string.Empty;
            if (request.Method == HttpMethod.Get)
            {
                var query = request.GetQueryNameValuePairs();

                if (query != null)
                {
                    var queryVal = query.FirstOrDefault(x => x.Key.ToLower() == "callback");
                    if (!queryVal.Equals(default(KeyValuePair<string, string>))) callback = queryVal.Value;
                }
            }
            return callback;
        }
    }
}