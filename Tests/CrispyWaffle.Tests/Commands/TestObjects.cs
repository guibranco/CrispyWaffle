using System;
using System.Threading.Tasks;
using CrispyWaffle.Commands;

namespace CrispyWaffle.Tests.Commands;

internal sealed class TestObjects
{
    internal sealed class TestDoneCommand(Guid identifier, string text) : ICommand
    {
        public Guid Identifier { get; } = identifier;

        public string Text { get; } = text;

        public DateTime CreatedDateTime { get; } = DateTime.UtcNow;
    }

    internal sealed class TestDoneResultCommand(string text)
    {
        public string Text { get; } = text;
    }

    public sealed class TestDoneCommandHandler
        : ICommandHandler<TestDoneCommand, TestDoneResultCommand>,
            ICommandHandlerAsync<TestDoneCommand, TestDoneResultCommand>
    {
        TestDoneResultCommand ICommandHandler<TestDoneCommand, TestDoneResultCommand>.Handle(
            TestDoneCommand command
        ) =>
            new(
                $"Sample done action handled: {command.Identifier} - {command.Text} - {command.CreatedDateTime:dd/MM/yyyy HH:mm:ss}"
            );

        Task<TestDoneResultCommand> ICommandHandlerAsync<
            TestDoneCommand,
            TestDoneResultCommand
        >.HandleAsync(TestDoneCommand command) =>
            Task.Run(
                () =>
                    new TestDoneResultCommand(
                        $"Sample done action handled: {command.Identifier} - {command.Text} - {command.CreatedDateTime:dd/MM/yyyy HH:mm:ss}"
                    )
            );
    }
}
