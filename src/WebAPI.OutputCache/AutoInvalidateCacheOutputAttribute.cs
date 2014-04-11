using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebApi.OutputCache.Core;

namespace WebAPI.OutputCache
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class AutoInvalidateCacheOutputAttribute : BaseCacheAttribute
    {
        public bool TryMatchType { get; set; }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response != null && !actionExecutedContext.Response.IsSuccessStatusCode) return;
            if (actionExecutedContext.ActionContext.Request.Method != HttpMethod.Post &&
                actionExecutedContext.ActionContext.Request.Method != HttpMethod.Put &&
                actionExecutedContext.ActionContext.Request.Method != HttpMethod.Delete &&
                actionExecutedContext.ActionContext.Request.Method.Method.ToLower() != "patch") return;

            var controller = actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor;
            var actions = FindAllGetMethods(controller.ControllerType, TryMatchType ? actionExecutedContext.ActionContext.ActionDescriptor.GetParameters() : null);

            var config = actionExecutedContext.ActionContext.Request.GetConfiguration();
            EnsureCache(config, actionExecutedContext.ActionContext.Request);

            foreach (var action in actions)
            {
                var key = config.CacheOutputConfiguration().MakeBaseCachekey(controller.ControllerName, action);
                if (WebApiCache.Contains(key))
                {
                    WebApiCache.RemoveStartsWith(key);
                }
            }
        }

        private static IEnumerable<string> FindAllGetMethods(Type controllerType, IEnumerable<HttpParameterDescriptor> httpParameterDescriptors)
        {
            var actions = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            var filteredActions = actions.Where(x =>
                {
                    if (x.Name.ToLower().StartsWith("get")) return true;
                    if (x.GetCustomAttributes(typeof(HttpGetAttribute), true).Any()) return true;

                    return false;
                });

            if (httpParameterDescriptors != null)
            {
                var allowedTypes = httpParameterDescriptors.Select(x => x.ParameterType).ToList();
                var filteredByType = filteredActions.ToList().Where(x =>
                    {
                        if (allowedTypes.Any(s => s == x.ReturnType)) return true;
                        if (allowedTypes.Any(s => typeof(IEnumerable).IsAssignableFrom(x.ReturnType) && x.ReturnType.GetGenericArguments().Any() && x.ReturnType.GetGenericArguments()[0] == s)) return true;
                        if (allowedTypes.Any(s => typeof(IEnumerable).IsAssignableFrom(x.ReturnType) && x.ReturnType.GetElementType() == s)) return true;
                        return false;
                    });

                filteredActions = filteredByType;
            }

            var projectedActions = filteredActions.Select(x =>
                {
                    var overridenNames = x.GetCustomAttributes(typeof(ActionNameAttribute), false);
                    if (overridenNames.Any())
                    {
                        var first = (ActionNameAttribute)overridenNames.FirstOrDefault();
                        if (first != null) return first.Name;
                    }
                    return x.Name;
                });

            return projectedActions;
        }
    }
}