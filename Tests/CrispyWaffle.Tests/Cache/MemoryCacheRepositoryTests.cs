using System;
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
    public void Set_ShouldStoreValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";

        _repository.Set(value, key);

        // Act
        var actualValue = _repository.Get<string>(key);

        // Assert
        actualValue.Should().Be(value);
    }

    [Fact]
    public void Get_ShouldReturnStoredValue()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = "test-value";
        _repository.Set(expectedValue, key);

        // Act
        var actualValue = _repository.Get<string>(key);

        // Assert
        actualValue.Should().Be(expectedValue);
    }

    [Fact]
    public void Remove_ShouldRemoveStoredValue()
    {
        // Arrange
        var key = "test-key";
        _repository.Set("test-value", key);
        _repository.Remove(key);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(
            () => _repository.Get<string>(key)
        );

        // Assert
        exception.Should().NotBeNull();
    }
}
