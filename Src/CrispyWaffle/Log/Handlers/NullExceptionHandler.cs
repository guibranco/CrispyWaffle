using System;
using System.Threading;
using CrispyWaffle.Composition;
using CrispyWaffle.Log.Providers;

namespace CrispyWaffle.Log.Handlers;

/// <summary>
/// A no-op implementation of the <see cref="IExceptionHandler"/> interface.
/// This handler does not perform any action when an exception is encountered.
/// </summary>
/// <seealso cref="IExceptionHandler" />
public sealed class NullExceptionHandler : IExceptionHandler
{
    /// <summary>
    /// Handles the provided exception by doing nothing.
    /// <para>
    /// This method is intended as a placeholder or fallback handler that does not log or process the exception.
    /// The exception could include properties such as the message, stack trace, and type, but no action is taken here.
    /// </para>
    /// </summary>
    /// <param name="exception">The exception to be handled. In this case, it is ignored.</param>
    /// <remarks>
    /// This method is typically used when the <see cref="IExceptionHandler"/> does not need to handle exceptions.
    /// It is triggered when the <see cref="LogLevel"/> is set to <c>ERROR</c>, but no logging occurs.
    /// </remarks>
    public void Handle(Exception exception) { }

    /// <summary>
    /// Handles the unhandled exception event by casting the <see cref="UnhandledExceptionEventArgs.ExceptionObject"/> to an <see cref="Exception"/>
    /// and calling <see cref="IExceptionHandler.Handle(Exception)"/>.
    /// This method provides a default behavior, and implementations can customize it as needed.
    /// </summary>
    /// <param name="sender">The sender of the unhandled exception event.</param>
    /// <param name="args">An instance of <see cref="UnhandledExceptionEventArgs"/> containing the unhandled exception data.</param>
    /// <remarks>
    /// This method requires that the <see cref="LogLevel"/> is set to <c>ERROR</c>, but in this case, the exception is ignored.
    /// </remarks>
    public void Handle(object sender, UnhandledExceptionEventArgs args) { }

    /// <summary>
    /// Handles a thread exception event by processing the exception from the <see cref="ThreadExceptionEventArgs"/>.
    /// This method provides the default behavior, but can be overridden by custom implementations.
    /// </summary>
    /// <param name="sender">The sender of the thread exception event.</param>
    /// <param name="args">An instance of <see cref="ThreadExceptionEventArgs"/> containing the exception data.</param>
    /// <remarks>
    /// This method requires that the <see cref="LogLevel"/> is set to <c>ERROR</c>, but in this case, no action is performed.
    /// </remarks>
    public void Handle(object sender, ThreadExceptionEventArgs args) { }

    /// <summary>
    /// Adds a log provider of the specified type for handling exception logs.
    /// This method resolves the log provider from the service locator.
    /// </summary>
    /// <typeparam name="TLogProvider">The type of the log provider to add.</typeparam>
    /// <param name="type">The type of log entry (e.g., <see cref="ExceptionLogType"/>).</param>
    /// <returns>The resolved log provider instance.</returns>
    /// <remarks>
    /// This method can be used to extend the handler with custom logging providers.
    /// The log provider is resolved using the <see cref="ServiceLocator"/>.
    /// </remarks>
    public ILogProvider AddLogProvider<TLogProvider>(ExceptionLogType type)
        where TLogProvider : ILogProvider => ServiceLocator.Resolve<TLogProvider>();
}
