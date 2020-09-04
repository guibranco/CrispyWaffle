using CrispyWaffle.Events;
using System;
using Xunit;

namespace CrispyWaffle.Tests.Events
{
    /// <summary>
    /// Class EventsTests.
    /// </summary>
    public class EventsTests
    {
        /// <summary>
        /// Defines the test method ValidateRaiseEvent.
        /// </summary>
        [Fact]
        public void ValidateRaiseEvent()
        {
            var @event = new TestObjects.TestDoneEvent(Guid.NewGuid(), @"Sample test");
            EventsConsumer.Raise(@event);
        }

        /// <summary>
        /// Defines the test method ValidateRaiseEventWithException.
        /// </summary>
        [Fact]
        public void ValidateRaiseEventWithException()
        {
            var @event = new TestObjects.ExceptionEvent(Guid.NewGuid());
            var result = Assert.Throws<NotImplementedException>(() => EventsConsumer.Raise(@event));
            Assert.Equal(@event.Identifier.ToString(), result.Message);
        }
    }
}
