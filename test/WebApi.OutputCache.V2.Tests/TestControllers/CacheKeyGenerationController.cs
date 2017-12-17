using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace WebApi.OutputCache.V2.Tests.TestControllers
{
    /// <summary>
    /// Controller needed for generating the <see cref="System.Web.Http.Controllers.HttpActionContext" /> needed for testing the <see cref="ICacheKeyGenerator"/> implementations
    /// </summary>
    [RoutePrefix("cacheKeyGeneration")]
    public class CacheKeyGenerationController : ApiController
    {
        private readonly string[] Values = new string[] { "first", "second", "third" };

        [Route("")]
        public IEnumerable<string> Get([FromUri(Name="filter")]string filterExpression)
        {
            return String.IsNullOrWhiteSpace(filterExpression) ? Values : Values.Where(x => x.Contains(filterExpression));
        }

        [Route("{index}")]
        public string GetByIndex(int index)
        {
            return Values[index];
        }
    }
}
