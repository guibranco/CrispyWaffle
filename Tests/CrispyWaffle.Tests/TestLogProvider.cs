using CrispyWaffle.Extensions;
using CrispyWaffle.Log;
using CrispyWaffle.Log.Providers;
using CrispyWaffle.Serialization;
using System;

namespace CrispyWaffle.Tests
{
    /// <summary>
    /// Class TestLogProvider.
    /// Implements the <see cref="CrispyWaffle.Log.Providers.ILogProvider" />
    /// </summary>
    /// <seealso cref="CrispyWaffle.Log.Providers.ILogProvider" />
    internal class TestLogProvider : ILogProvider
    {
        #region Implementation of ILogProvider

        /// <summary>
        /// Sets the log level of the instance
        /// </summary>
        /// <param name="level">The log level</param>
        public void SetLevel(LogLevel level)
        {
            System.Diagnostics.Debug.WriteLine(level.GetHumanReadableValue());
        }

        /// <summary>
        /// Logs the message with fatal level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        public void Fatal(string category, string message)
        {
            System.Diagnostics.Debug.WriteLine("Fatal: {0} - {1}", category, message);
        }

        /// <summary>
        /// Logs the message with error level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        public void Error(string category, string message)
        {
            System.Diagnostics.Debug.WriteLine("Error: {0} - {1}", category, message);
        }

        /// <summary>
        /// Logs the message with warning level
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged</param>
        public void Warning(string category, string message)
        {
            System.Diagnostics.Debug.WriteLine("Warning: {0} - {1}", category, message);
        }

        /// <summary>
        /// Logs the message with info level 
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged</param>
        public void Info(string category, string message)
        {
            System.Diagnostics.Debug.WriteLine("Info: {0} - {1}", category, message);
        }

        /// <summary>
        /// Logs the message with trace level
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged</param>
        public void Trace(string category, string message)
        {
            System.Diagnostics.Debug.WriteLine("Trace: {0} - {1}", category, message);
        }

        /// <summary>
        /// Logs the message with trace level and shows exception details.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="exception">The exception.</param>
        public void Trace(string category, string message, Exception exception)
        {
            System.Diagnostics.Debug.WriteLine("Trace: {0} - {1}", category, message);
            WriteExceptionDetails("Trace", category, exception);
        }

        /// <summary>
        /// Logs the exception details with trace level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="exception">The exception.</param>
        public void Trace(string category, Exception exception)
        {
            WriteExceptionDetails("Trace", category, exception);
        }

        /// <summary>
        /// Logs the message with debug level
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged</param>
        public void Debug(string category, string message)
        {
            System.Diagnostics.Debug.WriteLine("Debug: {0} - {1}", category, message);
        }

        /// <summary>
        /// Logs the message as a file/attachment with a file name/identifier with debug level
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="content">The content to be stored</param>
        /// <param name="identifier">The file name of the content. This can be a filename, a key, a identifier. Depends upon each implementation</param>
        public void Debug(string category, string content, string identifier)
        {
            System.Diagnostics.Debug.WriteLine("Error: {0} - {1}", category, identifier);
            System.Diagnostics.Debug.WriteLine(content);
        }

        /// <summary>
        /// Logs the message as a file/attachment with a file name/identifier with debug level using a custom serializer or default.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <typeparam name="T">any class that can be serialized to the <paramref name="customFormat"/> serializer format</typeparam>
        /// <param name="content">The object to be serialized</param>
        /// <param name="identifier">The filename/attachment identifier (file name or key)</param>
        /// <param name="customFormat">(Optional) the custom serializer format</param>
        public void Debug<T>(string category, T content, string identifier, SerializerFormat customFormat) where T : class, new()
        {
            System.Diagnostics.Debug.WriteLine("Error: {0} - {1}", category, identifier);
            System.Diagnostics.Debug.WriteLine((string)content.GetCustomSerializer(customFormat));
        }

        #endregion

        /// <summary>
        /// Writes the exception details.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="category">The category.</param>
        /// <param name="exception">The exception.</param>
        private static void WriteExceptionDetails(string level, string category, Exception exception)
        {
            do
            {
                System.Diagnostics.Debug.WriteLine("{0}: {1} - {2}", level, category, exception.Message);
                System.Diagnostics.Debug.WriteLine("{0}: {1} - {2}", level, category, exception.GetType().FullName);
                System.Diagnostics.Debug.WriteLine("{0}: {1} - {2}", level, category, exception.StackTrace);

                exception = exception.InnerException;
            } while (exception != null);
        }
    }
}
