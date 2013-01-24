using System;

namespace WebAPI.OutputCache.Time
{
    internal class ShortTime : IModelQuery<DateTime, CacheTime>
    {
        private readonly int serverTimeInSeconds;
        private readonly int clientTimeInSeconds;

        public ShortTime(int serverTimeInSeconds, int clientTimeInSeconds)
        {
            if (serverTimeInSeconds < 1)
                serverTimeInSeconds = 1;

            this.serverTimeInSeconds = serverTimeInSeconds;

            if (clientTimeInSeconds < 1)
                clientTimeInSeconds = 1;

            this.clientTimeInSeconds = clientTimeInSeconds;
        }

        public CacheTime Execute(DateTime model)
        {
            var cacheTime = new CacheTime
                {
                    AbsoluteExpiration = model.AddSeconds(clientTimeInSeconds),
                    ServerTimespan = TimeSpan.FromSeconds(serverTimeInSeconds),
                    ClientTimeSpan = TimeSpan.FromSeconds(clientTimeInSeconds)
                };

            return cacheTime;
        }
    }
}