using System.Threading.Tasks;
using CrispyWaffle.Composition;
using CrispyWaffle.Log;

namespace CrispyWaffle.Events;

/// <summary>
/// Provides methods to raise events and invoke their handlers.
/// This class supports both synchronous and asynchronous event handling.
/// </summary>
public static class EventsConsumer
{
    /// <summary>
    /// Raises the specified event synchronously and invokes all registered handlers for the event.
    /// Handlers are resolved from the service locator and invoked in the order they are registered.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to raise. The event must implement <see cref="IEvent"/>.</typeparam>
    /// <param name="event">The event to raise. It will be passed to all the registered handlers.</param>
    /// <remarks>
    /// This method resolves all synchronous event handlers for the specified event type
    /// and calls their <see cref="IEventHandler{TEvent}.Handle(TEvent)"/> method.
    /// </remarks>
    public static void Raise<TEvent>(TEvent @event)
        where TEvent : IEvent
    {
        var handlers = ServiceLocator.ResolveAll<IEventHandler<TEvent>>();

        foreach (var handler in handlers)
        {
            LogConsumer.Trace(
                $"Calling {handler.GetType().FullName} for event {@event.GetType().FullName}"
            );

            handler.Handle(@event);
        }
    }

    /// <summary>
    /// Raises the specified event asynchronously and invokes all registered asynchronous handlers for the event.
    /// Handlers are resolved from the service locator and invoked in the order they are registered.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to raise. The event must implement <see cref="IEvent"/>.</typeparam>
    /// <param name="event">The event to raise. It will be passed to all the registered handlers asynchronously.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This method resolves all asynchronous event handlers for the specified event type
    /// and calls their <see cref="IEventHandlerAsync{TEvent}.HandleAsync(TEvent)"/> method.
    /// The event handlers will be executed asynchronously in parallel.
    /// </remarks>
    public static async Task RaiseAsync<TEvent>(TEvent @event)
        where TEvent : IEvent
    {
        await Task.Run(() =>
        {
            var handlers = ServiceLocator.ResolveAll<IEventHandlerAsync<TEvent>>();
            foreach (var handler in handlers)
            {
                LogConsumer.Trace(
                    $"Calling {handler.GetType().FullName} for event {@event.GetType().FullName}"
                );
                handler.HandleAsync(@event);
            }
        });
    }
}
