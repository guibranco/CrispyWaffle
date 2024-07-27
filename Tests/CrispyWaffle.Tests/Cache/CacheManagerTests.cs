using System;
using CrispyWaffle.Cache;
using Moq;
using Xunit;

namespace CrispyWaffle.Tests.Cache;

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
        // Arrange
        var key = "test-key";
        var value = "test-value";

        // Act
        CacheManager.Set(value, key);

        // Assert
        _mockRepository1.Verify(m => m.Set(value, key, (TimeSpan?)null), Times.Once);
        _mockRepository2.Verify(m => m.Set(value, key, (TimeSpan?)null), Times.Once);
    }

    [Fact]
    public void Get_ShouldRetrieveValueFromRepository()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = "test-value";
        _mockRepository1.Setup(m => m.TryGet(key, out expectedValue)).Returns(true);

        // Act
        var actualValue = CacheManager.Get<string>(key);

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void Remove_ShouldRemoveValueFromAllRepositories()
    {
        // Arrange
        var key = "test-key";

        // Act
        CacheManager.Remove(key);

        // Assert
        _mockRepository1.Verify(m => m.Remove(key), Times.Once);
        _mockRepository2.Verify(m => m.Remove(key), Times.Once);
    }
}
