using System;
using CrispyWaffle.Composition;
using CrispyWaffle.Extensions;
using CrispyWaffle.Redis.Utils.Communications;
using CrispyWaffle.Serialization;
using CrispyWaffle.Telemetry;
using StackExchange.Redis;

namespace CrispyWaffle.Redis.Telemetry;

/// <summary>
/// A Redis-based implementation of the <see cref="ITelemetryClient"/> interface for tracking hits, events, metrics, exceptions, and dependencies in Redis.
/// </summary>
/// <seealso cref="ITelemetryClient"/>
public sealed class RedisTelemetryClient : ITelemetryClient
{
    /// <summary>
    /// The Redis connector used to interact with Redis.
    /// </summary>
    private readonly RedisConnector _redis;

    /// <summary>
    /// The default Time-to-Live (TTL) for events.
    /// </summary>
    private readonly TimeSpan _defaultTTL;

    /// <summary>
    /// A suffix to be appended to keys for events and metrics.
    /// </summary>
    private readonly string _suffix;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisTelemetryClient"/> class using the default Redis connector and TTL.
    /// </summary>
    public RedisTelemetryClient()
    {
        _redis = ServiceLocator.Resolve<RedisConnector>();
        _defaultTTL = new TimeSpan(30, 0, 0, 0); // Default TTL is 30 days
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisTelemetryClient"/> class using the specified Redis connector and key suffix.
    /// </summary>
    /// <param name="redis">The <see cref="RedisConnector"/> instance used to interact with Redis.</param>
    /// <param name="suffix">The key suffix to be appended to event and metric keys.</param>
    public RedisTelemetryClient(RedisConnector redis, string suffix)
    {
        _redis = redis;
        _suffix = suffix;
    }

    /// <summary>
    /// Gets or sets the database used for tracking hits.
    /// </summary>
    /// <value>The database number for hits tracking.</value>
    public int HitDatabase { get; set; }

    /// <summary>
    /// Gets or sets the database used for tracking metrics.
    /// </summary>
    /// <value>The database number for metrics tracking.</value>
    public int MetricDatabase { get; set; }

    /// <summary>
    /// Retrieves the current hit count for a given hit name from Redis.
    /// </summary>
    /// <param name="hitName">The name of the hit to retrieve the count for.</param>
    /// <returns>The current count of hits for the specified <paramref name="hitName"/>.</returns>
    public int GetHit(string hitName)
    {
        var result = _redis.GetDatabase(HitDatabase).StringGet(hitName, CommandFlags.PreferReplica);
        return result.HasValue ? result.ToString().ToInt32() : 0;
    }

    /// <summary>
    /// Increments the hit count for a given hit name in Redis.
    /// </summary>
    /// <param name="hitName">The name of the hit to track.</param>
    public void TrackHit(string hitName) =>
        _redis.GetDatabase(HitDatabase).StringIncrement(hitName, 1, CommandFlags.FireAndForget);

    /// <summary>
    /// Removes the hit count for a given hit name from Redis.
    /// </summary>
    /// <param name="hitName">The name of the hit to remove.</param>
    /// <returns><c>true</c> if the hit was successfully removed, otherwise <c>false</c>.</returns>
    public bool RemoveHit(string hitName) => _redis.GetDatabase(HitDatabase).KeyDelete(hitName);

    /// <summary>
    /// Retrieves the event data for the specified event from Redis.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to retrieve.</typeparam>
    /// <param name="event">The event to retrieve data for.</param>
    /// <returns>The event data if found, or <c>null</c> if the event does not exist.</returns>
    public TEvent GetEvent<TEvent>(ITelemetryEvent<TEvent> @event)
        where TEvent : class, new()
    {
        var valueBytes = _redis
            .GetDatabase(@event.Category)
            .StringGet(@event.Name, CommandFlags.PreferReplica);
        return !valueBytes.HasValue ? null : _redis.Serializer.Deserialize<TEvent>(valueBytes);
    }

    /// <summary>
    /// Tracks and stores the event data in Redis with the default TTL.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to track.</typeparam>
    /// <param name="event">The event data to track.</param>
    public void TrackEvent<TEvent>(ITelemetryEvent<TEvent> @event)
        where TEvent : class, new()
    {
        _redis
            .GetDatabase(@event.Category)
            .StringSet(
                @event.Name,
                (string)@event.Event.GetSerializer(),
                _defaultTTL,
                When.Always,
                CommandFlags.FireAndForget
            );
    }

    /// <summary>
    /// Tracks and stores the event data in Redis with the specified TTL.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to track.</typeparam>
    /// <param name="event">The event data to track.</param>
    /// <param name="ttl">The TTL (Time-to-Live) for the event.</param>
    public void TrackEvent<TEvent>(ITelemetryEvent<TEvent> @event, TimeSpan ttl)
        where TEvent : class, new()
    {
        _redis
            .GetDatabase(@event.Category)
            .StringSet(
                @event.Name,
                (string)@event.Event.GetSerializer(),
                ttl,
                When.Always,
                CommandFlags.FireAndForget
            );
    }

    /// <summary>
    /// Retrieves the value of a metric from Redis based on its name and variation.
    /// </summary>
    /// <param name="metricName">The name of the metric.</param>
    /// <param name="variation">The variation of the metric.</param>
    /// <returns>The current value of the specified metric, or 0 if not found.</returns>
    public int GetMetric(string metricName, string variation)
    {
        var value = _redis
            .GetDatabase(MetricDatabase)
            .HashGet(metricName, variation, CommandFlags.PreferReplica);
        return value.HasValue ? value.ToString().ToInt32() : 0;
    }

    /// <summary>
    /// Increments the value of a metric in Redis based on its name and variation.
    /// </summary>
    /// <param name="metricName">The name of the metric.</param>
    /// <param name="variation">The variation of the metric.</param>
    public void TrackMetric(string metricName, string variation)
    {
        _redis
            .GetDatabase(MetricDatabase)
            .HashIncrement(metricName, variation, 1, CommandFlags.FireAndForget);
    }

    /// <summary>
    /// Removes the specified metric variation from Redis.
    /// </summary>
    /// <param name="metricName">The name of the metric.</param>
    /// <param name="variation">The variation of the metric to remove.</param>
    /// <returns><c>true</c> if the metric variation was successfully removed, otherwise <c>false</c>.</returns>
    public bool RemoveMetric(string metricName, string variation) =>
        _redis.GetDatabase(MetricDatabase).HashDelete(metricName, variation);

    /// <summary>
    /// Tracks an exception in Redis by incrementing the count of the specified exception type.
    /// </summary>
    /// <param name="exceptionType">The type of the exception to track.</param>
    public void TrackException(Type exceptionType)
    {
        _redis
            .GetDatabase(MetricDatabase)
            .HashIncrement(
                $"Exceptions_{_suffix}",
                exceptionType.FullName,
                1,
                CommandFlags.FireAndForget
            );
    }

    /// <summary>
    /// Tracks a dependency in Redis by incrementing the count of resolved times for the specified dependency.
    /// </summary>
    /// <param name="interfaceType">The type of the dependency to track.</param>
    /// <param name="resolvedTimes">The number of times the dependency was resolved.</param>
    public void TrackDependency(Type interfaceType, int resolvedTimes)
    {
        _redis
            .GetDatabase(MetricDatabase)
            .HashIncrement(
                $"Dependencies_{_suffix}",
                interfaceType.FullName,
                resolvedTimes,
                CommandFlags.FireAndForget
            );
    }
}
