using System;
using System.Threading.Tasks;
using CrispyWaffle.Events;
using CrispyWaffle.Log;

namespace CrispyWaffle.Tests.Events;

internal class TestObjects
{
    internal sealed class TestDoneEvent : IEvent
    {
        public Guid Identifier { get; }

        public string Text { get; }

        public DateTime CreatedDateTIme { get; }

        public TestDoneEvent(Guid identifier, string text)
        {
            Identifier = identifier;
            Text = text;
            CreatedDateTIme = DateTime.Now;
        }
    }

    public sealed class LogDoneEvent
        : IEventHandler<TestDoneEvent>,
            IEventHandlerAsync<TestDoneEvent>
    {
        public void Handle(TestDoneEvent args)
        {
            LogConsumer.Info(
                @"Sample done action handled: {0} - {1} - {2:dd/MM/yyyy HH:mm:ss}",
                args.Identifier,
                args.Text,
                args.CreatedDateTIme
            );
        }

        public async Task HandleAsync(TestDoneEvent args)
        {
            await Task.Run(
                () =>
                    LogConsumer.Info(
                        @"Sample done action handled: {0} - {1} - {2:dd/MM/yyyy HH:mm:ss}",
                        args.Identifier,
                        args.Text,
                        args.CreatedDateTIme
                    )
            );
        }
    }

    internal sealed class ExceptionEvent : IEvent
    {
        public Guid Identifier { get; }

        public ExceptionEvent(Guid identifier) => Identifier = identifier;
    }

    public sealed class ThrowExceptionEvent : IEventHandler<ExceptionEvent>
    {
        public void Handle(ExceptionEvent args) =>
            throw new NotImplementedException(args.Identifier.ToString());
    }
}
