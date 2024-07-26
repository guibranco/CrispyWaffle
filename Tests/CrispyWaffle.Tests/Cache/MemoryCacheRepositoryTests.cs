using System;
using CrispyWaffle.Cache;
using Xunit;

namespace CrispyWaffle.Tests.Cache
{
    public class MemoryCacheRepositoryTests
    {
        private readonly MemoryCacheRepository _repository;

        public MemoryCacheRepositoryTests()
        {
            _repository = new MemoryCacheRepository();
        }

        [Fact]
        public void Set_ShouldStoreValue()
        {
            var key = "test-key";
            var value = "test-value";

            _repository.Set(value, key);

            var actualValue = _repository.Get<string>(key);
            Assert.Equal(value, actualValue);
        }

        [Fact]
        public void Get_ShouldReturnStoredValue()
        {
            var key = "test-key";
            var expectedValue = "test-value";
            _repository.Set(expectedValue, key);

            var actualValue = _repository.Get<string>(key);
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void Remove_ShouldRemoveStoredValue()
        {
            var key = "test-key";
            _repository.Set("test-value", key);

            _repository.Remove(key);

            Assert.Throws<InvalidOperationException>(() => _repository.Get<string>(key));
        }
    }
}
