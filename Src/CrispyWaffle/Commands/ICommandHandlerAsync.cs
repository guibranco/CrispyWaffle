using System.Threading.Tasks;

namespace CrispyWaffle.Commands;

/// <summary>
/// The asynchronous command hanlder interface.
/// </summary>
/// <typeparam name="TCommand">The type of the command.</typeparam>
/// <typeparam name="TResult">The type of command result.</typeparam>
public interface ICommandHandlerAsync<in TCommand, TResult>
    where TCommand : ICommand
{
    /// <summary>
    /// Handles the command asynchronously.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns>Result of the handled command.</returns>
    Task<TResult> HandleAsync(TCommand command);
}
