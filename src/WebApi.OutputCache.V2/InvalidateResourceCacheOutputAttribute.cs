using System;
using System.Net.Http;
using System.Web.Http.Filters;

namespace WebApi.OutputCache.V2
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class InvalidateResourceCacheOutputAttribute : BaseCacheAttribute
    {
        private readonly IEndpointGenerator generator;

        public InvalidateResourceCacheOutputAttribute()
        {
        }

        public InvalidateResourceCacheOutputAttribute(Type type)
        {
            generator = Activator.CreateInstance(type) as IEndpointGenerator;
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response != null && !actionExecutedContext.Response.IsSuccessStatusCode)
            {
                return;
            }

            var endpoint = generator == null ?
                actionExecutedContext.Request.RequestUri.AbsolutePath.ToLower() :
                generator.Generate(actionExecutedContext.ActionContext.ActionArguments);

            var config = actionExecutedContext.Request.GetConfiguration();
           
            EnsureCache(config, actionExecutedContext.Request);

            var keys = WebApiCache.FindKeysStartingWith(endpoint);
            foreach (var key in keys)
            {
                WebApiCache.Remove(key);
            }
        }
    }
}