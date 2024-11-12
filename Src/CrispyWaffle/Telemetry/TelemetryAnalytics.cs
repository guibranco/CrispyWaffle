using System;
using System.Collections.Generic;
using System.ComponentModel;
using CrispyWaffle.Composition;
using CrispyWaffle.Log;

namespace CrispyWaffle.Telemetry
{
    /// <summary>
    /// Provides methods for tracking and managing telemetry events, hits, metrics, exceptions, and dependencies.
    /// </summary>
    /// <remarks>
    /// The <see cref="TelemetryAnalytics"/> class acts as a central hub for interacting with registered telemetry clients.
    /// It allows adding clients, tracking different telemetry data types (events, hits, metrics, etc.),
    /// and retrieving or removing the tracked data. The methods ensure consistent logging and analytics across all clients.
    /// </remarks>
    public static class TelemetryAnalytics
    {
        /// <summary>
        /// A collection of registered telemetry clients that handle telemetry data tracking.
        /// </summary>
        private static readonly List<ITelemetryClient> _clients = new List<ITelemetryClient>();

        /// <summary>
        /// Adds a telemetry client of the specified type to the collection of clients.
        /// </summary>
        /// <typeparam name="TTelemetryClient">The type of telemetry client to add.</typeparam>
        /// <returns>The added telemetry client.</returns>
        /// <remarks>
        /// This method resolves an instance of the specified telemetry client type using the
        /// <see cref="ServiceLocator"/> and adds it to the client collection for tracking telemetry data.
        /// </remarks>
        public static ITelemetryClient AddClient<TTelemetryClient>()
            where TTelemetryClient : ITelemetryClient
        {
            var client = ServiceLocator.Resolve<TTelemetryClient>();
            LogConsumer.Trace("Adding telemetry client of type {0}", client.GetType().FullName);
            _clients.Add(client);
            return client;
        }

        /// <summary>
        /// Adds an existing telemetry client to the collection of clients.
        /// </summary>
        /// <param name="client">The telemetry client to add.</param>
        /// <returns>The added telemetry client.</returns>
        /// <remarks>
        /// This method adds a pre-existing telemetry client instance to the client collection.
        /// </remarks>
        public static ITelemetryClient AddClient(ITelemetryClient client)
        {
            LogConsumer.Trace("Adding telemetry client of type {0}", client.GetType().FullName);
            _clients.Add(client);
            return client;
        }

        /// <summary>
        /// Tracks a hit with the specified name across all registered telemetry clients.
        /// </summary>
        /// <param name="hitName">The name of the hit to track.</param>
        /// <remarks>
        /// This method will notify all registered telemetry clients to track the specified hit.
        /// </remarks>
        public static void TrackHit([Localizable(false)] string hitName)
        {
            LogConsumer.Trace("Tracking hit of {0}", hitName);
            foreach (var client in _clients)
            {
                client.TrackHit(hitName);
            }
        }

        /// <summary>
        /// Retrieves the count of a specific hit from the telemetry clients.
        /// </summary>
        /// <param name="hitName">The name of the hit to retrieve.</param>
        /// <returns>The count of the specified hit.</returns>
        /// <remarks>
        /// This method will check each client for the specified hit and return the count from the first client
        /// that returns a non-zero value.
        /// </remarks>
        public static int GetHit(string hitName)
        {
            int temp;
            LogConsumer.Trace("Getting hits of {0}", hitName);
            foreach (var client in _clients)
            {
                if ((temp = client.GetHit(hitName)) > 0)
                {
                    return temp;
                }
            }

            return 0;
        }

        /// <summary>
        /// Removes a tracked hit from all telemetry clients.
        /// </summary>
        /// <param name="hitName">The name of the hit to remove.</param>
        /// <remarks>
        /// This method will notify all registered telemetry clients to remove the specified hit.
        /// </remarks>
        public static void RemoveHit(string hitName)
        {
            LogConsumer.Trace("Removing hits of {0}", hitName);
            foreach (var client in _clients)
            {
                client.RemoveHit(hitName);
            }
        }

        /// <summary>
        /// Tracks an event of the specified type across all registered telemetry clients.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to track.</typeparam>
        /// <param name="event">The event data to track.</param>
        /// <remarks>
        /// This method will notify all registered telemetry clients to track the specified event.
        /// </remarks>
        public static void TrackEvent<TEvent>(ITelemetryEvent<TEvent> @event)
            where TEvent : class, new()
        {
            LogConsumer.Trace("Tracking event of type {0}", typeof(TEvent).FullName);
            foreach (var client in _clients)
            {
                client.TrackEvent(@event);
            }
        }

