using System;
using System.Web.Http;
using System.Web.Http.SelfHost;
using WebApi.OutputCache.Core.Cache;

namespace WebApi.OutputCache.V2.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            const string hostUrl = "http://localhost:999";
            var config = new HttpSelfHostConfiguration(hostUrl);
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                  name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            var server = new HttpSelfHostServer(config);

            config.CacheOutputConfiguration().RegisterCacheOutputProvider(() => new MemoryCacheDefault());

            server.OpenAsync().Wait();

            Console.WriteLine($"WebAPI Hosted and listening on: {hostUrl}");
            Console.ReadKey();

            server.CloseAsync().Wait();
        }
    }
}
