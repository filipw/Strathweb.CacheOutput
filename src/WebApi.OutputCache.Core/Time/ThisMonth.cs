using System;

namespace WebApi.OutputCache.Core.Time
{
    public class ThisMonth : IModelQuery<DateTime, CacheTime>
    {
        private readonly int day;
        private readonly int hour;
        private readonly int minute;
        private readonly int second;

        public ThisMonth(int day, int hour, int minute, int second)
        {
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
                                                      model.Month,
                                                      day,
                                                      hour,
                                                      minute,
                                                      second),
                };

            if (cacheTime.AbsoluteExpiration <= model)
                cacheTime.AbsoluteExpiration = cacheTime.AbsoluteExpiration.AddMonths(1);

            cacheTime.ClientTimeSpan = cacheTime.AbsoluteExpiration.Subtract(model);

            return cacheTime;
        }
    }
}