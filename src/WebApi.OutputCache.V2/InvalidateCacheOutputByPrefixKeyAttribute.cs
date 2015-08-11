using System;
using System.Net.Http;
using System.Web.Http.Filters;

namespace WebApi.OutputCache.V2
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class InvalidateCacheOutputByPrefixKeyAttribute : BaseCacheAttribute
    {
        private readonly IKeyPrefixGenerator _keyPrefixPrefixGenerator;
        private string _controller;
        private readonly string _actionName;
        
        public InvalidateCacheOutputByPrefixKeyAttribute(Type generatorKeyType, string actionName)
            : this(generatorKeyType, actionName, null)
        {
        }

        public InvalidateCacheOutputByPrefixKeyAttribute(Type generatorKeyType, string actionName, Type controllerType = null)
        {
            _keyPrefixPrefixGenerator = Activator.CreateInstance(generatorKeyType) as IKeyPrefixGenerator;
            _controller = controllerType != null ? controllerType.Name.Replace("Controller", string.Empty) : null;
            _actionName = actionName;
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response != null && !actionExecutedContext.Response.IsSuccessStatusCode)
            {
                return;
            }

            var httpActionContext = actionExecutedContext.ActionContext;
            _controller = _controller ?? actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerType.FullName;
            var baseCacheKey = actionExecutedContext.Request.GetConfiguration().CacheOutputConfiguration().MakeBaseCachekey(_controller, _actionName);
            
            var prefix = _keyPrefixPrefixGenerator.Generate(httpActionContext, baseCacheKey);

            var config = actionExecutedContext.Request.GetConfiguration();
           
            EnsureCache(config, actionExecutedContext.Request);

            var keys = WebApiCache.FindKeysStartingWith(prefix);
            foreach (var key in keys)
            {
                WebApiCache.Remove(key);
            }
        }
    }
}