using System;
using CrispyWaffle.Log;
using CrispyWaffle.Log.Providers;
using CrispyWaffle.Serialization;
using log4net;

namespace CrispyWaffle.Log4Net
{
    /// <summary>
    /// The Log4Net log provider.
    /// </summary>
    /// <seealso cref="ILogProvider" />
    public sealed class Log4NetLogProvider : ILogProvider
    {
        /// <summary>
        /// The level.
        /// </summary>
        private LogLevel _level;

        /// <summary>
        /// The adapter.
        /// </summary>
        private readonly ILog _adapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLogProvider" /> class.
        /// </summary>
        /// <param name="adapter">The adapter.</param>
        public Log4NetLogProvider(ILog adapter)
        {
            _adapter = adapter;
        }

        /// <summary>
        /// Sets the log level of the instance.
        /// </summary>
        /// <param name="level">The log level.</param>
        public void SetLevel(LogLevel level) => _level = level;

        /// <summary>
        /// Logs the message with a fatal level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        public void Fatal(string category, string message)
        {
            if (_level.HasFlag(LogLevel.Fatal))
            {
                _adapter.Fatal(message);
            }
        }

        /// <summary>
        /// Logs the message with the error level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        public void Error(string category, string message)
        {
            if (_level.HasFlag(LogLevel.Error))
            {
                _adapter.Error(message);
            }
        }

        /// <summary>
        /// Logs the message with a warning level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        public void Warning(string category, string message)
        {
            if (_level.HasFlag(LogLevel.Warning))
            {
                _adapter.Warn(message);
            }
        }

        /// <summary>
        /// Logs the message with info level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        public void Info(string category, string message)
        {
            if (_level.HasFlag(LogLevel.Info))
            {
                _adapter.Info(message);
            }
        }

        /// <summary>
        /// Logs the message with a trace level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        public void Trace(string category, string message)
        {
            if (_level.HasFlag(LogLevel.Trace))
            {
                _adapter.Info(message);
            }
        }

        /// <summary>
        /// Traces the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public void Trace(string category, string message, Exception exception)
        {
            if (!_level.HasFlag(LogLevel.Trace))
            {
                return;
            }

            _adapter.Info(message);
            do
            {
                _adapter.Info(exception.Message);
                _adapter.Info(exception.StackTrace);

                exception = exception.InnerException;
            } while (exception != null);
        }

        /// <summary>
        /// Traces the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="exception">The exception.</param>
        public void Trace(string category, Exception exception)
        {
            if (!_level.HasFlag(LogLevel.Trace))
            {
                return;
            }

            do
            {
                _adapter.Info(exception.Message);
                _adapter.Info(exception.StackTrace);

                exception = exception.InnerException;
            } while (exception != null);
        }

        /// <summary>
        /// Logs the message with debug level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        public void Debug(string category, string message)
        {
            if (_level.HasFlag(LogLevel.Debug))
            {
                _adapter.Debug(message);
            }
        }

        /// <summary>
        /// Logs the message as a file/attachment with a file name/identifier with debug level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="content">The content to be stored.</param>
        /// <param name="identifier">The file name of the content. This can be a filename, a key, or an identifier. Depends upon each implementation.</param>
        public void Debug(string category, string content, string identifier)
        {
            if (!_level.HasFlag(LogLevel.Debug))
            {
                return;
            }

            _adapter.Debug(identifier);
            _adapter.Debug(content);
        }

        /// <summary>
        /// Logs the message as a file/attachment with a file name/identifier at a debug level using a custom serializer or default.
        /// </summary>
        /// <typeparam name="T">any class that can be serialized to the <paramref name="customFormat" /> serializer format.</typeparam>
        /// <param name="category">The category.</param>
        /// <param name="content">The object to be serialized.</param>
        /// <param name="identifier">The filename/attachment identifier (file name or key).</param>
        /// <param name="customFormat">(Optional) the custom serializer format.</param>
        public void Debug<T>(
            string category,
            T content,
            string identifier,
            SerializerFormat customFormat = SerializerFormat.None
        )
            where T : class, new()
        {
            if (!_level.HasFlag(LogLevel.Debug))
            {
                return;
            }

            _adapter.Debug(identifier);
            if (customFormat == SerializerFormat.None)
            {
                _adapter.Debug((string)content.GetSerializer());
            }
            else
            {
                _adapter.Debug((string)content.GetCustomSerializer(customFormat));
            }
        }
    }
}
