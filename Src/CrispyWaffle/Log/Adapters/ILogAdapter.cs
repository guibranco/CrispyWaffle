using System;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Log.Adapters
{
    /// <summary>
    /// Interface for Log Adapter.
    /// </summary>
    /// <seealso cref="IDisposable" />
    public interface ILogAdapter : IDisposable
    {
        /// <summary>
        /// Change the LogLevel of Log Adapter instance.
        /// </summary>
        /// <param name="level">The new <seealso cref="LogLevel" /> level of the instance</param>
        void SetLevel(LogLevel level);

        /// <summary>
        /// Save the serializer version of <paramref name="content" /> in the file <paramref name="identifier" />,
        /// using default SerializerFormat, or a custom serializer format provided by <paramref name="customFormat" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter <paramref name="content" />.</typeparam>
        /// <param name="content">The object/instance of a class to be serialized and saved in a disk file.</param>
        /// <param name="identifier">The file name to be persisted to disk with the content.</param>
        /// <param name="customFormat">Whatever or not to use a custom Serializer adapter different than one that is default for type.</param>
        /// <remarks>Requires LogLevel.DEBUG flag.</remarks>
        void Debug<T>(
            T content,
            string identifier,
            SerializerFormat customFormat = SerializerFormat.None
        )
            where T : class;

        /// <summary>
        /// Save the string <paramref name="content" /> into a file with name <paramref name="fileName" />.
        /// </summary>
        /// <param name="content">The file content.</param>
        /// <param name="fileName">The file name.</param>
        /// <remarks>Requires LogLevel.DEBUG flag.</remarks>
        void Debug(string content, string fileName);

        /// <summary>
        /// Logs a message as DEBUG level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <remarks>Requires LogLevel.DEBUG flag.</remarks>
        void Debug(string message);

        /// <summary>
        /// Logs the exception with trace level.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void Trace(Exception exception);

        /// <summary>
        /// Logs the message with trace level and shows exception details.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="exception">The exception.</param>
        void Trace(string message, Exception exception);

        /// <summary>
        /// Logs a message as TRACE level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <remarks>Requires LogLevel.TRACE flag.</remarks>
        void Trace(string message);

        /// <summary>
        /// Logs a message as INFO level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <remarks>Requires LogLevel.INFO flag.</remarks>
        void Info(string message);

        /// <summary>
        /// Logs a message as WARNING level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <remarks>Requires LogLevel.WARNING flag.</remarks>
        void Warning(string message);

        /// <summary>
        /// Logs a message as ERROR level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <remarks>Requires LogLevel.ERROR flag.</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Naming",
            "CA1716:Identifiers should not match keywords",
            Justification = "Design decision."
        )]
        void Error(string message);

        /// <summary>
        /// Logs a message as FATAL level.
        /// </summary>
        /// <param name="message">The message.</param>
        void Fatal(string message);
    }
}
