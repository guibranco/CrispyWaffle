using System;
using System.Threading.Tasks;
using CrispyWaffle.Commands;

namespace CrispyWaffle.Tests.Commands;

/// <summary>
/// Class TestObjects.
/// </summary>
internal sealed class TestObjects
{
    /// <summary>
    /// The test done command class.
    /// </summary>
    /// <seealso cref="ICommand"/>
    /// <param name="identifier">The identifier.</param>
    /// <param name="text">The text.</param>
    internal sealed class TestDoneCommand(Guid identifier, string text) : ICommand
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public Guid Identifier { get; } = identifier;

        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text { get; } = text;

        /// <summary>
        /// Gets the created date at IME.
        /// </summary>
        /// <value>The created date at IME.</value>
        public DateTime CreatedDateTime { get; } = DateTime.UtcNow;
    }

    /// <summary>
    /// The test done result command class.
    /// </summary>
    /// <param name="text">The text.</param>
    internal sealed class TestDoneResultCommand(string text)
    {
        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text { get; } = text;
    }

    /// <summary>
    /// The handler done command class.
    /// </summary>
    public sealed class TestDoneCommandHandler
        : ICommandHandler<TestDoneCommand, TestDoneResultCommand>,
            ICommandHandlerAsync<TestDoneCommand, TestDoneResultCommand>
    {
        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>Result of the handled command.</returns>
        TestDoneResultCommand ICommandHandler<TestDoneCommand, TestDoneResultCommand>.Handle(
            TestDoneCommand command
        ) =>
            new(
                $"Sample done action handled: {command.Identifier} - {command.Text} - {command.CreatedDateTime:dd/MM/yyyy HH:mm:ss}"
            );

        /// <summary>
        /// Handles the specified command asynchronously.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>Result of the handled command.</returns>
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
