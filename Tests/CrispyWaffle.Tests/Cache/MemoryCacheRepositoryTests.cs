using System;
using System.Threading.Tasks;
using CrispyWaffle.Cache;
using FluentAssertions;
using Xunit;

namespace CrispyWaffle.Tests.Cache;

public class MemoryCacheRepositoryTests
{
    private readonly MemoryCacheRepository _repository;

    public MemoryCacheRepositoryTests() => _repository = new MemoryCacheRepository();

    [Fact]
    public async Task SetAsyncShouldStoreValue()
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
    public async Task GetShouldReturnStoredValue()
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
    public async Task RemoveShouldRemoveStoredValue()
    {
        // Arrange
        var key = "test-key";
        var Task1 = _repository.SetAsync("test-value", key).AsTask();
        var Task2 = _repository.RemoveAsync(key);

        await Task.WhenAll(Task1, Task2);
        // Act
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _repository.GetAsync<string>(key)
        );

        // Assert
        exception.Should().NotBeNull();
    }
}
