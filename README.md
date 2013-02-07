ASP.NET Web API CacheOutput
========================

A small library bringing caching options, similar to MVC's "OutputCacheAttribute", to Web API actions.

**CacheOutput** will take care of server side caching and set the appropriate client side (response) headers for you.

You can specify the following properties:
 - *ClientTimeSpan* (corresponds to CacheControl MaxAge HTTP header)
 - *MustRevalidate* (corresponds to MustRevalidate HTTP header - indicates whether the origin server requires revalidation of 
a cache entry on any subsequent use when the cache entry becomes stale)
 - *ExcludeQueryStringFromCacheKey* (do not vary cache by querystring values)
 - *ServerTimeSpan* (time how long the response should be cached on the server side)
 - *AnonymousOnly* (cache enabled only for requests when Thread.CurrentPrincipal is not set)
 
Additionally, the library is setting ETags for you, and keeping them unchanged for the duration of the caching period.
Caching by default can only be applied to GET actions.

Installation
--------------------
You can build from the source here, or you can install the Nuget version (currently pre-release):
    
    PM> Install-Package Strathweb.CacheOutput -Pre


Usage
--------------------

	//Cache for 100s on the server, inform the client that response is valid for 100s
        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100)]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

	//Cache for 100s on the server, inform the client that response is valid for 100s. Cache for anonymous users only.
        [CacheOutput(ClientTimeSpan = 100, ServerTimeSpan = 100, AnonymousOnly = true)]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

	//Inform the client that response is valid for 50s. Force client to revalidate.
        [CacheOutput(ClientTimeSpan = 50, MustRevalidate = true)]
        public string Get(int id)
        {
            return "value";
        }

	//Cache for 50s on the server. Ignore querystring parameters when serving cached content.
        [CacheOutput(ServerTimeSpan = 50, ExcludeQueryStringFromCacheKey = true)]
        public string Get(int id)
        {
            return "value";
        }


Variations
--------------------
*CacheOutputUntil* is used to cache data until a specific moment in time. This applies to both client and server.
	
	//Cache until 01/25/2013 17:00
        [CacheOutputUntil(2013,01,25,17,00)]
        public string Get_until25012013_1700()
        {
            return "test";
        }


*CacheOutputUntilToday* is used to cache data until a specific hour later on the same day. This applies to both client and server.

	//Cache until 23:55:00 today
        [CacheOutputUntilToday(23,55)]
        public string Get_until2355_today()
        {
            return "value";
        }

*CacheOutputUntilThisMonth* is used to cache data until a specific point later this month. This applies to both client and server.

	//Cache until the 31st day of the current month
        [CacheOutputUntilThisMonth(31)]
        public string Get_until31_thismonth()
        {
            return "value";
        }

*CacheOutputUntilThisYear* is used to cache data until a specific point later this year. This applies to both client and server.

	//Cache until the 31st of July this year
        [CacheOutputUntilThisYear(7,31)]
        public string Get_until731_thisyear()
        {
            return "value";
        }

Each of these can obviously be combined with the 5 general properties mentioned in the beginning.

Caching convention
--------------------
In order to determine the expected content type of the response, **CacheOutput** will run Web APIs internal *content negotiation process*, based on the incoming request & the return type of the action on which caching is applied. 

Each individual content type response is cached separately (so out of the box, you can expect the action to be cached as JSON and XML, if you introduce more formatters, those will be cached as well).

Server side caching
--------------------
By default **CacheOutput** will use *System.Runtime.Caching.MemoryCache* to cache on the server side. However, you are free to swap this with anything else
(static Dictionary, Memcached, Redis, whatever..) as long as you implement the following *IApiOutputCache* interface (part of the distributed assembly).

    public interface IApiOutputCache
    {
        T Get<T>(string key) where T : class;
        object Get(string key);
        void Remove(string key);
        bool Contains(string key);
        void Add(string key, object o, DateTimeOffset expiration);
    }

Suppose you have a custom implementation:

    public class MyCache : IApiOutputCache {
      //omitted for brevity
    }

You can register your implementation using a handy *GlobalConfiguration* extension method:

    //instance
    configuration.RegisterCacheOutputProvider(() => new MyCache());

    //singleton
    var cache = new MyCache();
    configuration.RegisterCacheOutputProvider(() => cache);	

If you prefer **CacheOutput** to use resolve the cache implementation directly from your dependency injection provider, that's also possible. Simply register your *IApiOutputCache* implementation in your Web API DI and that's it. Whenever **CacheOutput** does not find an implementation in the *GlobalConiguration*, it will fall back to the DI resolver. Example (using Autofac for Web API):

    cache = new MyCache();
    var builder = new ContainerBuilder();
    builder.RegisterInstance(cache);
    config.DependencyResolver = new AutofacWebApiDependencyResolver(builder.Build());

If no implementation is available in neither *GlobalConfiguration* or *DependencyReolver*, we will default to *System.Runtime.Caching.MemoryCache*.

JSONP
--------------------
We automatically exclude *callback* parameter from cache key to allow for smooth JSONP support. 

So:

    /api/something?abc=1&callback=jQuery1213

is cached as:

    /api/something?abc=1

Position of the *callback* parameter does not matter.

Etags
--------------------
For client side caching, in addition to *MaxAge*, we will issue Etags. You can use the Etag value to make a request with *If-None-Match* header. If the resource is still valid, server will then response with a 304 status code.

For example:

    GET /api/myresource
    Accept: application/json

    Status Code: 200
    Cache-Control: max-age=100
    Content-Length: 24
    Content-Type: application/json; charset=utf-8
    Date: Fri, 25 Jan 2013 03:37:11 GMT
    ETag: "5c479911-97b9-4b78-ae3e-d09db420d5ba"
    Server: Microsoft-HTTPAPI/2.0

On the next request:

    GET /api/myresource
    Accept: application/json
    If-None-Match: "5c479911-97b9-4b78-ae3e-d09db420d5ba"
    
    Status Code: 304
    Cache-Control: max-age=100
    Content-Length: 0
    Date: Fri, 25 Jan 2013 03:37:13 GMT
    Server: Microsoft-HTTPAPI/2.0

License
--------------------

Licensed under Apache v2. License included.
