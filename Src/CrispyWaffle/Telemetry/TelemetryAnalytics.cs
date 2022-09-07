namespace CrispyWaffle.Telemetry
{
    using Composition;
    using Log;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// The telemetry analytics class.
    /// </summary>
    public static class TelemetryAnalytics
    {
        #region Private fields

        /// <summary>
        /// The clients
        /// </summary>
        private static readonly List<ITelemetryClient> _clients = new List<ITelemetryClient>();

        #endregion

        #region Public methods

        /// <summary>
        /// Adds the client.
        /// </summary>
        /// <typeparam name="TTelemetryClient">The type of the i telemetry client.</typeparam>
        /// <returns></returns>
        public static ITelemetryClient AddClient<TTelemetryClient>() where TTelemetryClient : ITelemetryClient
        {
            var client = ServiceLocator.Resolve<TTelemetryClient>();
            LogConsumer.Trace("Adding telemetry client of type {0}", client.GetType().FullName);
            _clients.Add(client);
            return client;
        }

        /// <summary>
        /// Adds the client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public static ITelemetryClient AddClient(ITelemetryClient client)
        {
            LogConsumer.Trace("Adding telemetry client of type {0}", client.GetType().FullName);
            _clients.Add(client);
            return client;
        }

        /// <summary>
        /// Tracks the hit.
        /// </summary>
        /// <param name="hitName">Name of the hit.</param>
        public static void TrackHit([Localizable(false)]string hitName)
        {
            LogConsumer.Trace("Tracking hit of {0}", hitName);
            foreach (var client in _clients)
            {
                client.TrackHit(hitName);
            }
        }

        /// <summary>
        /// Gets the hit.
        /// </summary>
        /// <param name="hitName">Name of the hit.</param>
        /// <returns></returns>
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
        /// Removes the hit.
        /// </summary>
        /// <param name="hitName">Name of the hit.</param>
        public static void RemoveHit(string hitName)
        {
            LogConsumer.Trace("Removing hits of {0}", hitName);
            foreach (var client in _clients)
            {
                client.RemoveHit(hitName);
            }
        }

        /// <summary>
        /// Tracks the event.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The event.</param>
        public static void TrackEvent<TEvent>(ITelemetryEvent<TEvent> @event) where TEvent : class, new()
        {
            LogConsumer.Trace("Tracking event of type {0}", typeof(TEvent).FullName);
            foreach (var client in _clients)
            {
                client.TrackEvent(@event);
            }
        }

        /// <summary>
        /// Tracks the event.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The event.</param>
        /// <param name="ttl">The TTL.</param>
        public static void TrackEvent<TEvent>(ITelemetryEvent<TEvent> @event, TimeSpan ttl) where TEvent : class, new()
        {
            LogConsumer.Trace("Tracking event of type {0}", typeof(TEvent).FullName);
            foreach (var client in _clients)
            {
                client.TrackEvent(@event, ttl);
            }
        }

        /// <summary>
        /// Gets the event.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The event.</param>
        /// <returns></returns>
        public static TEvent GetEvent<TEvent>(ITelemetryEvent<TEvent> @event) where TEvent : class, new()
        {
            LogConsumer.Trace("Getting event of type {0} with key {1}", typeof(TEvent).FullName, @event.Name);
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
        /// Gets the metric.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        /// <param name="variation">The variation.</param>
        /// <returns></returns>
        public static int GetMetric([Localizable(false)] string metricName, [Localizable(false)] string variation)
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
        /// Tracks the metric.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        /// <param name="variation">The variation.</param>
        public static void TrackMetric([Localizable(false)]string metricName, [Localizable(false)]string variation)
        {
            LogConsumer.Trace("Tracking metric {0} with variation {1}", metricName, variation);
            foreach (var client in _clients)
            {
                client.TrackMetric(metricName, variation);
            }
        }

        /// <summary>
        /// Removes the metric.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        /// <param name="variation">The variation.</param>
        public static void RemoveMetric([Localizable(false)]string metricName, [Localizable(false)]string variation)
        {
            LogConsumer.Trace("Deleting metric {0} with variation {1}", metricName, variation);
            foreach (var client in _clients)
            {
                client.RemoveMetric(metricName, variation);
            }
        }

        /// <summary>
        /// Tracks the exception.
        /// </summary>
        /// <param name="exceptionType">Type of the exception.</param>
        public static void TrackException(Type exceptionType)
        {
            LogConsumer.Trace("Tracking exception of type {0}", exceptionType.FullName);
            foreach (var client in _clients)
            {
                client.TrackException(exceptionType);
            }
        }

        /// <summary>
        /// Tracks the dependency.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="resolvedTimes">The resolved times.</param>
        public static void TrackDependency(Type interfaceType, int resolvedTimes)
        {
            LogConsumer.Trace("Tracking dependency of {0}, resolved {1} time{2}",
                              interfaceType.FullName,
                              resolvedTimes,
                              resolvedTimes == 1
                                  ? string.Empty
                                  : @"s");
            foreach (var client in _clients)
            {
                client.TrackDependency(interfaceType, resolvedTimes);
            }
        }

        #endregion
    }
}
