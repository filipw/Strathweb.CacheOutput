using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.OutputCache.V2.TimeAttributes;

namespace WebApi.OutputCache.V2.Demo
{
    public class TeamsController : ApiController
    {
        private static readonly List<Team> Teams = new List<Team>
            {
                new Team {Id = 1, League = "NHL", Name = "Leafs"},
                new Team {Id = 2, League = "NHL", Name = "Habs"},
            };

        [CacheOutput(ClientTimeSpan = 50, ServerTimeSpan = 50)]
        public IEnumerable<Team> Get()
        {
            return Teams;
        }

        [CacheOutputUntil(2016, 7, 20)]
        public Team GetById(int id)
        {
            var team = Teams.FirstOrDefault(i => i.Id == id);
            if (team == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            return team;
        }

        [InvalidateCacheOutput("Get")]
        public void Post(Team value)
        {
            if (!ModelState.IsValid) throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            Teams.Add(value);
        }

        public void Put(int id, Team value)
        {
            if (!ModelState.IsValid) throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));

            var team = Teams.FirstOrDefault(i => i.Id == id);
            if (team == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            team.League = value.League;
            team.Name = value.Name;

            var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);
            cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((TeamsController t) => t.GetById(0)));
        }

        public void Delete(int id)
        {
            var team = Teams.FirstOrDefault(i => i.Id == id);
            if (team == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            Teams.Remove(team);
        }
    }
}
