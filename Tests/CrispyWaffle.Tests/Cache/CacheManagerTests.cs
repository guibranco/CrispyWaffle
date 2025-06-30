using System;
using System.Threading;
using System.Threading.Tasks;
using CrispyWaffle.Cache;
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
    public async Task SetAsyncShouldSetValueInAllRepositories()
    {
        // Arrange
        var key = "testKey";
        var value = new { Name = "Test" };
        CacheManager.AddRepository(_mockCacheRepository);      

        // Act
        await CacheManager.SetAsync(value, key);

        // Assert
        await _mockCacheRepository.Received(1).SetAsync(value, key);
    }

    [Fact]
    public async Task SetAsyncShouldSetValueInRepositoriesWithTTL()
    {
        // Arrange
        var key = "testKey";
        var value = new { Name = "Test" };
        var ttl = TimeSpan.FromMinutes(10);
        CacheManager.AddRepository(_mockCacheRepository);

        // Act
        await CacheManager.SetAsync(value, key, ttl);

        // Assert
        await _mockCacheRepository.Received(1).SetAsync(value, key, ttl);
    }

    [Fact]
    public async Task SetAsyncShouldSetValueWithSubKey()
    {
        // Arrange
        var key = "testKey";
        var subKey = "testSubKey";
        var value = new { Name = "Test" };
        CacheManager.AddRepository(_mockCacheRepository);       

        // Act
        await CacheManager.SetAsync(value, key, subKey);

        // Assert
        await _mockCacheRepository.Received(1).SetAsync(value, key, subKey, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsyncShouldThrowWhenItemNotFound()
    {
        // Arrange
        var key = "testKey";
        CacheManager.AddRepository(_mockCacheRepository);
        // Act
        Func<Task> act = async () => await CacheManager.GetAsync<dynamic>(key);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Unable to get the item with key testKey");
    }

    [Fact]
    public async Task SetToShouldThrowWhenRepositoryNotFound()
    {
        // Arrange
        var key = "testKey";
        var value = new { Name = "Test" };
        CacheManager.AddRepository(_mockCacheRepository);

        // Act
        Func<Task> act = async () => await CacheManager.SetToAsync<ICacheRepository, object>(value, key);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "The repository of type CrispyWaffle.Cache.ICacheRepository isn't available in the repositories providers list"
            );
    }   

    [Fact]
    public async Task TTLShouldReturnCorrectTTLFromRepositories()
    {
        // Arrange
        var key = "testKey";
        var expectedTTL = TimeSpan.FromMinutes(10);
        CacheManager.AddRepository(_mockCacheRepository); 
        await CacheManager.SetAsync(new { Name = "Test" }, key, expectedTTL, CancellationToken.None); // Set value with TTL

        // Act
        var result = await CacheManager.TTLAsync(key);

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
        Func<Task> act = async () => await CacheManager.RemoveFrom<ICacheRepository>(key);

        // Assert
        act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "The repository of type CrispyWaffle.Cache.ICacheRepository isn't available in the repositories providers list"
            );
    }
}
