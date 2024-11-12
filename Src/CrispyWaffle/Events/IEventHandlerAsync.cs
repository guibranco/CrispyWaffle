using System.Threading.Tasks;

namespace CrispyWaffle.Events;

/// <summary>
/// Defines an asynchronous event handler that processes events of type <typeparamref name="TEvent"/>.
/// </summary>
/// <typeparam name="TEvent">The type of the event that this handler processes. It must implement the <see cref="IEvent"/> interface.</typeparam>
/// <remarks>
/// This interface is designed for handling events asynchronously, providing a mechanism to process
/// events in a non-blocking manner. Implementations of this interface will define the logic for handling
/// the event and performing any associated asynchronous work.
/// </remarks>
public interface IEventHandlerAsync<in TEvent>
    where TEvent : IEvent
{
    /// <summary>
    /// Asynchronously handles the specified event arguments.
    /// </summary>
    /// <param name="args">The event arguments to be handled. This will be an instance of <typeparamref name="TEvent"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task completes when the event has been processed.</returns>
    /// <remarks>
    /// The implementation should include the logic to process the event in an asynchronous manner,
    /// allowing for non-blocking operations such as I/O tasks or external service calls.
    /// </remarks>
    Task HandleAsync(TEvent args);
}
