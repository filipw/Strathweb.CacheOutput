using System;
using System.Collections.Generic;
using System.Net.Http;

namespace WebApi.OutputCache.V2
{
	/// <summary>
	/// When this attribute is used, only the specified vebrs will participate in cache.
	/// Otherwise GET method only.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public class CacheOutputHttpVerbsAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets the list of HTTP verbs that needs to participate in cache.
		/// </summary>
		public string[] Verbs { get; private set; }


		public CacheOutputHttpVerbsAttribute(params string[] verbs)
		{
			Verbs = verbs;
		}
	}



	/// <summary>
	/// A shorthand version to CacheOutputHttpVerbsAttribute.
	/// </summary>
	/// <see cref="CacheOutputHttpVerbsAttribute"/>
	public sealed class CacheOutputHttpGetAttribute : CacheOutputHttpVerbsAttribute
	{
		public CacheOutputHttpGetAttribute()
			: base("GET")
		{
		}
	}



	/// <summary>
	/// A shorthand version to CacheOutputHttpVerbsAttribute.
	/// </summary>
	/// <see cref="CacheOutputHttpVerbsAttribute"/>
	public sealed class CacheOutputHttpPostAttribute : CacheOutputHttpVerbsAttribute
	{
		public CacheOutputHttpPostAttribute()
			: base("Post")
		{
		}
	}


}