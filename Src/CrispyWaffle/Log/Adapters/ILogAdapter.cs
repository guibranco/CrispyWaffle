using System;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Log.Adapters;

/// <summary>
/// Represents a log adapter interface for logging messages at various levels.
/// Provides methods to log messages, exceptions, and to set log levels.
/// </summary>
/// <seealso cref="IDisposable" />
public interface ILogAdapter : IDisposable
{
    /// <summary>
    /// Changes the log level of the log adapter instance.
    /// </summary>
    /// <param name="level">The new <seealso cref="LogLevel"/> to set for the instance.</param>
    void SetLevel(LogLevel level);

    /// <summary>
    /// Serializes the provided <paramref name="content"/> and saves it to a file with the specified <paramref name="identifier"/>.
    /// Uses the default serializer format or a custom format provided via <paramref name="customFormat"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to be serialized and logged.</typeparam>
    /// <param name="content">The object to be serialized and saved.</param>
    /// <param name="identifier">The file name where the serialized content will be saved.</param>
    /// <param name="customFormat">Optional custom serializer format to use. If not provided, the default serializer format will be used.</param>
    /// <remarks>
    /// This method requires the <see cref="LogLevel.Debug"/> log level to be enabled.
    /// </remarks>
    void Debug<T>(
        T content,
        string identifier,
        SerializerFormat customFormat = SerializerFormat.None
    )
        where T : class;

    /// <summary>
    /// Saves the specified <paramref name="content"/> (as a string) to a file with the given <paramref name="fileName"/>.
    /// </summary>
    /// <param name="content">The content to be written to the file.</param>
    /// <param name="fileName">The name of the file to store the content.</param>
    /// <remarks>
    /// This method requires the <see cref="LogLevel.Debug"/> log level to be enabled.
    /// </remarks>
    void Debug(string content, string fileName);

    /// <summary>
    /// Logs a message at the DEBUG level.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    /// <remarks>
    /// This method requires the <see cref="LogLevel.Debug"/> log level to be enabled.
    /// </remarks>
    void Debug(string message);

    /// <summary>
    /// Logs an exception at the TRACE level.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    void Trace(Exception exception);

    /// <summary>
    /// Logs a message at the TRACE level along with exception details.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    /// <param name="exception">The exception to be logged.</param>
    void Trace(string message, Exception exception);

    /// <summary>
    /// Logs a message at the TRACE level.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    /// <remarks>
    /// This method requires the <see cref="LogLevel.Trace"/> log level to be enabled.
    /// </remarks>
    void Trace(string message);

    /// <summary>
    /// Logs a message at the INFO level.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    /// <remarks>
    /// This method requires the <see cref="LogLevel.Info"/> log level to be enabled.
    /// </remarks>
    void Info(string message);

    /// <summary>
    /// Logs a message at the WARNING level.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    /// <remarks>
    /// This method requires the <see cref="LogLevel.Warning"/> log level to be enabled.
    /// </remarks>
    void Warning(string message);

    /// <summary>
    /// Logs a message at the ERROR level.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    /// <remarks>
    /// This method requires the <see cref="LogLevel.Error"/> log level to be enabled.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Naming",
        "CA1716:Identifiers should not match keywords",
        Justification = "Design decision."
    )]
    void Error(string message);

    /// <summary>
    /// Logs a message at the FATAL level.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    void Fatal(string message);
}
