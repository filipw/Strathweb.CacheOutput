using System;

namespace WebApi.OutputCache.Core.Time
{
    public class PreciseTime : IModelQuery<DateTime, CacheTime>
    {
        private readonly TimeSpan _server;
        private readonly TimeSpan _client;
        private readonly TimeSpan? _shared;

        public PreciseTime(TimeSpan server, TimeSpan client, TimeSpan? shared = null)
        {
            _server = server;
            _client = client;
            _shared = shared;
        }

        public CacheTime Execute(DateTime model)
        {
            return  new CacheTime
            {
                AbsoluteExpiration = model.Add(_server),
                ClientTimeSpan = _client,
                SharedTimeSpan = _shared
            };
        }
    }
}