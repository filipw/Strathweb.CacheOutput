using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;

namespace WebApi.OutputCache.V2
{
    public class DefaultCacheKeyGenerator : ICacheKeyGenerator
    {
        public virtual string MakeCacheKey(HttpActionContext context, MediaTypeHeaderValue mediaType, bool excludeQueryString, Dictionary<string, List<string>> headers)
        {
            var key = MakeBaseKey(context);
            var parameters = FormatParameters(context, excludeQueryString);
            string custHeaders = GetCustomHeaders(headers);

            if (!(string.IsNullOrEmpty(custHeaders)))
                return string.Format("{0}{1}:{2}:{3}", key, parameters, custHeaders, mediaType);
            else
                return string.Format("{0}{1}:{2}", key, parameters, mediaType);
        }

        protected virtual string MakeBaseKey(HttpActionContext context)
        {
            var controller = context.ControllerContext.ControllerDescriptor.ControllerType.FullName;
            var action = context.ActionDescriptor.ActionName;
            return context.Request.GetConfiguration().CacheOutputConfiguration().MakeBaseCachekey(controller, action);
        }

        protected virtual string FormatParameters(HttpActionContext context, bool excludeQueryString)
        {
            var actionParameters = context.ActionArguments.Where(x => x.Value != null).Select(x => x.Key + "=" + GetValue(x.Value));

            string parameters;

            if (!excludeQueryString)
            {
                var queryStringParameters =
                    context.Request.GetQueryNameValuePairs()
                           .Where(x => x.Key.ToLower() != "callback")
                           .Select(x => x.Key + "=" + x.Value);
                var parametersCollections = actionParameters.Union(queryStringParameters);
                parameters = "-" + string.Join("&", parametersCollections);

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
            else
            {
                parameters = "-" + string.Join("&", actionParameters);
            }

            if (parameters == "-") parameters = string.Empty;
            return parameters;
        }
        protected virtual string GetCustomHeaders(Dictionary<string, List<string>> headers)
        {
            //string returnValue = string.Empty;

            if (!(headers == null))
            {
                //foreach(var item in headers)
                //{
                //    if (!(returnValue.Length > 0))
                //    {
                //        //first group and needs to not include the ;
                //        returnValue = item.ToString();

                //    }
                //    else
                //    {
                //        returnValue = string.Format("{0}|{1}={2}", returnValue, item.Key, item.Value);
                //    }
                //}
                return string.Join("&", headers.Select(x => x.Key.ToLower() + "=" + GetValue(x.Value)));
            }
            else
            {
                return string.Empty;
            }

            //return returnValue;
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
    }
}