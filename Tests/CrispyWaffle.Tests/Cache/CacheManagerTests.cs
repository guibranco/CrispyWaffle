using System;
using System.Linq;
using CrispyWaffle.Cache;
using Moq;
using Xunit;

namespace CrispyWaffle.Tests.Cache
{
    public class CacheManagerTests
    {
        private readonly Mock<ICacheRepository> _mockRepository1;
        private readonly Mock<ICacheRepository> _mockRepository2;

        public CacheManagerTests()
        {
            _mockRepository1 = new Mock<ICacheRepository>();
            _mockRepository2 = new Mock<ICacheRepository>();
            CacheManager.AddRepository(_mockRepository1.Object, 1);
            CacheManager.AddRepository(_mockRepository2.Object, 2);
        }

        [Fact]
        public void Set_ShouldStoreValueInAllRepositories()
        {
            var key = "test-key";
            var value = "test-value";

            CacheManager.Set(value, key);

            _mockRepository1.Verify(m => m.Set(value, key, null), Times.Once);
            _mockRepository2.Verify(m => m.Set(value, key, null), Times.Once);
        }

        [Fact]
        public void Get_ShouldRetrieveValueFromRepository()
        {
            var key = "test-key";
            var expectedValue = "test-value";
            _mockRepository1.Setup(m => m.TryGet(key, out expectedValue)).Returns(true);

            var actualValue = CacheManager.Get<string>(key);

            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void Remove_ShouldRemoveValueFromAllRepositories()
        {
            var key = "test-key";

            CacheManager.Remove(key);

            _mockRepository1.Verify(m => m.Remove(key), Times.Once);
            _mockRepository2.Verify(m => m.Remove(key), Times.Once);
        }
    }
}
