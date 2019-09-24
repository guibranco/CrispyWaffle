namespace CrispyWaffle.Events
{
    /// <summary>
    /// The event handler interface
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public interface IEventHandler<in TEvent> where TEvent : IEvent

    {
        /// <summary>
        /// Handles the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        void Handle(TEvent args);
    }
}
