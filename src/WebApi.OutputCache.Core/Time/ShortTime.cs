using System;

namespace WebApi.OutputCache.Core.Time
{
    public class ShortTime : IModelQuery<DateTime, CacheTime>
    {
        private readonly int serverTimeInSeconds;
        private readonly int clientTimeInSeconds;
        private readonly int? sharedTimeInSecounds;

        public ShortTime(int serverTimeInSeconds, int clientTimeInSeconds, int? sharedTimeInSecounds)
        {
            if (serverTimeInSeconds < 0)
                serverTimeInSeconds = 0;

            this.serverTimeInSeconds = serverTimeInSeconds;

            if (clientTimeInSeconds < 0)
                clientTimeInSeconds = 0;

            this.clientTimeInSeconds = clientTimeInSeconds;

            if (sharedTimeInSecounds.HasValue && sharedTimeInSecounds.Value < 0)
                sharedTimeInSecounds = 0;

            this.sharedTimeInSecounds = sharedTimeInSecounds;
        }

        public CacheTime Execute(DateTime model)
        {
            var cacheTime = new CacheTime
                {
                    AbsoluteExpiration = model.AddSeconds(serverTimeInSeconds),
                    ClientTimeSpan = TimeSpan.FromSeconds(clientTimeInSeconds),
                    SharedTimeSpan = sharedTimeInSecounds.HasValue ? (TimeSpan?) TimeSpan.FromSeconds(sharedTimeInSecounds.Value) : null
                };

            return cacheTime;
        }
    }
}