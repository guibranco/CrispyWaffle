namespace CrispyWaffle.Telemetry
{
    /// <summary>
    /// The telemetry event interface.
    /// Use this class to track Orders, Products and Shipments (As used today by LogToDatabase)
    ///
    /// Name is the Redis key
    /// Category is the Redis database
    /// Event is the data to be stored
    /// </summary>
    public interface ITelemetryEvent<out TEvent> where TEvent : class, new()
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        int Category { get; }

        /// <summary>
        /// Gets or sets the event.
        /// </summary>
        /// <value>
        /// The event.
        /// </value>
        TEvent Event { get; }

    }
}
