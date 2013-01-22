namespace WebAPI.OutputCache
{
    public class CacheOutputUntilAttribute : CacheOutputAttribute
    {
        /// <summary>
        /// Cache item until absolute expiration 2012/01/01 @ 17h45
        /// </summary>
        /// <param name="year">2012</param>
        /// <param name="month">1</param>
        /// <param name="day">1</param>
        /// <param name="hour">17</param>
        /// <param name="minute">45</param>
        /// <param name="second">0</param>
        /// <param name="anonymousOnly">only cache requests from annonymous clients</param>
        /// <param name="mustRevalidate"> whether the origin server require revalidation of a cache entry on any subsequent use when the cache entry becomes stale.</param>
        /// <param name="excludeQueryStringFromCacheKey">will exclude the query string from the cache key</param>
        public CacheOutputUntilAttribute(int year,
                                         int month,
                                         int day,
                                         int hour = 0,
                                         int minute = 0,
                                         int second = 0,
                                         bool anonymousOnly = false,
                                         bool mustRevalidate = false,
                                         bool excludeQueryStringFromCacheKey = false)
            : base(anonymousOnly: anonymousOnly, mustRevalidate: mustRevalidate, excludeQueryStringFromCacheKey: excludeQueryStringFromCacheKey)
        {
            CacheTimeQuery = new SpecificTime(year, month, day, hour, minute, second);
        }

        /// <summary>
        /// Cache item until absolute expiration THIS YEAR / 01 / 01 @ 17h45
        /// </summary>
        /// <param name="month">1</param>
        /// <param name="day">1</param>
        /// <param name="hour">17</param>
        /// <param name="minute">45</param>
        /// <param name="second">0</param>
        /// <param name="anonymousOnly">only cache requests from annonymous clients</param>
        /// <param name="mustRevalidate"> whether the origin server require revalidation of a cache entry on any subsequent use when the cache entry becomes stale.</param>
        /// <param name="excludeQueryStringFromCacheKey">will exclude the query string from the cache key</param>
        public CacheOutputUntilAttribute(int month,
                                         int day,
                                         int hour = 0,
                                         int minute = 0,
                                         int second = 0,
                                         bool anonymousOnly = false,
                                         bool mustRevalidate = false,
                                         bool excludeQueryStringFromCacheKey = false)
            : base(anonymousOnly: anonymousOnly, mustRevalidate: mustRevalidate, excludeQueryStringFromCacheKey: excludeQueryStringFromCacheKey)
        {
            CacheTimeQuery = new ThisYear(month, day, hour, minute, second);
        }

        /// <summary>
        /// Cache item until absolute expiration THIS YEAR / THIS MONTH / 01 @ 17h45
        /// </summary>
        /// <param name="day">1</param>
        /// <param name="hour">17</param>
        /// <param name="minute">45</param>
        /// <param name="second">0</param>
        /// <param name="anonymousOnly">only cache requests from annonymous clients</param>
        /// <param name="mustRevalidate"> whether the origin server require revalidation of a cache entry on any subsequent use when the cache entry becomes stale.</param>
        /// <param name="excludeQueryStringFromCacheKey">will exclude the query string from the cache key</param>
        public CacheOutputUntilAttribute(int day,
                                         int hour = 0,
                                         int minute = 0,
                                         int second = 0,
                                         bool anonymousOnly = false,
                                         bool mustRevalidate = false,
                                         bool excludeQueryStringFromCacheKey = false)
            : base(anonymousOnly: anonymousOnly, mustRevalidate: mustRevalidate, excludeQueryStringFromCacheKey: excludeQueryStringFromCacheKey)
        {
            CacheTimeQuery = new ThisMonth(day, hour, minute, second);
        }

        /// <summary>
        /// Cache item until absolute expiration today @ 17h45
        /// </summary>
        /// <param name="hour">17</param>
        /// <param name="minute">45</param>
        /// <param name="second">0</param>
        /// <param name="anonymousOnly">only cache requests from annonymous clients</param>
        /// <param name="mustRevalidate"> whether the origin server require revalidation of a cache entry on any subsequent use when the cache entry becomes stale.</param>
        /// <param name="excludeQueryStringFromCacheKey">will exclude the query string from the cache key</param>
        public CacheOutputUntilAttribute(int hour = 23,
                                         int minute = 59,
                                         int second = 59,
                                         bool anonymousOnly = false,
                                         bool mustRevalidate = false,
                                         bool excludeQueryStringFromCacheKey = false)
            : base(anonymousOnly: anonymousOnly, mustRevalidate: mustRevalidate, excludeQueryStringFromCacheKey: excludeQueryStringFromCacheKey)
        {
            CacheTimeQuery = new ThisDay(hour, minute, second);
        }
    }
}