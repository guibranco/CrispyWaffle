using System;
using System.Threading;
using CrispyWaffle.Log.Providers;

namespace CrispyWaffle.Log.Handlers;

/// <summary>
/// Defines the contract for an exception handler.
/// Implementations of this interface are responsible for handling exceptions by logging them,
/// and may also define custom behavior for handling unhandled exceptions or thread exceptions.
/// </summary>
public interface IExceptionHandler
{
    /// <summary>
    /// Logs an exception at the ERROR level.
    /// The exception is typically logged with its <see cref="Exception.Message"/>, <see cref="Exception.StackTrace"/>,
    /// and <see cref="Type.FullName"/>. Inner exceptions are logged recursively until no more inner exceptions remain.
    /// The specific behavior of the logging, including which details are logged, depends on the implementation of the adapter.
    /// </summary>
    /// <param name="exception">The exception to be logged.</param>
    /// <remarks>
    /// This method requires the <see cref="LogLevel.Error"/> flag to be set for logging.
    /// </remarks>
    void Handle(Exception exception);

    /// <summary>
    /// Handles unhandled exceptions by casting <see cref="UnhandledExceptionEventArgs.ExceptionObject"/> to an <see cref="Exception"/>
    /// and calling the <see cref="Handle(Exception)"/> method.
    /// This is the default behavior; individual implementations can define custom handling.
    /// </summary>
    /// <param name="sender">The sender of the unhandled exception event.</param>
    /// <param name="args">An instance of <see cref="UnhandledExceptionEventArgs"/> containing information about the unhandled exception.</param>
    /// <remarks>
    /// This method requires the <see cref="LogLevel.Error"/> flag to be set for logging.
    /// </remarks>
    void Handle(object sender, UnhandledExceptionEventArgs args);

    /// <summary>
    /// Handles exceptions that occur in a thread by processing the <see cref="ThreadExceptionEventArgs.Exception"/>
    /// and logging the exception details using the <see cref="Handle(Exception)"/> method.
    /// </summary>
    /// <param name="sender">The sender of the thread exception event.</param>
    /// <param name="args">An instance of <see cref="ThreadExceptionEventArgs"/> containing the event data, including the exception to be logged.</param>
    void Handle(object sender, ThreadExceptionEventArgs args);

    /// <summary>
    /// Adds a log provider to the exception handler for logging exception details.
    /// This method allows the addition of a custom log provider to be used for handling exceptions.
    /// </summary>
    /// <typeparam name="TLogProvider">The type of the log provider that implements <see cref="ILogProvider"/>.</typeparam>
    /// <param name="type">The type of the log provider to be added, indicating the kind of logging (e.g., full exception details or filtered logs).</param>
    /// <returns>An instance of <see cref="ILogProvider"/> that has been added to the exception handler.</returns>
    ILogProvider AddLogProvider<TLogProvider>(ExceptionLogType type)
        where TLogProvider : ILogProvider;
}
