using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WebAPI.OutputCache
{
    public class CacheOutputAttribute : ActionFilterAttribute
    {
        protected ModelQuery<DateTime, CacheTime> CacheTimeQuery;

        private readonly bool anonymousOnly;
        private readonly bool mustRevalidate;

        // cache repository
        private static readonly ObjectCache WebApiCache = MemoryCache.Default;

        readonly Func<HttpActionContext, bool, bool> isCachingTimeValid = (ac, anonymous) =>
        {
            if (anonymous)
                if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
                    return false;

            return ac.Request.Method == HttpMethod.Get;
        };

        private readonly bool excludeQueryStringFromCacheKey;

        public CacheOutputAttribute(int serveTimeSpan = 1,
                                    int clientTimeSpan = 1,
                                    bool anonymousOnly = false,
                                    bool mustRevalidate = false,
                                    bool excludeQueryStringFromCacheKey = false)
        {
            CacheTimeQuery = new ShortTime(serveTimeSpan, clientTimeSpan);

            this.anonymousOnly = anonymousOnly;
            this.mustRevalidate = mustRevalidate;
            this.excludeQueryStringFromCacheKey = excludeQueryStringFromCacheKey;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            if (!isCachingTimeValid(actionContext, anonymousOnly)) return;

            var cachekey = MakeCachekey(actionContext.Request);

            if (!WebApiCache.Contains(cachekey)) return;

            var val = WebApiCache.Get(cachekey) as string;

            if (val == null) return;

            var contenttype = (MediaTypeHeaderValue)WebApiCache.Get(cachekey + ":response-ct") ??
                              new MediaTypeHeaderValue(cachekey.Split(':')[1]);

            actionContext.Response = actionContext.Request.CreateResponse();
            actionContext.Response.Content = new StringContent(val);

            actionContext.Response.Content.Headers.ContentType = contenttype;
            actionContext.Response.Headers.ETag = (EntityTagHeaderValue)WebApiCache.Get(cachekey + ":response-etag");


            var cacheTime = CacheTimeQuery.Execute(DateTime.Now);
            ApplyCacheHeaders(actionContext.Response, cacheTime);
        }

        private string MakeCachekey(HttpRequestMessage actionContext)
        {
            string uri = actionContext.RequestUri.PathAndQuery;

            if (excludeQueryStringFromCacheKey)
                uri = actionContext.RequestUri.AbsolutePath;

            var cachekey = string.Join(":", new[]
            {
                uri,
                Convert.ToString(actionContext.Headers.Accept.FirstOrDefault())
            });
            return cachekey;
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var cacheTime = CacheTimeQuery.Execute(DateTime.Now);

            var cachekey = MakeCachekey(actionExecutedContext.Request);

            if (!(WebApiCache.Contains(cachekey)) && !string.IsNullOrWhiteSpace(cachekey))
            {
                SetEtag(actionExecutedContext);

                var body = actionExecutedContext.Response.Content.ReadAsStringAsync().Result;

                WebApiCache.Add(cachekey, body, cacheTime.AbsoluteExpiration);
                WebApiCache.Add(cachekey + ":response-ct",
                                actionExecutedContext.Response.Content.Headers.ContentType,
                                cacheTime.AbsoluteExpiration);
                WebApiCache.Add(cachekey + ":response-etag",
                              actionExecutedContext.Response.Headers.ETag,
                              cacheTime.AbsoluteExpiration);
            }

            if (!isCachingTimeValid(actionExecutedContext.ActionContext, anonymousOnly)) return;

            ApplyCacheHeaders(actionExecutedContext.ActionContext.Response, cacheTime);
        }

        private void ApplyCacheHeaders(HttpResponseMessage response, CacheTime cacheTime)
        {
            var cachecontrol = new CacheControlHeaderValue
            {
                MaxAge = cacheTime.ClientTimeSpan,
                MustRevalidate = mustRevalidate
            };

            response.Headers.CacheControl = cachecontrol;
        }

        private static void SetEtag(HttpActionExecutedContext actionExecutedContext)
        {
            var eTag = new EntityTagHeaderValue(@"""" + Guid.NewGuid() + @"""");
            actionExecutedContext.Response.Headers.ETag = eTag;
        }

        protected interface ModelQuery<in TModel, out TResult>
        {
            TResult Execute(TModel model);
        }

        protected class CacheTime
        {
            //// cache length in seconds
            public TimeSpan ServerTimespan { get; set; }

            // client cache length in seconds
            public TimeSpan ClientTimeSpan { get; set; }

            public DateTimeOffset AbsoluteExpiration { get; set; }
        }

        protected class ShortTime : ModelQuery<DateTime, CacheTime>
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

        protected class SpecificTime : ModelQuery<DateTime, CacheTime>
        {
            private readonly int year;
            private readonly int month;
            private readonly int day;
            private readonly int hour;
            private readonly int minute;
            private readonly int second;

            public SpecificTime(int year, int month, int day, int hour, int minute, int second)
            {
                this.year = year;
                this.month = month;
                this.day = day;
                this.hour = hour;
                this.minute = minute;
                this.second = second;
            }

            public CacheTime Execute(DateTime model)
            {
                var cacheTime = new CacheTime
                {
                    AbsoluteExpiration = new DateTime(year,
                                                        month,
                                                        day,
                                                        hour,
                                                        minute,
                                                        second),
                };

                cacheTime.ServerTimespan = cacheTime.AbsoluteExpiration.Subtract(model);
                cacheTime.ClientTimeSpan = cacheTime.ServerTimespan;

                return cacheTime;
            }
        }

        protected class ThisYear : ModelQuery<DateTime, CacheTime>
        {
            private readonly int month;
            private readonly int day;
            private readonly int hour;
            private readonly int minute;
            private readonly int second;

            public ThisYear(int month, int day, int hour, int minute, int second)
            {
                this.month = month;
                this.day = day;
                this.hour = hour;
                this.minute = minute;
                this.second = second;
            }

            public CacheTime Execute(DateTime model)
            {
                var cacheTime = new CacheTime
                {
                    AbsoluteExpiration = new DateTime(model.Year,
                                                      month,
                                                      day,
                                                      hour,
                                                      minute,
                                                      second),
                };

                if (cacheTime.AbsoluteExpiration <= model)
                    cacheTime.AbsoluteExpiration = cacheTime.AbsoluteExpiration.AddYears(1);

                cacheTime.ServerTimespan = cacheTime.AbsoluteExpiration.Subtract(model);
                cacheTime.ClientTimeSpan = cacheTime.ServerTimespan;

                return cacheTime;
            }
        }

        protected class ThisMonth : ModelQuery<DateTime, CacheTime>
        {
            private readonly int day;
            private readonly int hour;
            private readonly int minute;
            private readonly int second;

            public ThisMonth(int day, int hour, int minute, int second)
            {
                this.day = day;
                this.hour = hour;
                this.minute = minute;
                this.second = second;
            }

            public CacheTime Execute(DateTime model)
            {
                var cacheTime = new CacheTime
                {
                    AbsoluteExpiration = new DateTime(model.Year,
                                                      model.Month,
                                                      day,
                                                      hour,
                                                      minute,
                                                      second),
                };

                if (cacheTime.AbsoluteExpiration <= model)
                    cacheTime.AbsoluteExpiration = cacheTime.AbsoluteExpiration.AddMonths(1);

                cacheTime.ServerTimespan = cacheTime.AbsoluteExpiration.Subtract(model);
                cacheTime.ClientTimeSpan = cacheTime.ServerTimespan;

                return cacheTime;
            }
        }

        protected class ThisDay : ModelQuery<DateTime, CacheTime>
        {
            private readonly int hour;
            private readonly int minute;
            private readonly int second;

            public ThisDay(int hour, int minute, int second)
            {
                this.hour = hour;
                this.minute = minute;
                this.second = second;
            }

            public CacheTime Execute(DateTime model)
            {
                var cacheTime = new CacheTime
                {
                    AbsoluteExpiration = new DateTime(model.Year,
                                                      model.Month,
                                                      model.Day,
                                                      hour,
                                                      minute,
                                                      second),
                };

                if (cacheTime.AbsoluteExpiration <= model)
                    cacheTime.AbsoluteExpiration = cacheTime.AbsoluteExpiration.AddDays(1);

                cacheTime.ServerTimespan = cacheTime.AbsoluteExpiration.Subtract(model);
                cacheTime.ClientTimeSpan = cacheTime.ServerTimespan;

                return cacheTime;
            }
        }
    }
}