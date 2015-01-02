using System;
using NUnit.Framework;
using ServiceStack.Redis;

namespace WebApi.OutputCache.Redis.Tests
{
	[TestFixture]
	public class RedisCacheProviderTest
	{
		private IRedisClient _redisClient;
		private RedisCacheProvider _testClass;

		[SetUp]
		public void fixture_init()
		{
			_redisClient = new RedisClient("myredisinstance", 6379, "myredispassword");
			_testClass = new RedisCacheProvider(_redisClient);
		}

		[TearDown]
		public void fixture_dispose()
		{
			if (_redisClient != null)
			{
				var keys = _redisClient.GetAllKeys();
				_redisClient.RemoveAll(keys);

				_redisClient.Dispose();
			}
			
			_testClass = null;
		}

		[Test]
		public void remove_starts_with_method_should_remove_the_correct_keys()
		{
			_redisClient.Add("MyCacheKey1", "something", TimeSpan.FromMinutes(1));
			_redisClient.Add("MyCacheKey2", "something", TimeSpan.FromMinutes(1));
			_redisClient.Add("MyCacheKey3", "something", TimeSpan.FromMinutes(1));
			_redisClient.Add("CacheKey1", "something", TimeSpan.FromMinutes(1));
			_redisClient.Add("CacheKey2", "something", TimeSpan.FromMinutes(1));
			_redisClient.Add("CacheKey3", "something", TimeSpan.FromMinutes(1));

			_testClass.RemoveStartsWith("MyCache");

			var keys = _redisClient.GetAllKeys();

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

			var keys = _redisClient.GetAllItemsFromList("DependencyKey");

			Assert.IsTrue(keys.Count == 2);
		}
	}
}