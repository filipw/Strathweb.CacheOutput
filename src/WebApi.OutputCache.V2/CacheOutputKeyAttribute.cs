using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace WebApi.OutputCache.V2
{
	/// <summary>
	/// Enables custom cache key generation on a type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class CacheKeyComponentAttribute : Attribute
	{
		public Type GeneratorType { get; private set; }


		public CacheKeyComponentAttribute(Type generatorType)
		{
			GeneratorType = generatorType;
		}
	}




	public interface ICacheKeyComponentGenerator
	{
		string Generate(object value);
	}





	/// <remarks>No recursive generation.</remarks>
	public abstract class ComplexValueCacheKeyComponentGenerator : ICacheKeyComponentGenerator
	{
		public string Generate(object value)
		{
			return string.Format("{0}{{{1}}}", value.GetType().FullName,
				string.Join(";", GetNamedValues(value).Select(a => a.Key + "=" + a.Value)));
		}

		/// <summary>
		/// Usually will return a dictionary of property name/value.
		/// </summary>
		protected abstract IDictionary<string, object> GetNamedValues(object complexValue);
	}




	internal class EnumerableCacheKeyComponentGenerator : ICacheKeyComponentGenerator
	{
		public string Generate(object value)
		{
			return string.Join(";", (IEnumerable)value);
		}
	}


	internal class AnyCacheKeyComponentGenerator : ICacheKeyComponentGenerator
	{
		public string Generate(object value)
		{
			return value.ToString();
		}
	}

}