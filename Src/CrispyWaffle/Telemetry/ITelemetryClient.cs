using System;

namespace CrispyWaffle.Telemetry;

/// <summary>
/// Interface for a telemetry client, providing methods to track, retrieve, and remove telemetry data such as hits, events, metrics, and exceptions.
/// </summary>
public interface ITelemetryClient
{
    /// <summary>
    /// Retrieves the current count of a specific hit, identified by its name.
    /// </summary>
    /// <param name="hitName">The name of the hit to retrieve the count for.</param>
    /// <returns>The current count of the specified hit.</returns>
    int GetHit(string hitName);

    /// <summary>
    /// Tracks a specific hit event, identified by its name.
    /// </summary>
    /// <param name="hitName">The name of the hit to track.</param>
    void TrackHit(string hitName);

    /// <summary>
    /// Removes a specific hit, identified by its name.
    /// </summary>
    /// <param name="hitName">The name of the hit to remove.</param>
    /// <returns><c>true</c> if the hit was successfully removed; otherwise, <c>false</c>.</returns>
    bool RemoveHit(string hitName);

    /// <summary>
    /// Retrieves a specific telemetry event of the given type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to retrieve.</typeparam>
    /// <param name="event">An instance of the telemetry event to retrieve.</param>
    /// <returns>The requested telemetry event.</returns>
    TEvent GetEvent<TEvent>(ITelemetryEvent<TEvent> @event)
        where TEvent : class, new();

    /// <summary>
    /// Tracks the specified telemetry event.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to be tracked. This must be a reference type and have a parameterless constructor.</typeparam>
    /// <param name="event">The telemetry event to be tracked. This event contains the data to be sent for telemetry purposes.</param>
    /// <remarks>
    /// This method captures the specified event data and sends it to the telemetry system for tracking and analysis.
    /// It is used to track events that are defined by the <typeparamref name="TEvent"/> type, and the event
    /// object should contain the relevant data to be tracked.
    /// </remarks>
    void TrackEvent<TEvent>(ITelemetryEvent<TEvent> @event)
        where TEvent : class, new();

    /// <summary>
    /// Tracks a specific telemetry event with a time-to-live (TTL) duration.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to track.</typeparam>
    /// <param name="event">An instance of the telemetry event to track.</param>
    /// <param name="ttl">The TTL duration for the event.</param>
    void TrackEvent<TEvent>(ITelemetryEvent<TEvent> @event, TimeSpan ttl)
        where TEvent : class, new();

    /// <summary>
    /// Retrieves the current value of a specific metric, identified by its name and variation.
    /// </summary>
    /// <param name="metricName">The name of the metric to retrieve.</param>
    /// <param name="variation">The variation of the metric to retrieve.</param>
    /// <returns>The current value of the specified metric.</returns>
    int GetMetric(string metricName, string variation);

    /// <summary>
    /// Tracks a specific metric, identified by its name and variation.
    /// </summary>
    /// <param name="metricName">The name of the metric to track.</param>
    /// <param name="variation">The variation of the metric to track.</param>
    void TrackMetric(string metricName, string variation);

    /// <summary>
    /// Removes a specific metric, identified by its name and variation.
    /// </summary>
    /// <param name="metricName">The name of the metric to remove.</param>
    /// <param name="variation">The variation of the metric to remove.</param>
    /// <returns><c>true</c> if the metric was successfully removed; otherwise, <c>false</c>.</returns>
    bool RemoveMetric(string metricName, string variation);

    /// <summary>
    /// Tracks a specific exception type.
    /// </summary>
    /// <param name="exceptionType">The type of the exception to track.</param>
    void TrackException(Type exceptionType);

    /// <summary>
    /// Tracks the resolution of a specific dependency, identified by its interface type and the number of times it was resolved.
    /// </summary>
    /// <param name="interfaceType">The interface type of the dependency.</param>
    /// <param name="resolvedTimes">The number of times the dependency was resolved.</param>
    void TrackDependency(Type interfaceType, int resolvedTimes);
}
