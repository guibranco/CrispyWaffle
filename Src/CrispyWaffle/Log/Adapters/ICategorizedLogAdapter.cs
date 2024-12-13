using System;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Log.Adapters;

/// <summary>
/// Defines an interface for logging messages with different severity levels, categorized by specific tags.
/// Implementers of this interface should support logging at various levels (fatal, error, warning, info, trace, debug) within specific categories.
/// This allows for organized and categorized logging, which is useful for filtering, searching, and analyzing logs.
/// </summary>
public interface ICategorizedLogAdapter : ILogAdapter
{
    /// <summary>
    /// Logs a fatal-level message under a specific category.
    /// Fatal-level logs indicate a critical error that causes a complete failure of the application.
    /// </summary>
    /// <param name="category">The category under which the fatal log should be recorded. Typically represents a module or feature.</param>
    /// <param name="message">The message detailing the fatal error.</param>
    void CategorizedFatal(string category, string message);

    /// <summary>
    /// Logs an error-level message under a specific category.
    /// Error-level logs represent significant issues that do not stop the application but may require attention.
    /// </summary>
    /// <param name="category">The category under which the error log should be recorded.</param>
    /// <param name="message">The message detailing the error.</param>
    void CategorizedError(string category, string message);

    /// <summary>
    /// Logs a warning-level message under a specific category.
    /// Warning-level logs indicate potential issues or circumstances that may lead to problems if not addressed.
    /// </summary>
    /// <param name="category">The category under which the warning log should be recorded.</param>
    /// <param name="message">The message detailing the warning.</param>
    void CategorizedWarning(string category, string message);

    /// <summary>
    /// Logs an informational message under a specific category.
    /// Info-level logs represent general operational messages or status updates.
    /// </summary>
    /// <param name="category">The category under which the info log should be recorded.</param>
    /// <param name="message">The message providing informative details.</param>
    void CategorizedInfo(string category, string message);

    /// <summary>
    /// Logs a trace-level message under a specific category.
    /// Trace-level logs are typically used for debugging and provide detailed information about application execution.
    /// </summary>
    /// <param name="category">The category under which the trace log should be recorded.</param>
    /// <param name="message">The message providing trace details.</param>
    void CategorizedTrace(string category, string message);

    /// <summary>
    /// Logs a trace-level message under a specific category, including exception details.
    /// This method is useful for logging exceptions or errors that occurred during application execution.
    /// </summary>
    /// <param name="category">The category under which the trace log should be recorded.</param>
    /// <param name="message">The message providing trace details.</param>
    /// <param name="exception">The exception object containing details of the error.</param>
    void CategorizedTrace(string category, string message, Exception exception);

    /// <summary>
    /// Logs an exception under a specific category at the trace level.
    /// This method logs the exception details without any associated message.
    /// </summary>
    /// <param name="category">The category under which the trace log should be recorded.</param>
    /// <param name="exception">The exception to be logged.</param>
    void CategorizedTrace(string category, Exception exception);

    /// <summary>
    /// Logs a debug-level message under a specific category.
    /// Debug-level logs are typically used for development and troubleshooting, providing information about the system state.
    /// </summary>
    /// <param name="category">The category under which the debug log should be recorded.</param>
    /// <param name="message">The message containing debug information.</param>
    void CategorizedDebug(string category, string message);

    /// <summary>
    /// Logs content to a file or attachment with a specific identifier at the debug level under a category.
    /// This method is used to log large or complex data that may not be suitable for simple string messages.
    /// </summary>
    /// <param name="category">The category under which the debug log should be recorded.</param>
    /// <param name="content">The content to be logged, such as a file or complex data.</param>
    /// <param name="identifier">An identifier that associates the content with a specific log entry.</param>
    void CategorizedDebug(string category, string content, string identifier);

    /// <summary>
    /// Logs content to a file or attachment with a specific identifier at the debug level under a category.
    /// This method allows logging of custom object types, which will be serialized according to the specified format.
    /// </summary>
    /// <typeparam name="T">The type of the content to be logged. This type must be a reference type.</typeparam>
    /// <param name="category">The category under which the debug log should be recorded.</param>
    /// <param name="content">The content to be logged, such as an object or file.</param>
    /// <param name="identifier">An identifier that associates the content with a specific log entry.</param>
    /// <param name="customFormat">An optional parameter specifying a custom serializer format for serializing the content. The default value is <see cref="SerializerFormat.None"/>.</param>
    void CategorizedDebug<T>(
        string category,
        T content,
        string identifier,
        SerializerFormat customFormat = SerializerFormat.None
    )
        where T : class;
}
