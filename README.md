ASP.NET Web API OutputCache
========================

A simple filter bringing caching options, similar to MVC's "OutputCacheAttribute" to Web API ApiControllers.

Usage:

        [WebApiOutputCache(120, 0, false)]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [WebApiOutputCache(0, 60, false)]
        public string Get(int id)
        {
            return "value";
        }

Where the first parameter indicates caching on the server side in seconds, the second indicates caching on the client side in seconds, and the third decides whether to cache for anonymous users only.