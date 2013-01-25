using System;
using System.Web.Http;
using WebAPI.OutputCache.Cache;

namespace WebAPI.OutputCache
{
    public static class HttpConfigurationExtensions
    {
        public static void RegisterCacheOutputProvider(this HttpConfiguration config, Func<IApiOutputCache> provider)
        {
            config.Properties.GetOrAdd(typeof (IApiOutputCache), x => provider);
        }
    }
}