using System;

namespace WebAPI.OutputCache.Time
{
    public class CacheTime
    {
        //// cache length in seconds
        public TimeSpan ServerTimespan { get; set; }

        // client cache length in seconds
        public TimeSpan ClientTimeSpan { get; set; }

        public DateTimeOffset AbsoluteExpiration { get; set; }
    }
}