using System.Text;
using CrispyWaffle.Redis.Cache;
using CrispyWaffle.Redis.Utils.Communications;
using FluentAssertions;
using NSubstitute;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using Xunit;

namespace CrispyWaffle.Tests.Cache;

public class RedisCacheRepositoryTests
{
    private readonly IDatabase _database;
    private readonly RedisConnector _connector;
    private readonly RedisCacheRepository _repository;

    public RedisCacheRepositoryTests()
    {
        const string key = "test-key";
        const string value = "test-value";

        _database = Substitute.For<IDatabase>();
        _database.StringGet(key, CommandFlags.PreferReplica).Returns(new RedisValue(value));
        var connectionPoolManager = Substitute.For<IRedisConnectionPoolManager>();
        connectionPoolManager.GetConnection().GetDatabase(0).Returns(_database);
        var serializer = Substitute.For<ISerializer>();
        serializer.Deserialize<string>(Arg.Any<byte[]>()).Returns(value);
        serializer.Serialize(Arg.Is(value)).Returns(Encoding.UTF8.GetBytes(value));
        var client = Substitute.For<IRedisClient>();
        _connector = Substitute.For<RedisConnector>(
            client,
            connectionPoolManager,
            serializer,
            "prefix"
        );
        _repository = new RedisCacheRepository(_connector);
    }

    [Fact]
    public void SetToDatabaseShouldStoreValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";

        // Act
        _repository.SetToDatabase(value, key, 0);

        // Assert
        _connector.Received(1).GetDatabase(0);
        _database
            .Received(1)
            .StringSet(key, Arg.Any<RedisValue>(), null, When.Always, CommandFlags.None);
    }

    [Fact]
    public void GetFromDatabaseShouldReturnStoredValue()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = "test-value";

        // Act
        var actualValue = _repository.GetFromDatabase<string>(key, 0);

        // Assert
        actualValue.Should().Be(expectedValue);
    }

    [Fact]
    public void RemoveFromDatabaseShouldRemoveValue()
    {
        // Arrange
        var key = "test-key";

        // Act
        _repository.RemoveFromDatabase(key, 0);

        // Assert
        _connector.Received(1).GetDatabase(0).KeyDelete(key, CommandFlags.FireAndForget);
    }
}
