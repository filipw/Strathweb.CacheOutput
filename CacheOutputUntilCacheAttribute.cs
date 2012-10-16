using System;

namespace WebApi.OutputCache
{
    public class CacheOutputUntilCacheAttribute : OutputCacheAttribute
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
        public CacheOutputUntilCacheAttribute(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, bool anonymousOnly = false, bool mustRevalidate = false)
            : base(anonymousOnly: anonymousOnly, mustRevalidate: mustRevalidate)
        {
            AbsoluteExpiration = new DateTime(year,
                                              month,
                                              day,
                                              hour,
                                              minute,
                                              second);

            Timespan = AbsoluteExpiration.Subtract(DateTime.Now);
            ClientTimeSpan = Timespan;
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
        public CacheOutputUntilCacheAttribute(int month, int day, int hour = 0, int minute = 0, int second = 0, bool anonymousOnly = false, bool mustRevalidate = false)
            : base(anonymousOnly: anonymousOnly, mustRevalidate: mustRevalidate)
        {
            AbsoluteExpiration = new DateTime(DateTime.Now.Year,
                                              month,
                                              day,
                                              hour,
                                              minute,
                                              second);

            Timespan = AbsoluteExpiration.Subtract(DateTime.Now);
            ClientTimeSpan = Timespan;
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
        public CacheOutputUntilCacheAttribute(int day, int hour = 0, int minute = 0, int second = 0, bool anonymousOnly = false, bool mustRevalidate = false)
            : base(anonymousOnly: anonymousOnly, mustRevalidate: mustRevalidate)
        {
            AbsoluteExpiration = new DateTime(DateTime.Now.Year,
                                              DateTime.Now.Month,
                                              day,
                                              hour,
                                              minute,
                                              second);

            Timespan = AbsoluteExpiration.Subtract(DateTime.Now);
            ClientTimeSpan = Timespan;
        }

        /// <summary>
        /// Cache item until absolute expiration today @ 17h45
        /// </summary>
        /// <param name="hour">17</param>
        /// <param name="minute">45</param>
        /// <param name="second">0</param>
        /// <param name="anonymousOnly">only cache requests from annonymous clients</param>
        /// <param name="mustRevalidate"> whether the origin server require revalidation of a cache entry on any subsequent use when the cache entry becomes stale.</param>
        public CacheOutputUntilCacheAttribute(int hour = 0, int minute = 0, int second = 0, bool anonymousOnly = false, bool mustRevalidate = false)
            : base(anonymousOnly: anonymousOnly, mustRevalidate: mustRevalidate)
        {
            AbsoluteExpiration = new DateTime(DateTime.Now.Year,
                                              DateTime.Now.Month,
                                              DateTime.Now.Day,
                                              hour,
                                              minute,
                                              second);

            Timespan = AbsoluteExpiration.Subtract(DateTime.Now);
            ClientTimeSpan = Timespan;
        }
    }
}