using System.Web.Http.Controllers;

namespace WebApi.OutputCache.V2
{
    public interface IKeyPrefixGenerator
    {
        string Generate(HttpActionContext actionContext, string baseCacheKey);
    }
}