namespace CrispyWaffle.Commands;

/// <summary>
/// The command handler interface.
/// </summary>
/// <typeparam name="TCommand">The type of the command.</typeparam>
/// <typeparam name="TResult">The type of command result.</typeparam>
public interface ICommandHandler<in TCommand, out TResult>
    where TCommand : ICommand
{
    /// <summary>
    /// Handles the specified command.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns>Result of the handled command.</returns>
    TResult Handle(TCommand command);
}
