using System;
using CrispyWaffle.Log.Adapters;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Log.Providers
{
    /// <summary>
    /// Text file log provider.
    /// </summary>
    public sealed class TextFileLogProvider : ILogProvider
    {
        /// <summary>
        /// Text file log adapter.
        /// </summary>
        private readonly ITextFileLogAdapter _adapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFileLogProvider"/> class.
        /// </summary>
        /// <param name="adapter">The adapter.</param>
        public TextFileLogProvider(ITextFileLogAdapter adapter)
        {
            _adapter = adapter;
        }

        /// <summary>
        /// Sets the log level of the instance.
        /// </summary>
        /// <param name="level">The log level.</param>
        public void SetLevel(LogLevel level) => _adapter.SetLevel(level);

        /// <summary>
        /// Logs the message with fatal level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        public void Fatal(string category, string message) =>
            _adapter.CategorizedFatal(category, message);

        /// <summary>
        /// Logs the message with error level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        public void Error(string category, string message) =>
            _adapter.CategorizedError(category, message);

        /// <summary>
        /// Logs the message with warning level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        public void Warning(string category, string message) =>
            _adapter.CategorizedWarning(category, message);

        /// <summary>
        /// Logs the message with info level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        public void Info(string category, string message) =>
            _adapter.CategorizedInfo(category, message);

        /// <summary>
        /// Logs the message with trace level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        public void Trace(string category, string message) =>
            _adapter.CategorizedTrace(category, message);

        /// <summary>
        /// Logs the message with trace level and shows exception details.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="exception">The exception.</param>
        public void Trace(string category, string message, Exception exception) =>
            _adapter.CategorizedTrace(category, message, exception);

        /// <summary>
        /// Logs the exception details with trace level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="exception">The exception.</param>
        public void Trace(string category, Exception exception) =>
            _adapter.CategorizedTrace(category, exception);

        /// <summary>
        /// Logs the message with debug level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        public void Debug(string category, string message) =>
            _adapter.CategorizedDebug(category, message);

        /// <summary>
        /// Logs the message as a file with file name specified in <paramref name="identifier"/> with debug level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="content">The content to be stored.</param>
        /// <param name="identifier">The name of the attachment.</param>
        public void Debug(string category, string content, string identifier) =>
            _adapter.CategorizedDebug(category, content, identifier);

        /// <summary>
        /// Logs the message as a /attachment with a file name with debug level using a custom serializer or default.
        /// </summary>
        /// <param name="category">The category</param>
        /// <typeparam name="T">any class that can be serialized to the <paramref name="customFormat"/> serializer format.</typeparam>
        /// <param name="content">The object to be serialized.</param>
        /// <param name="identifier">The filename/attachment identifier (file name or key).</param>
        /// <param name="customFormat">(Optional) the custom serializer format.</param>
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
            _adapter.CategorizedDebug(category, content, identifier, customFormat);
    }
}
