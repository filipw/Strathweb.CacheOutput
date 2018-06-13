using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApi.OutputCache.V2
{
    public class DefaultCacheKeyGenerator : ICacheKeyGenerator
    {
        public virtual string MakeCacheKey(HttpActionContext context, MediaTypeHeaderValue mediaType, bool excludeQueryString = false)
        {
            var key = MakeBaseKey(context);
            var parameters = FormatParameters(context, excludeQueryString);

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
            if (excludeQueryString || !(context.Request.Method == HttpMethod.Post || context.Request.Method == HttpMethod.Put))
                return parameters;

            var postBody = GetPostBody(context);
            if (string.IsNullOrEmpty(postBody))
                return parameters;

            var postParameters = JObject.Parse(postBody);
            if (postParameters == null)
                return parameters;
            parameters += FormatPostParameters(postParameters);
            return parameters;
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

        private static string FormatPostParameters(JObject postParameters)
        {
            var parametersAsString = "-";
            foreach (var param in postParameters)
            {
                var val = param.Value?.ToString(Formatting.None);
                if (string.IsNullOrEmpty(param.Key) || string.IsNullOrEmpty(val))
                    continue;

                val = val.Replace(Environment.NewLine, "")
                    .Replace(":", "=")
                    .Replace("{", "")
                    .Replace("}", "")
                    .Replace(",", "_")
                    .Replace("[", "")
                    .Replace("]", "")
                    .Replace("\"", "")
                    .Trim()
                    .ToLower();
                if (string.IsNullOrEmpty(val))
                    continue;

                parametersAsString += param.Key.ToLower() + "=" + val + "-";
            }

            return parametersAsString.TrimEnd('-');
        }

        private static string GetPostBody(HttpActionContext context)
        {
            if (context.Request.Content == null)
                return null;
            string postBody;
            using (var stream = new MemoryStream())
            {
                var inputStream = context.Request.Content.ReadAsStreamAsync().Result;
                inputStream.Seek(0, SeekOrigin.Begin);
                inputStream.CopyTo(stream);
                postBody = Encoding.UTF8.GetString(stream.ToArray());
            }

            return postBody;
        }
    }
}