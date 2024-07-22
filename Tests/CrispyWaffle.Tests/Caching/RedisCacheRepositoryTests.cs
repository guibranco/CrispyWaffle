using System;
using CrispyWaffle.Redis.Cache;
using CrispyWaffle.Redis.Utils.Communications;
using Moq;
using StackExchange.Redis.Extensions.Core.Abstractions;
using Xunit;

namespace CrispyWaffle.Tests.Caching
{
    public class RedisCacheRepositoryTests
    {
        private readonly RedisCacheRepository _repository;
        private readonly Mock<IRedisClient> _mockCacheClient;

        public RedisCacheRepositoryTests()
        {
            var mockConnector = new Mock<RedisConnector>();
            _mockCacheClient = new Mock<IRedisClient>();
            mockConnector.Setup(m => m.Cache).Returns(_mockCacheClient.Object);
            _repository = new RedisCacheRepository(mockConnector.Object);
        }

        [Fact]
        public void SetToDatabase_ShouldStoreValue()
        {
            var key = "test-key";
            var value = "test-value";

            _repository.SetToDatabase(value, key, 0);

            _mockCacheClient.Verify(m => m.Db0.StringSet(key, It.IsAny<byte[]>(), null, When.Always, CommandFlags.None), Times.Once);
        }

        [Fact]
        public void GetFromDatabase_ShouldReturnStoredValue()
        {
            var key = "test-key";
            var expectedValue = "test-value";
            var valueBytes = System.Text.Encoding.UTF8.GetBytes(expectedValue);
            _mockCacheClient.Setup(m => m.Db0.StringGet(key, CommandFlags.PreferReplica)).Returns(valueBytes);

            var actualValue = _repository.GetFromDatabase<string>(key, 0);

            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void RemoveFromDatabase_ShouldRemoveValue()
        {
            var key = "test-key";

            _repository.RemoveFromDatabase(key, 0);

            _mockCacheClient.Verify(m => m.Db0.KeyDelete(key), Times.Once);
        }
    }
}