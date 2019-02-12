namespace WebApi.OutputCache.Core
{
    public sealed class Constants
    {
        public const string ContentTypeKey = ":response-ct";
        public const string EtagKey = ":response-etag";
        public const string GenerationTimestampKey = ":response-generationtimestamp";
        public const string CustomHeaders = ":custom-headers";
        public const string CustomContentHeaders = ":custom-content-headers";
    }
}
