using System;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Log.Adapters
{
    /// <summary>
    /// Categorized log adapter interface
    /// </summary>
    public interface ICategorizedLogAdapter : ILogAdapter
    {
        /// <summary>
        /// Logs the message with fatal level in the specified category
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        void CategorizedFatal(string category, string message);

        /// <summary>
        /// Logs the message with error level in the specified category
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged.</param>
        void CategorizedError(string category, string message);

        /// <summary>
        /// Logs the message with warning level in the specified category
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged.</param>
        void CategorizedWarning(string category, string message);

        /// <summary>
        /// Logs the message with info level in the specified category
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged.</param>
        void CategorizedInfo(string category, string message);

        /// <summary>
        /// Logs the message with trace level in the specified category
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged.</param>
        void CategorizedTrace(string category, string message);

        /// <summary>
        /// Logs the message with trace level in the specified category with exception details.
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="exception">The exception.</param>
        void CategorizedTrace(string category, string message, Exception exception);

        /// <summary>
        /// Logs the exception details with trace level in the specified category
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="exception">The exception.</param>
        void CategorizedTrace(string category, Exception exception);

        /// <summary>
        /// Logs the message with debug level in the specified category
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged.</param>
        void CategorizedDebug(string category, string message);

        /// <summary>
        /// Logs the content to a file/attachment with the specified identifier with debug level in the specified category
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="content">The message to be logged.</param>
        /// <param name="identifier">The identifier of the content</param>
        void CategorizedDebug(string category, string content, string identifier);

        /// <summary>
        /// Logs the content to a file/attachment with the specified identifier with debug level in the specified category
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="content">The message to be logged.</param>
        /// <param name="identifier">The identifier of the content</param>
        /// <param name="customFormat">(Optional)Specify a custom serializer format for the content</param>
        void CategorizedDebug<T>(
            string category,
            T content,
            string identifier,
            SerializerFormat customFormat = SerializerFormat.None
        )
            where T : class;
    }
}
