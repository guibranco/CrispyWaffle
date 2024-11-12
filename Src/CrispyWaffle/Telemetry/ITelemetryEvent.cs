namespace CrispyWaffle.Telemetry;

/// <summary>
/// Represents a telemetry event used to track data such as Orders, Products, and Shipments.
/// This interface is used by implementations like <c>LogToDatabase</c> for storing events in Redis.
/// </summary>
/// <typeparam name="TEvent">The type of event data to be stored. The type must be a reference type and have a parameterless constructor.</typeparam>
/// <remarks>
/// The <see cref="Name"/> property represents the Redis key.
/// The <see cref="Category"/> property represents the Redis database.
/// The <see cref="Event"/> property holds the data associated with the event to be stored.
/// </remarks>
public interface ITelemetryEvent<out TEvent>
    where TEvent : class, new()
{
    /// <summary>
    /// Gets the name of the telemetry event, used as the Redis key.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the name of the event.
    /// </value>
    string Name { get; }

    /// <summary>
    /// Gets the category of the telemetry event, used as the Redis database identifier.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> representing the Redis database number.
    /// </value>
    int Category { get; }

    /// <summary>
    /// Gets the event data to be stored.
    /// </summary>
    /// <value>
    /// The event data of type <typeparamref name="TEvent"/>.
    /// </value>
    TEvent Event { get; }
}
