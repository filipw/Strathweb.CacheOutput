using System.Collections.Generic;

namespace WebApi.OutputCache.V2
{
    public interface IEndpointGenerator
    {
        string Generate(Dictionary<string, object> arguments);
    }
}