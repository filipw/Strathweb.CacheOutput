using System;

namespace WebApi.OutputCache.Core.Time
{
    public class ThisYear : IModelQuery<DateTime, CacheTime>
    {
        private readonly int month;
        private readonly int day;
        private readonly int hour;
        private readonly int minute;
        private readonly int second;

        public ThisYear(int month, int day, int hour, int minute, int second)
        {
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.second = second;
        }

        public CacheTime Execute(DateTime model)
        {
            var cacheTime = new CacheTime
                {
                    AbsoluteExpiration = new DateTime(model.Year,
                                                      month,
                                                      day,
                                                      hour,
                                                      minute,
                                                      second),
                };

            if (cacheTime.AbsoluteExpiration <= model)
                cacheTime.AbsoluteExpiration = cacheTime.AbsoluteExpiration.AddYears(1);

            cacheTime.ClientTimeSpan = cacheTime.AbsoluteExpiration.Subtract(model);

            return cacheTime;
        }
    }
}