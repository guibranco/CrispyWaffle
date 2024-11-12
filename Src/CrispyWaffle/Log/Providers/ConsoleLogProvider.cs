using System;
using CrispyWaffle.Log.Adapters;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Log.Providers;

/// <summary>
/// Provides console-based logging by using the specified <see cref="IConsoleLogAdapter"/>.
/// This class follows the singleton pattern to ensure only one instance of the adapter is used
/// throughout the application's lifetime, preventing unnecessary reinitializations.
/// </summary>
public sealed class ConsoleLogProvider : ILogProvider
{
    /// <summary>
    /// A synchronization object used to ensure thread-safe initialization of the adapter.
    /// </summary>
    private static readonly object _syncRoot = new object();

    /// <summary>
    /// The adapter responsible for sending log messages to the console.
    /// This field is initialized only once during the application's lifecycle.
    /// </summary>
    private static IConsoleLogAdapter _adapter;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleLogProvider"/> class.
    /// This constructor ensures that only one <see cref="IConsoleLogAdapter"/> instance is initialized,
    /// even in multi-threaded environments.
    /// </summary>
    /// <param name="adapter">The <see cref="IConsoleLogAdapter"/> instance used for logging messages.</param>
    public ConsoleLogProvider(IConsoleLogAdapter adapter)
    {
        if (_adapter != null)
        {
            return;
        }

        lock (_syncRoot)
        {
            if (_adapter == null)
            {
                _adapter = adapter;
            }
        }
    }

    /// <summary>
    /// Sets the log level for the console log provider. Only log messages at or above the specified log level will be logged.
    /// </summary>
    /// <param name="level">The <see cref="LogLevel"/> to set for the provider. Determines the minimum log level that will be logged.</param>
    public void SetLevel(LogLevel level) => _adapter.SetLevel(level);

    /// <summary>
    /// Logs a message with a fatal log level, typically indicating a critical error that may cause the application to fail.
    /// </summary>
    /// <param name="category">The category or context of the log message (e.g., the class or module where the log is generated).</param>
    /// <param name="message">The fatal error message to be logged, describing a severe issue that requires immediate attention.</param>
    public void Fatal(string category, string message) => _adapter.Fatal(message);

    /// <summary>
    /// Logs a message with an error log level, indicating an issue that needs to be addressed, but does not necessarily cause failure.
    /// </summary>
    /// <param name="category">The category or context of the log message (e.g., the class or module where the log is generated).</param>
    /// <param name="message">The error message to be logged, describing the issue that needs attention.</param>
    public void Error(string category, string message) => _adapter.Error(message);

    /// <summary>
    /// Logs a message with a warning log level, indicating a potential issue or an abnormal condition that may require investigation.
    /// </summary>
    /// <param name="category">The category or context of the log message (e.g., the class or module where the log is generated).</param>
    /// <param name="message">The warning message to be logged, describing a condition that should be monitored.</param>
    public void Warning(string category, string message) => _adapter.Warning(message);

    /// <summary>
    /// Logs a message with an informational log level, typically used to report general information about the application's operation.
    /// </summary>
    /// <param name="category">The category or context of the log message (e.g., the class or module where the log is generated).</param>
    /// <param name="message">The informational message to be logged, providing details about the application's state or activities.</param>
    public void Info(string category, string message) => _adapter.Info(message);

    /// <summary>
    /// Logs a message with a trace log level, typically used to output detailed diagnostic information for debugging purposes.
    /// </summary>
    /// <param name="category">The category or context of the log message (e.g., the class or module where the log is generated).</param>
    /// <param name="message">The trace message to be logged, providing detailed information about the application's internal behavior.</param>
    public void Trace(string category, string message) => _adapter.Trace(message);

    /// <summary>
    /// Logs a message with a trace log level, including exception details for debugging purposes.
    /// This is useful when tracking errors or failures and needing additional context from exceptions.
    /// </summary>
    /// <param name="category">The category or context of the log message (e.g., the class or module where the log is generated).</param>
    /// <param name="message">The trace message to be logged.</param>
    /// <param name="exception">The exception whose details will be logged along with the trace message.</param>
    public void Trace(string category, string message, Exception exception) =>
        _adapter.Trace(message, exception);

    /// <summary>
    /// Logs only the exception details with a trace log level. This is used when the primary goal is to capture
    /// exception information without any additional context.
    /// </summary>
    /// <param name="category">The category or context of the log message (e.g., the class or module where the log is generated).</param>
    /// <param name="exception">The exception details to be logged.</param>
    public void Trace(string category, Exception exception) => _adapter.Trace(exception);

    /// <summary>
    /// Logs a message with a debug log level, typically used to output information helpful for debugging and development.
    /// </summary>
    /// <param name="category">The category or context of the log message (e.g., the class or module where the log is generated).</param>
    /// <param name="message">The debug message to be logged, containing information useful for troubleshooting.</param>
    public void Debug(string category, string message) => _adapter.Debug(message);

    /// <summary>
    /// This method is currently a placeholder and does not perform any actions. It exists to provide a method signature
    /// consistent with the <see cref="Debug{T}"/> method for generic usage.
    /// </summary>
    /// <param name="category">The category or context of the log message (e.g., the class or module where the log is generated).</param>
    /// <param name="content">This parameter is not used in the current implementation.</param>
    /// <param name="identifier">This parameter is not used in the current implementation.</param>
    public void Debug(string category, string content, string identifier) =>
        _adapter.Debug(content, identifier);

    /// <summary>
    /// This method is currently a placeholder and does not perform any actions. It exists to provide a method signature
    /// consistent with the <see cref="Debug{T}"/> method for generic usage with custom formats.
    /// </summary>
    /// <param name="category">The category or context of the log message (e.g., the class or module where the log is generated).</param>
    /// <typeparam name="T">This type parameter is not used in the current implementation.</typeparam>
    /// <param name="content">This parameter is not used in the current implementation.</param>
    /// <param name="identifier">This parameter is not used in the current implementation.</param>
    /// <param name="customFormat">This parameter is not used in the current implementation.</param>
    public void Debug<T>(
        string category,
        T content,
        string identifier,
        SerializerFormat customFormat = SerializerFormat.None
    )
        where T : class, new() => _adapter.Debug(content, identifier, customFormat);
}
