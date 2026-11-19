using System;
using CrispyWaffle.Redis.Telemetry;
using CrispyWaffle.Redis.Utils.Communications;
using CrispyWaffle.Serialization;
using CrispyWaffle.Telemetry;
using NSubstitute;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using Xunit;

namespace CrispyWaffle.Tests.Telemetry;

public sealed class RedisTelemetryClientTests
{
    [Fact]
    public void TrackEventWithConnectorAndSuffixConstructorUsesThirtyDayDefaultTtl()
    {
        var database = Substitute.For<IDatabase>();
        var connection = Substitute.For<IConnectionMultiplexer>();
        connection.GetDatabase(1, null).Returns(database);

        var connectionPoolManager = Substitute.For<IRedisConnectionPoolManager>();
        connectionPoolManager.GetConnection().Returns(connection);

        var serializer = Substitute.For<ISerializer>();
        var redisClient = Substitute.For<IRedisClient>();

        using var connector = new RedisConnector(
            redisClient,
            connectionPoolManager,
            serializer,
            "prefix"
        );

        var telemetryClient = new RedisTelemetryClient(connector, "test-suffix");
        var telemetryEvent = Substitute.For<ITelemetryEvent<SampleTelemetryPayload>>();
        telemetryEvent.Category.Returns(1);
        telemetryEvent.Name.Returns("evt-key");
        telemetryEvent.Event.Returns(new SampleTelemetryPayload { Value = "hello" });

        telemetryClient.TrackEvent(telemetryEvent);

        database
            .Received(1)
            .StringSet(
                (RedisKey)"evt-key",
                Arg.Any<RedisValue>(),
                TimeSpan.FromDays(30),
                When.Always,
                CommandFlags.FireAndForget
            );
    }

    [Serializer]
    public sealed class SampleTelemetryPayload
    {
        public string Value { get; set; }
    }
}
