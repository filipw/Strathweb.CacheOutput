using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace WebApi.OutputCache.V2
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class InvalidateCacheOutputAttribute : BaseCacheAttribute
    {
        private string _controller;
        private readonly string _methodName;

        public InvalidateCacheOutputAttribute(string methodName)
            : this(methodName, null)
        {
        }

        public InvalidateCacheOutputAttribute(string methodName, Type type = null)
        {
            _controller = type != null ? type.FullName : null;
            _methodName = methodName;
        }

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            if (actionExecutedContext.Response != null && !actionExecutedContext.Response.IsSuccessStatusCode) return;
            _controller = _controller ?? actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerType.FullName;

            var config = actionExecutedContext.Request.GetConfiguration();
            EnsureCache(config, actionExecutedContext.Request);

            var key = actionExecutedContext.Request.GetConfiguration().CacheOutputConfiguration().MakeBaseCachekey(_controller, _methodName);
            if (await WebApiCache.ContainsAsync(key))
            {
                await WebApiCache.RemoveStartsWithAsync(key);
            }
        }
    }
}