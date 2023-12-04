﻿using System.Threading.Tasks;
using CrispyWaffle.Composition;
using CrispyWaffle.Log;

namespace CrispyWaffle.Events
{
    /// <summary>
    /// Manage events raising
    /// </summary>
    public static class EventsConsumer
    {
        /// <summary>
        /// Raises the specified event.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The event.</param>
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
        /// Raises the specified event asynchronously.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The event.</param>
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
}
