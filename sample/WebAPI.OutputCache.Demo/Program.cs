using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.SelfHost;
using WebAPI.OutputCache.Cache;

namespace WebAPI.OutputCache.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new HttpSelfHostConfiguration("http://localhost:999");
            config.Routes.MapHttpRoute(
                  name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            var server = new HttpSelfHostServer(config);

            config.CacheOutputConfiguration().RegisterCacheOutputProvider(() => new MemoryCacheDefault());

            server.OpenAsync().Wait();

            Console.ReadKey();

            server.CloseAsync().Wait();
        }
    }
}
