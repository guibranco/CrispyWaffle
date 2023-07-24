// ***********************************************************************
// Assembly         : CrispyWaffle.Redis
// Author           : Guilherme Branco Stracini
// Created          : 09-06-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-06-2020
// ***********************************************************************
// <copyright file="RedisTelemetryClient.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace CrispyWaffle.Redis.Telemetry
{
    using Composition;
    using Extensions;
    using Utils.Communications;
    using Serialization;
    using CrispyWaffle.Telemetry;
    using StackExchange.Redis;
    using System;

    /// <summary>
    /// The Redis telemetry client class.
    /// </summary>
    /// <seealso cref="ITelemetryClient" />
    public sealed class RedisTelemetryClient : ITelemetryClient
    {
        #region Private fields

        /// <summary>
        /// The redis
        /// </summary>
        private readonly RedisConnector _redis;

        /// <summary>
        /// The default TTL
        /// </summary>
        private readonly TimeSpan _defaultTTL;

        /// <summary>
        /// The suffix
        /// </summary>
        private readonly string _suffix;

        #endregion

        #region ~Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisTelemetryClient" /> class.
        /// </summary>
        public RedisTelemetryClient()
        {
            _redis = ServiceLocator.Resolve<RedisConnector>();
            _defaultTTL = new TimeSpan(30, 0, 0, 0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisTelemetryClient" /> class.
        /// </summary>
        /// <param name="redis">The redis.</param>
        /// <param name="suffix">The key suffix</param>
        public RedisTelemetryClient(RedisConnector redis, string suffix)
        {
            _redis = redis;
            _suffix = suffix;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets the hit database.
        /// </summary>
        /// <value>The hit database.</value>
        public int HitDatabase { get; set; }

        /// <summary>
        /// Gets or sets the metric database.
        /// </summary>
        /// <value>The metric database.</value>
        public int MetricDatabase { get; set; }

        #endregion

        #region Implementation of ITelemetryClient

        /// <summary>
        /// Gets the hit.
        /// </summary>
        /// <param name="hitName">Name of the hit.</param>
        /// <returns>System.Int32.</returns>
        public int GetHit(string hitName)
        {
            var result = _redis
                .GetDatabase(HitDatabase)
                .StringGet(hitName, CommandFlags.PreferReplica);
            return result.HasValue ? result.ToString().ToInt32() : 0;
        }

        /// <summary>
        /// Tracks the hit.
        /// </summary>
        /// <param name="hitName">Name of the hit.</param>
        public void TrackHit(string hitName)
        {
            _redis.GetDatabase(HitDatabase).StringIncrement(hitName, 1, CommandFlags.FireAndForget);
        }

        /// <summary>
        /// Removes the hit.
        /// </summary>
        /// <param name="hitName">Name of the hit.</param>
        /// <returns><c>true</c> if remove hit, <c>false</c> otherwise.</returns>
        public bool RemoveHit(string hitName)
        {
            return _redis.GetDatabase(HitDatabase).KeyDelete(hitName);
        }

        /// <summary>
        /// Gets the event.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The event.</param>
        /// <returns>TEvent.</returns>
        public TEvent GetEvent<TEvent>(ITelemetryEvent<TEvent> @event)
            where TEvent : class, new()
        {
            var valueBytes = _redis
                .GetDatabase(@event.Category)
                .StringGet(@event.Name, CommandFlags.PreferReplica);
            return !valueBytes.HasValue ? null : _redis.Serializer.Deserialize<TEvent>(valueBytes);
        }

        /// <summary>
        /// Tracks the event.
        /// </summary>
        /// <typeparam name="TEvent">The type of the t event.</typeparam>
        /// <param name="event">Name of the event.</param>
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
        /// Tracks the event.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The event.</param>
        /// <param name="ttl">The TTL.</param>
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
        /// Gets the metric.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        /// <param name="variation">The variation.</param>
        /// <returns>System.Int32.</returns>
        public int GetMetric(string metricName, string variation)
        {
            var value = _redis
                .GetDatabase(MetricDatabase)
                .HashGet(metricName, variation, CommandFlags.PreferReplica);
            return value.HasValue ? value.ToString().ToInt32() : 0;
        }

        /// <summary>
        /// Tracks the metric.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        /// <param name="variation">The variation.</param>
        public void TrackMetric(string metricName, string variation)
        {
            _redis
                .GetDatabase(MetricDatabase)
                .HashIncrement(metricName, variation, 1, CommandFlags.FireAndForget);
        }

        /// <summary>
        /// Removes the metric.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        /// <param name="variation">The variation.</param>
        /// <returns><c>true</c> if remove metric, <c>false</c> otherwise.</returns>
        public bool RemoveMetric(string metricName, string variation)
        {
            return _redis.GetDatabase(MetricDatabase).HashDelete(metricName, variation);
        }

        /// <summary>
        /// Tracks the exception.
        /// </summary>
        /// <param name="exceptionType">Type of the exception.</param>
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
        /// Tracks the dependency.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="resolvedTimes">The resolved times.</param>
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

        #endregion
    }
}
