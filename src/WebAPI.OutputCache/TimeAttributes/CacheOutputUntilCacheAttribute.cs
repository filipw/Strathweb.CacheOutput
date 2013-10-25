using WebApi.OutputCache.Core.Time;

namespace WebAPI.OutputCache.TimeAttributes
{
    public sealed class CacheOutputUntilAttribute : CacheOutputAttribute
    {
        /// <summary>
        ///     Cache item until absolute expiration 2012/01/01 @ 17h45
        /// </summary>
        /// <param name="year">2012</param>
        /// <param name="month">1</param>
        /// <param name="day">1</param>
        /// <param name="hour">17</param>
        /// <param name="minute">45</param>
        /// <param name="second">0</param>
        public CacheOutputUntilAttribute(int year,
                                         int month,
                                         int day,
                                         int hour = 0,
                                         int minute = 0,
                                         int second = 0)
        {
            CacheTimeQuery = new SpecificTime(year, month, day, hour, minute, second);
        }
    }
}