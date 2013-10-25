using System;

namespace WebApi.OutputCache.Core.Time
{
    public class ThisDay : IModelQuery<DateTime, CacheTime>
    {
        private readonly int hour;
        private readonly int minute;
        private readonly int second;

        public ThisDay(int hour, int minute, int second)
        {
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
                                                  model.Day,
                                                  hour,
                                                  minute,
                                                  second),
            };

            if (cacheTime.AbsoluteExpiration <= model)
                cacheTime.AbsoluteExpiration = cacheTime.AbsoluteExpiration.AddDays(1);

            cacheTime.ClientTimeSpan = cacheTime.AbsoluteExpiration.Subtract(model);

            return cacheTime;
        }
    }
}
