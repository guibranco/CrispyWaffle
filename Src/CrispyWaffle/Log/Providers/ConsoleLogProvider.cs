using CrispyWaffle.Log.Adapters;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Log.Providers
{
    /// <summary>
    /// Console log provider
    /// </summary>
    public sealed class ConsoleLogProvider : ILogProvider
    {
        #region Private fields

        /// <summary>
        /// The synchronize lock
        /// </summary>
        private static readonly object _syncRoot = new object();

        /// <summary>
        /// The adapter
        /// </summary>
        private static IConsoleLogAdapter _adapter;

        #endregion

        #region ~Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogProvider"/> class.
        /// </summary>
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

        #endregion

        #region Implementation of ILogProvider

        /// <summary>
        /// Sets the log level of the instance
        /// </summary>
        /// <param name="level">The log level</param>
        public void SetLevel(LogLevel level)
        {
            _adapter.SetLevel(level);
        }

        /// <summary>
        /// Logs the message with fatal level
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged.</param>
        public void Fatal(string category, string message)
        {
            _adapter.Fatal(message);
        }

        /// <summary>
        /// Logs the message with error level
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged.</param>
        public void Error(string category, string message)
        {
            _adapter.Error(message);
        }

        /// <summary>
        /// Logs the message with warning level
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged.</param>
        public void Warning(string category, string message)
        {
            _adapter.Warning(message);
        }

        /// <summary>
        /// Logs the message with info level
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged.</param>
        public void Info(string category, string message)
        {
            _adapter.Info(message);
        }

        /// <summary>
        /// Logs the message with trace level
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged.</param>
        public void Trace(string category, string message)
        {
            _adapter.Trace(message);
        }

        /// <summary>
        /// Logs the message with trace level and shows exception details.
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="exception">The exception.</param>
        public void Trace(string category, string message, Exception exception)
        {
            _adapter.Trace(message, exception);
        }

        /// <summary>
        /// Logs the exception details with trace level
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="exception">The exception.</param>
        public void Trace(string category, Exception exception)
        {
            _adapter.Trace(exception);
        }

        /// <summary>
        /// Logs the message with debug level
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged.</param>
        public void Debug(string category, string message)
        {
            _adapter.Debug(message);
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="content">Not used</param>
        /// <param name="identifier">Not used</param>
        public void Debug(string category, string content, string identifier)
        {
            _adapter.Debug(content, identifier);
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        /// <param name="category">The category</param>
        /// <typeparam name="T">Not used</typeparam>
        /// <param name="content">Not used</param>
        /// <param name="identifier">Not used</param>
        /// <param name="customFormat">Not used</param>
        public void Debug<T>(
            string category,
            T content,
            string identifier,
            SerializerFormat customFormat = SerializerFormat.None
        )
            where T : class, new()
        {
            _adapter.Debug(content, identifier, customFormat);
        }

        #endregion
    }
}
