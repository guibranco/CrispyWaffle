using System;
using System.Threading.Tasks;
using CrispyWaffle.Cache;
using FluentAssertions;
using Xunit;

namespace CrispyWaffle.Tests.Cache;

public class MemoryCacheRepositoryTests
{
    private readonly MemoryCacheRepository _repository;

    public MemoryCacheRepositoryTests()
    {
        _repository = new MemoryCacheRepository();
    }

    [Fact]
    public async Task Set_ShouldStoreValueAsync()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";

        await _repository.SetAsync(value, key);

        // Act
        var actualValue = await _repository.GetAsync<string>(key);

        // Assert
        actualValue.Should().Be(value);
    }

    [Fact]
    public async Task Get_ShouldReturnStoredValueAsync()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = "test-value";
        await _repository.SetAsync(expectedValue, key);

        // Act
        var actualValue = await _repository.GetAsync<string>(key);

        // Assert
        actualValue.Should().Be(expectedValue);
    }

    [Fact]
    public async Task Remove_ShouldRemoveStoredValueAsync()
    {
        // Arrange
        var key = "test-key";
        await _repository.SetAsync("test-value", key);
        await _repository.RemoveAsync(key);

        // Act
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.GetAsync<string>(key)
        );

        // Assert
        exception.Should().NotBeNull();
    }
}
