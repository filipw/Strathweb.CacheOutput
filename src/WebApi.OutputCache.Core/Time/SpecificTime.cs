using System;

namespace WebApi.OutputCache.Core.Time
{
    public class SpecificTime : IModelQuery<DateTime, CacheTime>
    {
        private readonly int year;
        private readonly int month;
        private readonly int day;
        private readonly int hour;
        private readonly int minute;
        private readonly int second;

        public SpecificTime(int year, int month, int day, int hour, int minute, int second)
        {
            this.year = year;
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
                    AbsoluteExpiration = new DateTime(year,
                                                      month,
                                                      day,
                                                      hour,
                                                      minute,
                                                      second),
                };

            cacheTime.ClientTimeSpan = cacheTime.AbsoluteExpiration.Subtract(model);

            return cacheTime;
        }
    }
}