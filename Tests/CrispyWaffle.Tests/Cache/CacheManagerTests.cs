using System;
using CrispyWaffle.Cache;
using CrispyWaffle.Composition;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CrispyWaffle.Tests.Cache;

public class CacheManagerTests
{
    private readonly ICacheRepository _mockCacheRepository;
    private readonly ICacheRepository _mockCacheRepository2;

    public CacheManagerTests()
    {
        _mockCacheRepository = Substitute.For<ICacheRepository>();
        _mockCacheRepository2 = Substitute.For<ICacheRepository>();
    }

    [Fact]
    public void AddRepositoryShouldAddRepositoryWithDefaultPriority()
    {
        // Arrange

        // Act
        var first = CacheManager.AddRepository(_mockCacheRepository);
        var second = CacheManager.AddRepository(_mockCacheRepository2);

        // Assert
        first.Should().BeSameAs(_mockCacheRepository);
        second.Should().BeSameAs(_mockCacheRepository2);
    }

    [Fact]
    public void AddRepositoryShouldAddRepositoryWithSpecifiedPriority()
    {
        // Arrange
        var priority = CacheManager.AddRepository(_mockCacheRepository, 10);

        // Act
        var result = CacheManager.AddRepository(_mockCacheRepository, 10);

        // Assert
        priority.Should().Be(10);
    }

    [Fact]
    public void SetShouldSetValueInAllRepositories()
    {
        // Arrange
        var key = "testKey";
        var value = new { Name = "Test" };
        CacheManager.AddRepository(_mockCacheRepository);

        // Act
        CacheManager.Set(value, key);

        // Assert
        _mockCacheRepository.Received().Set(value, key);
    }

    [Fact]
    public void SetShouldSetValueInRepositoriesWithTTL()
    {
        // Arrange
        var key = "testKey";
        var value = new { Name = "Test" };
        var ttl = TimeSpan.FromMinutes(10);
        CacheManager.AddRepository(_mockCacheRepository);

        // Act
        CacheManager.Set(value, key, ttl);

        // Assert
        _mockCacheRepository.Received().Set(value, key, ttl);
    }

    [Fact]
    public void GetShouldThrowWhenItemNotFound()
    {
        // Arrange
        var key = "testKey";
        _mockCacheRepository.TryGet(key, out Arg.Any<object>()).Returns(false);

        // Act
        Action act = () => CacheManager.Get<dynamic>(key);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Unable to get the item with key testKey");
    }

    [Fact]
    public void SetToShouldThrowWhenRepositoryNotFound()
    {
        // Arrange
        var key = "testKey";
        var value = new { Name = "Test" };
        CacheManager.AddRepository(_mockCacheRepository);

        // Act
        Action act = () => CacheManager.SetTo<ICacheRepository, object>(value, key);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage(
                "The repository of type CrispyWaffle.Cache.ICacheRepository isn't available in the repositories providers list"
            );
    }

    [Fact]
    public void RemoveShouldRemoveKeyFromAllRepositories()
    {
        // Arrange
        var key = "testKey";
        CacheManager.AddRepository(_mockCacheRepository);

        // Act
        CacheManager.Remove(key);

        // Assert
        _mockCacheRepository.Received().Remove(key);
    }

    [Fact]
    public void TTLShouldReturnCorrectTTLFromRepositories()
    {
        // Arrange
        var key = "testKey";
        var expectedTTL = TimeSpan.FromMinutes(10);
        CacheManager.AddRepository(_mockCacheRepository);
        _mockCacheRepository.TTL(key).Returns(expectedTTL);

        // Act
        var result = CacheManager.TTL(key);

        // Assert
        result.Should().Be(expectedTTL);
    }

    [Fact]
    public void RemoveFromShouldThrowWhenRepositoryNotFound()
    {
        // Arrange
        var key = "testKey";
        CacheManager.AddRepository(_mockCacheRepository);

        // Act
        Action act = () => CacheManager.RemoveFrom<ICacheRepository>(key);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage(
                "The repository of type CrispyWaffle.Cache.ICacheRepository isn't available in the repositories providers list"
            );
    }
}
