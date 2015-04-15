using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using StackExchange.Redis;
using WebApi.OutputCache.Redis;

namespace WebApi.OutputCache.Stackexchange.Redis.Tests
{
	[TestFixture]
	public class RedisCacheProviderTest
	{
		private IDatabase _database;
		private RedisCacheProvider _testClass;
		private ConnectionMultiplexer _connectionMultiplexer;

		[SetUp]
		public void fixture_init()
		{
			_connectionMultiplexer = ConnectionMultiplexer.Connect("contoso5.redis.cache.windows.net,ssl=true,password=...");

			_testClass = new RedisCacheProvider(_connectionMultiplexer);
			_database = _connectionMultiplexer.GetDatabase();
		}

		[TearDown]
		public void fixture_dispose()
		{
			var keys = new HashSet<RedisKey>();

			var endPoints = _connectionMultiplexer.GetEndPoints();

			foreach (EndPoint endpoint in endPoints)
			{
				var dbKeys = _connectionMultiplexer.GetServer(endpoint).Keys();

				foreach (var dbKey in dbKeys)
				{
					if (!keys.Contains(dbKey))
					{
						keys.Add(dbKey);
					}
				}
			}

			foreach (RedisKey key in keys)
			{
				_database.KeyDelete(key);
			}

			_testClass = null;
		}

		[Test]
		public void remove_starts_with_method_should_remove_the_correct_keys()
		{
			_database.StringSet("MyCacheKey1", "something", TimeSpan.FromMinutes(1));
			_database.StringSet("MyCacheKey2", "something", TimeSpan.FromMinutes(1));
			_database.StringSet("MyCacheKey3", "something", TimeSpan.FromMinutes(1));
			_database.StringSet("CacheKey1", "something", TimeSpan.FromMinutes(1));
			_database.StringSet("CacheKey2", "something", TimeSpan.FromMinutes(1));
			_database.StringSet("CacheKey3", "something", TimeSpan.FromMinutes(1));

			_testClass.RemoveStartsWith("MyCache");

			var keys = _connectionMultiplexer.GetAllKeys().Select(x => x.ToString()).ToList();

			Assert.IsFalse(keys.Contains("MyCacheKey1"));
			Assert.IsFalse(keys.Contains("MyCacheKey2"));
			Assert.IsFalse(keys.Contains("MyCacheKey3"));
			Assert.IsTrue(keys.Count == 3);
		}

		[Test]
		public void add_method_with_dependency_should_add_a_list_into_redis()
		{
			_testClass.Add("MyCacheKey1", "something", DateTimeOffset.Now.AddMinutes(2), "DependencyKey");
			_testClass.Add("MyCacheKey2", "something", DateTimeOffset.Now.AddMinutes(2), "DependencyKey");

			var keys = _database.SetMembers("DependencyKey").Select(x => x.ToString()).ToList();

			Assert.IsTrue(keys.Count == 2);
		}

		[Test]
		public void add_complex_object_should_serialize_and_store_the_class()
		{
			var obj = new ComplexObject()
			{
				Firstname = "Ugo",
				Lastname = "Lattanzi",
				BirthDate = new DateTime(1978, 03, 29),
				Site = new Uri("http://tostring.it"),
				TwitterUsername = "@imperugo"
			};

			_testClass.Add("mycacheKey", obj, DateTimeOffset.Now.AddMinutes(2));

			RedisValue serializedObject = _database.StringGet("mycacheKey");

			Assert.IsNotNull(serializedObject);

			var binaryFormatter = new BinaryFormatter();
			using (var memoryStream = new MemoryStream(serializedObject))
			{
				ComplexObject result = binaryFormatter.Deserialize(memoryStream) as ComplexObject;

				Assert.IsNotNull(result);
				Assert.AreEqual(obj.Firstname,result.Firstname);
				Assert.AreEqual(obj.Lastname, result.Lastname);
				Assert.AreEqual(obj.BirthDate, result.BirthDate);
				Assert.AreEqual(obj.Site, result.Site);
				Assert.AreEqual(obj.TwitterUsername, result.TwitterUsername);
			}
		}
	}

	[Serializable]
	public class ComplexObject
	{
		public string Firstname { get; set; }
		public string Lastname { get; set; }
		public string TwitterUsername { get; set; }
		public Uri Site { get; set; }
		public DateTime BirthDate { get; set; }
	}
}