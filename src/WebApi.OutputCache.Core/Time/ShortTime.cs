using System;

namespace WebApi.OutputCache.Core.Time
{
    public class ShortTime : IModelQuery<DateTime, CacheTime>
    {
        private readonly int serverTimeInSeconds;
        private readonly int clientTimeInSeconds;
        private readonly int? sharedTimeInSecounds;
        private readonly bool slideExpiration;

        public ShortTime(int serverTimeInSeconds, int clientTimeInSeconds, int? sharedTimeInSeconds, bool slideExpiration)
        {
            if (serverTimeInSeconds < 0)
                serverTimeInSeconds = 0;

            this.serverTimeInSeconds = serverTimeInSeconds;

            if (clientTimeInSeconds < 0)
                clientTimeInSeconds = 0;

            this.clientTimeInSeconds = clientTimeInSeconds;

            if (sharedTimeInSeconds.HasValue && sharedTimeInSeconds.Value < 0)
                sharedTimeInSeconds = 0;

            this.sharedTimeInSecounds = sharedTimeInSeconds;

            this.slideExpiration = slideExpiration;
        }

        public CacheTime Execute(DateTime model)
        {
            var cacheTime = new CacheTime
                {
                    AbsoluteExpiration = model.AddSeconds(serverTimeInSeconds),
                SlidingExpiration = TimeSpan.FromSeconds(serverTimeInSeconds),
                    ClientTimeSpan = TimeSpan.FromSeconds(clientTimeInSeconds),
                    SharedTimeSpan = sharedTimeInSecounds.HasValue ? (TimeSpan?) TimeSpan.FromSeconds(sharedTimeInSecounds.Value) : null
                };

            return cacheTime;
        }
    }
}