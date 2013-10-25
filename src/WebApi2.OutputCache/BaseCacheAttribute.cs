using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using WebAPI.OutputCache.Cache;

namespace WebAPI.OutputCache
{
    public abstract class BaseCacheAttribute : ActionFilterAttribute
    {
        // cache repository
        protected IApiOutputCache WebApiCache;

        protected virtual void EnsureCache(HttpConfiguration config, HttpRequestMessage req)
        {
            WebApiCache = config.CacheOutputConfiguration().GetCacheOutputProvider(req);
        }
    }
}