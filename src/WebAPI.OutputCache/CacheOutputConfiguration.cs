using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Web.Http;
using WebAPI.OutputCache.Cache;

namespace WebAPI.OutputCache
{
    public class CacheOutputConfiguration
    {
        private readonly HttpConfiguration _configuration;

        public CacheOutputConfiguration(HttpConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void RegisterCacheOutputProvider(Func<IApiOutputCache> provider)
        {
            _configuration.Properties.GetOrAdd(typeof(IApiOutputCache), x => provider);
        }

        public string MakeBaseCachekey(string controller, string action)
        {
            return string.Format("{0}-{1}", controller.ToLower(), action.ToLower());
        }

        public string MakeBaseCachekey<T, U>(Expression<Func<T, U>> expression)
        {
            var method = expression.Body as MethodCallExpression;
            if (method == null) throw new ArgumentException("Expression is wrong");

            var methodName = method.Method.Name;
            var nameAttribs = method.Method.GetCustomAttributes(typeof(ActionNameAttribute), false);
            if (nameAttribs.Any())
            {
                var actionNameAttrib = (ActionNameAttribute) nameAttribs.FirstOrDefault();
                if (actionNameAttrib != null)
                {
                    methodName = actionNameAttrib.Name;
                }
            }

            return string.Format("{0}-{1}", typeof(T).Name.Replace("Controller",string.Empty).ToLower(), methodName.ToLower());
        }

        public IApiOutputCache GetCacheOutputProvider(HttpRequestMessage request)
        {
            object cache;
            _configuration.Properties.TryGetValue(typeof(IApiOutputCache), out cache);

            var cacheFunc = cache as Func<IApiOutputCache>;

            var cacheOutputProvider = cacheFunc != null ? cacheFunc() : request.GetDependencyScope().GetService(typeof(IApiOutputCache)) as IApiOutputCache ?? new MemoryCacheDefault();
            return cacheOutputProvider;
        }
    }
}