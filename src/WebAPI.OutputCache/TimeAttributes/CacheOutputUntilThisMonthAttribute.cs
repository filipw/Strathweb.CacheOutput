using WebApi.OutputCache.Core.Time;

namespace WebAPI.OutputCache.TimeAttributes
{
    public sealed class CacheOutputUntilThisMonthAttribute : CacheOutputAttribute
    {
        /// <summary>
        ///     Cache item until absolute expiration THIS YEAR / THIS MONTH / 01 @ 17h45
        /// </summary>
        /// <param name="day">1</param>
        /// <param name="hour">17</param>
        /// <param name="minute">45</param>
        /// <param name="second">0</param>
        public CacheOutputUntilThisMonthAttribute(int day,
                                                  int hour = 0,
                                                  int minute = 0,
                                                  int second = 0)
        {
            CacheTimeQuery = new ThisMonth(day, hour, minute, second);
        }
    }
}