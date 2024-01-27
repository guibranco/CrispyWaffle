using System.Threading.Tasks;

namespace CrispyWaffle.Events;

/// <summary>
/// The asynchronous event handler interface.
/// </summary>
/// <typeparam name="TEvent">The type of the event.</typeparam>
public interface IEventHandlerAsync<in TEvent>
    where TEvent : IEvent
{
    /// <summary>
    /// Handles the arguments asynchronously.
    /// </summary>
    /// <param name="args">The arguments.</param>
    Task HandleAsync(TEvent args);
}
