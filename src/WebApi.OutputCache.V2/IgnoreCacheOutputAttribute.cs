using System;

namespace WebApi.OutputCache.V2
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class IgnoreCacheOutputAttribute : Attribute
    {
    }
}