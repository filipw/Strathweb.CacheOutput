using WebApi.OutputCache.Core.Time;

namespace WebApi.OutputCache.V2.TimeAttributes
{
    public sealed class CacheOutputUntilThisYearAttribute : CacheOutputAttribute
    {
        /// <summary>
        ///     Cache item until absolute expiration THIS YEAR / 01 / 01 @ 17h45
        /// </summary>
        /// <param name="month">1</param>
        /// <param name="day">1</param>
        /// <param name="hour">17</param>
        /// <param name="minute">45</param>
        /// <param name="second">0</param>
        public CacheOutputUntilThisYearAttribute(int month,
                                                 int day,
                                                 int hour = 0,
                                                 int minute = 0,
                                                 int second = 0)
        {
            CacheTimeQuery = new ThisYear(month, day, hour, minute, second);
        }
    }
}