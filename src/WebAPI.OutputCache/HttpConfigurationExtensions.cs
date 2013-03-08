using System.Web.Http;

namespace WebAPI.OutputCache
{
    public static class HttpConfigurationExtensions
    {
        public static CacheOutputConfiguration CacheOutputConfiguration(this HttpConfiguration config)
        {
            return new CacheOutputConfiguration(config);
        }
    }
}