        /// <summary>
        /// Tracks an event of the specified type with a Time-to-Live (TTL) value across all registered telemetry clients.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to track.</typeparam>
        /// <param name="event">The event data to track.</param>
        /// <param name="ttl">The Time-to-Live value to associate with the event.</param>
        /// <remarks>
        /// This method will notify all registered telemetry clients to track the event with the specified TTL.
        /// </remarks>
        public static void TrackEvent<TEvent>(ITelemetryEvent<TEvent> @event, TimeSpan ttl)
            where TEvent : class, new()
        {
            LogConsumer.Trace("Tracking event of type {0}", typeof(TEvent).FullName);
            foreach (var client in _clients)
            {
                client.TrackEvent(@event, ttl);
            }
        }

        /// <summary>
        /// Retrieves an event of the specified type from the telemetry clients.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to retrieve.</typeparam>
        /// <param name="event">The event data to retrieve.</param>
        /// <returns>The event data, or <c>null</c> if not found.</returns>
        /// <remarks>
        /// This method checks each registered telemetry client for the specified event and returns the first found event.
        /// </remarks>
        public static TEvent GetEvent<TEvent>(ITelemetryEvent<TEvent> @event)
            where TEvent : class, new()
        {
            LogConsumer.Trace(
                "Getting event of type {0} with key {1}",
                typeof(TEvent).FullName,
                @event.Name
            );
            TEvent result;
            foreach (var client in _clients)
            {
                if ((result = client.GetEvent(@event)) != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves the value of a specific metric from the telemetry clients.
        /// </summary>
        /// <param name="metricName">The name of the metric to retrieve.</param>
        /// <param name="variation">The variation of the metric to retrieve.</param>
        /// <returns>The value of the metric, or 0 if not found.</returns>
        /// <remarks>
        /// This method checks each registered telemetry client for the specified metric and returns the value from
        /// the first client that returns a non-zero value.
        /// </remarks>
        public static int GetMetric(
            [Localizable(false)] string metricName,
            [Localizable(false)] string variation
        )
        {
            var result = 0;
            LogConsumer.Trace("Getting metric {0} with variation {1}", metricName, variation);
            foreach (var client in _clients)
            {
                result = client.GetMetric(metricName, variation);
                if (result != 0)
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Tracks a metric with the specified name and variation across all registered telemetry clients.
        /// </summary>
        /// <param name="metricName">The name of the metric to track.</param>
        /// <param name="variation">The variation of the metric to track.</param>
        /// <remarks>
        /// This method will notify all registered telemetry clients to track the specified metric.
        /// </remarks>
        public static void TrackMetric(
            [Localizable(false)] string metricName,
            [Localizable(false)] string variation
        )
        {
            LogConsumer.Trace("Tracking metric {0} with variation {1}", metricName, variation);
            foreach (var client in _clients)
            {
                client.TrackMetric(metricName, variation);
            }
        }

        /// <summary>
        /// Removes a tracked metric with the specified name and variation from all telemetry clients.
        /// </summary>
        /// <param name="metricName">The name of the metric to remove.</param>
        /// <param name="variation">The variation of the metric to remove.</param>
        /// <remarks>
        /// This method will notify all registered telemetry clients to remove the specified metric.
        /// </remarks>
        public static void RemoveMetric(
            [Localizable(false)] string metricName,
            [Localizable(false)] string variation
        )
        {
            LogConsumer.Trace("Deleting metric {0} with variation {1}", metricName, variation);
            foreach (var client in _clients)
            {
                client.RemoveMetric(metricName, variation);
            }
        }

        /// <summary>
        /// Tracks an exception of the specified type across all registered telemetry clients.
        /// </summary>
        /// <param name="exceptionType">The type of exception to track.</param>
        /// <remarks>
        /// This method will notify all registered telemetry clients to track the specified exception type.
        /// </remarks>
        public static void TrackException(Type exceptionType)
        {
            LogConsumer.Trace("Tracking exception of type {0}", exceptionType.FullName);
            foreach (var client in _clients)
            {
                client.TrackException(exceptionType);
            }
        }

        /// <summary>
        /// Tracks a dependency of the specified type, and the number of times it was resolved, across all registered telemetry clients.
        /// </summary>
        /// <param name="interfaceType">The type of dependency to track.</param>
        /// <param name="resolvedTimes">The number of times the dependency was resolved.</param>
        /// <remarks>
        /// This method will notify all registered telemetry clients to track the specified dependency and its resolved times.
        /// </remarks>
        public static void TrackDependency(Type interfaceType, int resolvedTimes)
        {
            LogConsumer.Trace(
                "Tracking dependency of {0}, resolved {1} time{2}",
                interfaceType.FullName,
                resolvedTimes,
                resolvedTimes == 1 ? string.Empty : @"s"
            );
            foreach (var client in _clients)
            {
                client.TrackDependency(interfaceType, resolvedTimes);
            }
        }
    }
}
