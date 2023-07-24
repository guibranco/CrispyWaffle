using System.Diagnostics.CodeAnalysis;

namespace CrispyWaffle.Log.Providers
{
    using Adapters;
    using Serialization;
    using System;

    /// <summary>
    /// Class EventLogProvider. This class cannot be inherited.
    /// Implements the <see cref="CrispyWaffle.Log.Providers.ILogProvider" />
    /// </summary>
    /// <seealso cref="CrispyWaffle.Log.Providers.ILogProvider" />
    public sealed class EventLogProvider : ILogProvider
    {
        /// <summary>
        /// Event log provider.
        /// </summary>
        private readonly EventLogAdapter _adapter;

        #region ~Ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogProvider"/> class.
        /// </summary>
        public EventLogProvider()
            : this("crispy-waffle") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogProvider"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public EventLogProvider(string source)
        {
            _adapter = new EventLogAdapter(
                source,
                string.Empty,
                Environment.MachineName,
                false,
                new EventIdProvider()
            );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogProvider"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="logName">Name of the log.</param>
        public EventLogProvider(string source, string logName)
        {
            _adapter = new EventLogAdapter(
                source,
                logName,
                Environment.MachineName,
                false,
                new EventIdProvider()
            );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogProvider"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="logName">Name of the log.</param>
        /// <param name="machineName">Name of the machine.</param>
        /// <param name="manageEventSource">if set to <c>true</c> [manage event source].</param>
        /// <param name="eventIdProvider">The event identifier provider.</param>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "TooManyDependencies")]
        public EventLogProvider(
            string source,
            string logName,
            string machineName,
            bool manageEventSource,
            IEventIdProvider eventIdProvider
        )
        {
            var provider = eventIdProvider ?? new EventIdProvider();
            _adapter = new EventLogAdapter(
                source,
                logName,
                machineName,
                manageEventSource,
                provider
            );
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
        /// Logs the message with fatal level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        public void Fatal(string category, string message)
        {
            _adapter.Fatal(message);
        }

        /// <summary>
        /// Logs the message with error level
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged</param>
        public void Error(string category, string message)
        {
            _adapter.Error(message);
        }

        /// <summary>
        /// Logs the message with warning level
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged</param>
        public void Warning(string category, string message)
        {
            _adapter.Warning(message);
        }

        /// <summary>
        /// Logs the message with info level
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged</param>
        public void Info(string category, string message)
        {
            _adapter.Info(message);
        }

        /// <summary>
        /// Logs the message with trace level
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged</param>
        public void Trace(string category, string message)
        {
            _adapter.Trace(message);
        }

        /// <summary>
        /// Logs the message with trace level and shows exception details.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="exception">The exception.</param>
        public void Trace(string category, string message, Exception exception)
        {
            _adapter.Trace(message, exception);
        }

        /// <summary>
        /// Logs the exception details with trace level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="exception">The exception.</param>
        public void Trace(string category, Exception exception)
        {
            _adapter.Trace(exception);
        }

        /// <summary>
        /// Logs the message with debug level
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged</param>
        public void Debug(string category, string message)
        {
            _adapter.Debug(message);
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="content">Not used</param>
        /// <param name="fileName">Not used</param>
        public void Debug(string category, string content, string fileName)
        {
            _adapter.Debug(content, fileName);
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        /// <typeparam name="T">Not used</typeparam>
        /// <param name="category">The category</param>
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
