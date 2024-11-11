using System;
using System.Threading.Tasks;
using CrispyWaffle.Cache;
using Moq;
using NSubstitute;
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
        CacheManager.AddRepositoryAsync(_mockRepository1.Object, 1);
        CacheManager.AddRepositoryAsync(_mockRepository2.Object, 2);
    }

    [Fact]
    public async Task Set_ShouldStoreValueInAllRepositoriesAsync()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";

        // Act
        await CacheManager.SetAsync(value, key);

        // Assert
        _mockRepository1.Verify(m => m.SetAsync(value, key, (TimeSpan?)null), Times.Once);
        _mockRepository2.Verify(m => m.SetAsync(value, key, (TimeSpan?)null), Times.Once);
    }

    [Fact]
    public async Task Get_ShouldRetrieveValueFromRepositoryAsync()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = "test-value";
        //(bool Exists, string value) test;


        _mockRepository1.Setup(m => m.TryGetAsync<string>(key)).ReturnsAsync((true, "test-value"));

        // Act
        var actualValue = await CacheManager.GetAsync<string>(key);

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public async Task Remove_ShouldRemoveValueFromAllRepositoriesAsync()
    {
        // Arrange
        var key = "test-key";

        // Act
        await CacheManager.RemoveAsync(key);

        // Assert
        await Task.Run(() => _mockRepository1.Verify(m => m.RemoveAsync(key), Times.Once));
        await Task.Run(() => _mockRepository2.Verify(m => m.RemoveAsync(key), Times.Once));
    }
}
