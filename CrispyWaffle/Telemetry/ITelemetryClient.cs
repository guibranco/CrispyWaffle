namespace CrispyWaffle.Telemetry
{
    using System;

    /// <summary>
    /// The telemetry client interface.
    /// </summary>
    public interface ITelemetryClient
    {
        /// <summary>
        /// Gets the hit.
        /// </summary>
        /// <param name="hitName">Name of the hit.</param>
        /// <returns></returns>
        int GetHit(string hitName);

        /// <summary>
        /// Tracks the hit.
        /// </summary>
        /// <param name="hitName">Name of the hit.</param>
        void TrackHit(string hitName);

        /// <summary>
        /// Removes the hit.
        /// </summary>
        /// <param name="hitName">Name of the hit.</param>
        bool RemoveHit(string hitName);

        /// <summary>
        /// Gets the event.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The event.</param>
        /// <returns></returns>
        TEvent GetEvent<TEvent>(ITelemetryEvent<TEvent> @event) where TEvent : class, new();

        /// <summary>
        /// Tracks the event.
        /// </summary>
        /// <param name="event">The event.</param>
        void TrackEvent<TEvent>(ITelemetryEvent<TEvent> @event) where TEvent : class, new();

        /// <summary>
        /// Tracks the event.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The event.</param>
        /// <param name="ttl">The TTL.</param>
        void TrackEvent<TEvent>(ITelemetryEvent<TEvent> @event, TimeSpan ttl) where TEvent : class, new();

        /// <summary>
        /// Gets the metric.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        /// <param name="variation">The variation.</param>
        /// <returns></returns>
        int GetMetric(string metricName, string variation);

        /// <summary>
        /// Tracks the metric.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        /// <param name="variation">The variation.</param>
        void TrackMetric(string metricName, string variation);

        /// <summary>
        /// Removes the metric.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        /// <param name="variation">The variation.</param>
        bool RemoveMetric(string metricName, string variation);

        /// <summary>
        /// Tracks the exception.
        /// </summary>
        /// <param name="exceptionType">Type of the exception.</param>
        void TrackException(Type exceptionType);

        /// <summary>
        /// Tracks the dependency.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="resolvedTimes">The resolved times.</param>
        void TrackDependency(Type interfaceType, int resolvedTimes);
    }
}