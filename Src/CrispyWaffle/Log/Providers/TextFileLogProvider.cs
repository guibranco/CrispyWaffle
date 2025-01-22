using System;
using CrispyWaffle.Log.Adapters;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Log.Providers;

/// <summary>
/// A log provider that writes log messages to a text file.
/// This provider uses a specific adapter to handle different log levels and log messages.
/// </summary>
/// <seealso cref="ILogProvider" />
/// <remarks>
/// Initializes a new instance of the <see cref="TextFileLogProvider"/> class with the specified adapter.
/// </remarks>
/// <param name="adapter">The <see cref="ITextFileLogAdapter"/> to use for handling log messages.</param>
/// <remarks>
/// This constructor allows the provider to delegate log handling to an adapter,
/// which is responsible for writing log entries to a text file.
/// </remarks>
public sealed class TextFileLogProvider(ITextFileLogAdapter adapter) : ILogProvider
{
    /// <summary>
    /// Sets the log level for this log provider. The log level determines the minimum severity of logs that will be recorded.
    /// </summary>
    /// <param name="level">The log level to set for this provider.</param>
    public void SetLevel(LogLevel level) => adapter.SetLevel(level);

    /// <summary>
    /// Logs a message with a <see cref="LogLevel.Fatal"/> severity.
    /// The message is recorded in the text file under the specified category.
    /// </summary>
    /// <param name="category">The category under which the log message will be logged.</param>
    /// <param name="message">The message to log.</param>
    public void Fatal(string category, string message) =>
        adapter.CategorizedFatal(category, message);

    /// <summary>
    /// Logs a message with a <see cref="LogLevel.Error"/> severity.
    /// The message is recorded in the text file under the specified category.
    /// </summary>
    /// <param name="category">The category under which the log message will be logged.</param>
    /// <param name="message">The message to log.</param>
    public void Error(string category, string message) =>
        adapter.CategorizedError(category, message);

    /// <summary>
    /// Logs a message with a <see cref="LogLevel.Warning"/> severity.
    /// The message is recorded in the text file under the specified category.
    /// </summary>
    /// <param name="category">The category under which the log message will be logged.</param>
    /// <param name="message">The message to log.</param>
    public void Warning(string category, string message) =>
        adapter.CategorizedWarning(category, message);

    /// <summary>
    /// Logs a message with a <see cref="LogLevel.Info"/> severity.
    /// The message is recorded in the text file under the specified category.
    /// </summary>
    /// <param name="category">The category under which the log message will be logged.</param>
    /// <param name="message">The message to log.</param>
    public void Info(string category, string message) => adapter.CategorizedInfo(category, message);

    /// <summary>
    /// Logs a message with a <see cref="LogLevel.Trace"/> severity.
    /// The message is recorded in the text file under the specified category.
    /// </summary>
    /// <param name="category">The category under which the log message will be logged.</param>
    /// <param name="message">The message to log.</param>
    public void Trace(string category, string message) =>
        adapter.CategorizedTrace(category, message);

    /// <summary>
    /// Logs a message with a <see cref="LogLevel.Trace"/> severity and includes exception details.
    /// The message along with exception information is recorded in the text file under the specified category.
    /// </summary>
    /// <param name="category">The category under which the log message will be logged.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception whose details will be logged.</param>
    public void Trace(string category, string message, Exception exception) =>
        adapter.CategorizedTrace(category, message, exception);

    /// <summary>
    /// Logs exception details with a <see cref="LogLevel.Trace"/> severity.
    /// The exception details are recorded in the text file under the specified category.
    /// </summary>
    /// <param name="category">The category under which the exception will be logged.</param>
    /// <param name="exception">The exception whose details will be logged.</param>
    public void Trace(string category, Exception exception) =>
        adapter.CategorizedTrace(category, exception);

    /// <summary>
    /// Logs a message with a <see cref="LogLevel.Debug"/> severity.
    /// The message is recorded in the text file under the specified category.
    /// </summary>
    /// <param name="category">The category under which the log message will be logged.</param>
    /// <param name="message">The message to log.</param>
    public void Debug(string category, string message) =>
        adapter.CategorizedDebug(category, message);

    /// <summary>
    /// Logs the message as a file attachment with a <see cref="LogLevel.Debug"/> severity.
    /// The file will be stored under the specified identifier.
    /// </summary>
    /// <param name="category">The category under which the log message will be logged.</param>
    /// <param name="content">The content to be stored as the file attachment.</param>
    /// <param name="identifier">The name of the file attachment.</param>
    public void Debug(string category, string content, string identifier) =>
        adapter.CategorizedDebug(category, content, identifier);

    /// <summary>
    /// Logs a message as a file attachment with a <see cref="LogLevel.Debug"/> severity.
    /// The message is serialized using the specified custom serializer format or the default format.
    /// </summary>
    /// <param name="category">The category under which the log message will be logged.</param>
    /// <typeparam name="T">The type of the object to be serialized.</typeparam>
    /// <param name="content">The object to be serialized and stored as the file attachment.</param>
    /// <param name="identifier">The filename or attachment identifier.</param>
    /// <param name="customFormat">An optional custom serializer format to use.</param>
    /// <remarks>
    /// This method allows custom serialization of log content before storing it as an attachment.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Critical Code Smell",
        "S1006:Method overrides should not change parameter defaults",
        Justification = "Needed here."
    )]
    public void Debug<T>(
        string category,
        T content,
        string identifier,
        SerializerFormat customFormat = SerializerFormat.None
    )
        where T : class, new() =>
        adapter.CategorizedDebug(category, content, identifier, customFormat);
}
