ASP.NET Web API OutputCache
========================

A simple filter bringing caching options, similar to MVC's "OutputCacheAttribute" to Web API ApiControllers.

Usage:

        [OutputCache(120, 0)]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [OutputCache(0, 60, false, false)]
        public string Get(int id)
        {
            return "value";
        }

Where the first parameter indicates caching on the server side in seconds, 
the second indicates caching on the client side in seconds, 
the third decides whether to cache for anonymous users only,
the fourth decides whether the origin server require revalidation of a cache entry on any subsequent use when the cache entry becomes stale.

Variation: CacheUntil is used to cache data until a specific moment in time.

Usage:

		// CacheOutput unitl TODAY @ 17:45:00
        [CacheOutputUntil(17, 45)]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

		// CacheOutput unitl 2012/01/01 00:00:00
        [CacheOutputUntil(2012,01,01)]
        public string Get(int id)
        {
            return "value";
        }

		// CacheOutput unitl (This year)/01/01 00:00:00
        [CacheOutputUntil(01,01)]
        public string Get(int id)
        {
            return "value";
        }

		// CacheOutput unitl (This year)/(this month)/01 00:00:00
        [CacheOutputUntil(01)]
        public string Get(int id)
        {
            return "value";
        }

		// CacheOutput unitl 2012/01/01 17:45:00
		[CacheOutputUntil(2012,01,01,17,45)]
        public string Get(int id)
        {
            return "value";
        }