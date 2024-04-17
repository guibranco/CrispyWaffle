using System;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Log.Providers
{
    /// <summary>
    /// Log provider interface
    /// </summary>
    public interface ILogProvider
    {
        /// <summary>
        /// Sets the log level of the instance
        /// </summary>
        /// <param name="level">The log level</param>
        void SetLevel(LogLevel level);

        /// <summary>
        /// Logs the message with fatal level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        void Fatal(string category, string message);

        /// <summary>
        /// Logs the message with error level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        void Error(string category, string message);

        /// <summary>
        /// Logs the message with warning level
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        void Warning(string category, string message);

        /// <summary>
        /// Logs the message with info level
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        void Info(string category, string message);

        /// <summary>
        /// Logs the message with trace level
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        void Trace(string category, string message);

        /// <summary>
        /// Logs the message with trace level and shows exception details.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="exception">The exception.</param>
        void Trace(string category, string message, Exception exception);

        /// <summary>
        /// Logs the exception details with trace level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="exception">The exception.</param>
        void Trace(string category, Exception exception);

        /// <summary>
        /// Logs the message with debug level
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        void Debug(string category, string message);

        /// <summary>
        /// Logs the message as a file/attachment with a file name/identifier with debug level
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="content">The content to be stored</param>
        /// <param name="identifier">The file name of the content. This can be a filename, a key, a identifier. Depends upon each implementation</param>
        void Debug(string category, string content, string identifier);

        /// <summary>
        /// Logs the message as a file/attachment with a file name/identifier with debug level using a custom serializer or default.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <typeparam name="T">any class that can be serialized to the <paramref name="customFormat"/> serializer format</typeparam>
        /// <param name="content">The object to be serialized</param>
        /// <param name="identifier">The filename/attachment identifier (file name or key)</param>
        /// <param name="customFormat">(Optional) the custom serializer format</param>
        void Debug<T>(string category, T content, string identifier, SerializerFormat customFormat)
            where T : class, new();
    }
}
