using System;

namespace WebApi.OutputCache.Core.Time
{
    public class ShortTime : IModelQuery<DateTime, CacheTime>
    {
        private readonly int serverTimeInSeconds;
        private readonly int clientTimeInSeconds;

        public ShortTime(int serverTimeInSeconds, int clientTimeInSeconds)
        {
            if (serverTimeInSeconds < 0)
                serverTimeInSeconds = 0;

            this.serverTimeInSeconds = serverTimeInSeconds;

            if (clientTimeInSeconds < 0)
                clientTimeInSeconds = 0;

            this.clientTimeInSeconds = clientTimeInSeconds;
        }

        public CacheTime Execute(DateTime model)
        {
            var cacheTime = new CacheTime
                {
                    AbsoluteExpiration = model.AddSeconds(serverTimeInSeconds),
                    ClientTimeSpan = TimeSpan.FromSeconds(clientTimeInSeconds)
                };

            return cacheTime;
        }
    }
}