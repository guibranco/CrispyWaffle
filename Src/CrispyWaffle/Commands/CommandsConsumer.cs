using System.Threading.Tasks;
using CrispyWaffle.Composition;
using CrispyWaffle.Log;

namespace CrispyWaffle.Commands;

/// <summary>
/// Manage commands raising.
/// </summary>
public static class CommandsConsumer
{
    /// <summary>
    /// Raise the specified command.
    /// </summary>
    /// <typeparam name="TCommand">The type of command.</typeparam>
    /// <typeparam name="TResult">The type of command result.</typeparam>
    /// <param name="command">The command.</param>
    /// <returns>Result of the handled command.</returns>
    public static TResult Raise<TCommand, TResult>(TCommand command)
        where TCommand : ICommand
    {
        var handler = ServiceLocator.Resolve<ICommandHandler<TCommand, TResult>>();

        LogConsumer.Trace(
            $"Calling {handler.GetType().FullName} for command {command.GetType().FullName}"
        );

        return handler.Handle(command);
    }

    /// <summary>
    /// Raises the specified command asynchronously.
    /// </summary>
    /// <typeparam name="TCommand">The type of command.</typeparam>
    /// <typeparam name="TResult">The type of command result.</typeparam>
    /// <param name="command">The command.</param>
    /// <returns>Result of the handled command.</returns>
    public static Task<TResult> RaiseAsync<TCommand, TResult>(TCommand command)
        where TCommand : ICommand
    {
        var handler = ServiceLocator.Resolve<ICommandHandlerAsync<TCommand, TResult>>();

        LogConsumer.Trace(
            $"Calling {handler.GetType().FullName} for command {command.GetType().FullName}"
        );

        return handler.HandleAsync(command);
    }
}
