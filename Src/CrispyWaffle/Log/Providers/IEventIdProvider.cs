namespace CrispyWaffle.Log.Providers
{
    /// <summary>
    /// Interface IEventIdProvider
    /// </summary>
    public interface IEventIdProvider
    {
        /// <summary>
        /// Computes the event identifier.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>System.UInt16.</returns>
        ushort ComputeEventId(string message);
    }
}
