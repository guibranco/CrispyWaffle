using System;
using System.Threading.Tasks;
using CrispyWaffle.Events;
using Xunit;

namespace CrispyWaffle.Tests.Events;

public class EventsTests
{
    [Fact]
    public void ValidateRaiseEvent()
    {
        var @event = new TestObjects.TestDoneEvent(Guid.NewGuid(), @"Sample test");
        EventsConsumer.Raise(@event);
        Assert.Equal("Sample test", @event.Text);
    }

    [Fact]
    public async Task ValidateRaiseEventAsync()
    {
        var @event = new TestObjects.TestDoneEvent(Guid.NewGuid(), @"Sample test");
        await EventsConsumer.RaiseAsync(@event);
        Assert.Equal("Sample test", @event.Text);
    }

    [Fact]
    public void ValidateRaiseEventWithException()
    {
        var @event = new TestObjects.ExceptionEvent(Guid.NewGuid());
        var result = Assert.Throws<NotImplementedException>(() => EventsConsumer.Raise(@event));
        Assert.Equal(@event.Identifier.ToString(), result.Message);
    }
}
