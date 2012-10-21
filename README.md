ASP.NET Web API CacheOutput
========================

A simple filter bringing caching options, similar to MVC's "OutputCacheAttribute" to Web API ApiControllers.

Usage:

        [CacheOutput(120, 0)]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [CacheOutput(0, 60, false, false)]
        public string Get(int id)
        {
            return "value";
        }

Where the first parameter indicates caching on the server side in seconds, 
the second indicates caching on the client side in seconds, 
the third decides whether to cache for anonymous users only,
the fourth decides whether the origin server require revalidation of 
a cache entry on any subsequent use when the cache entry becomes stale.

Variation: CacheOutputUntil is used to cache data until a specific moment in time.

Usage:

    // CacheOutput until TODAY @ 17:45:00 & don't pay attention to query strings
    // [CacheOutputUntil(Hour, Minutes, Seconds, AnonymousOnly, excludeQueryStringFromCacheKey: true)]
    [CacheOutputUntil(17, 45, 00, true, excludeQueryStringFromCacheKey: true)]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    // CacheOutput until TODAY @ 17:45:00
    [CacheOutputUntil(17, 45)]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    // CacheOutput until 2012/01/01 00:00:00
    [CacheOutputUntil(2012,01,01)]
    public string Get(int id)
    {
        return "value";
    }

    // CacheOutput until (This year)/01/01 00:00:00
    [CacheOutputUntil(01,01)]
    public string Get(int id)
    {
        return "value";
    }

    // CacheOutput until (This year)/(this month)/01 00:00:00
    [CacheOutputUntil(01)]
    public string Get(int id)
    {
        return "value";
    }

    // CacheOutput until 2012/01/01 17:45:00
    [CacheOutputUntil(2012,01,01,17,45)]
    public string Get(int id)
    {
        return "value";
    }