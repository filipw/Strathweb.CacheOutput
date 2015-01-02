using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using StackExchange.Redis;

namespace StackExchange.Redis
{
	public static class StackExchangeRedisExtensions
	{
		public static T Get<T>(this IDatabase cache, string key)
		{
			return Deserialize<T>(cache.StringGet(key));
		}

		public static object Get(this IDatabase cache, string key)
		{
			return Deserialize<object>(cache.StringGet(key));
		}

		public static IEnumerable<RedisKey> GetAllKeys(this ConnectionMultiplexer connectionMultiplexer)
		{
			var keys = new HashSet<RedisKey>();

			//Could have more than one instance https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/KeysScan.md

			var endPoints = connectionMultiplexer.GetEndPoints();

			foreach (EndPoint endpoint in endPoints)
			{
				var dbKeys = connectionMultiplexer.GetServer(endpoint).Keys();

				foreach (var dbKey in dbKeys)
				{
					if (!keys.Contains(dbKey))
					{
						keys.Add(dbKey);
					}
				}
			}

			return keys;
		}

		public static IEnumerable<RedisKey> SearchKeys(this ConnectionMultiplexer connectionMultiplexer, string searchPattern)
		{
			var keys = new HashSet<RedisKey>();

			//Could have more than one instance https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/KeysScan.md

			var endPoints = connectionMultiplexer.GetEndPoints();

			foreach (EndPoint endpoint in endPoints)
			{
				var dbKeys = connectionMultiplexer.GetServer(endpoint).Keys(pattern: searchPattern);

				foreach (var dbKey in dbKeys)
				{
					if (!keys.Contains(dbKey))
					{
						keys.Add(dbKey);
					}
				}
			}

			return keys;
		}

		public static bool Set(this IDatabase cache, string key, object value, TimeSpan expiry)
		{
			return cache.StringSet(key, Serialize(value), expiry);
		}

		private static byte[] Serialize(object o)
		{
			if (o == null)
			{
				return null;
			}

			var binaryFormatter = new BinaryFormatter();
			using (var memoryStream = new MemoryStream())
			{
				binaryFormatter.Serialize(memoryStream, o);
				var objectDataAsStream = memoryStream.ToArray();
				return objectDataAsStream;
			}
		}

		private static T Deserialize<T>(byte[] stream)
		{
			if (stream == null)
			{
				return default(T);
			}

			var binaryFormatter = new BinaryFormatter();
			using (var memoryStream = new MemoryStream(stream))
			{
				var result = (T) binaryFormatter.Deserialize(memoryStream);
				return result;
			}
		}
	}
}