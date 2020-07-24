using CrispyWaffle.Log.Providers;
using CrispyWaffle.Serialization;
using System;
using System.Diagnostics;

namespace CrispyWaffle.Log.Adapters
{
    /// <summary>
    /// Class EventLogAdapter. This class cannot be inherited.
    /// Implements the <see cref="CrispyWaffle.Log.Adapters.ILogAdapter" />
    /// </summary>
    /// <seealso cref="CrispyWaffle.Log.Adapters.ILogAdapter" />
    public sealed class EventLogAdapter : ILogAdapter
    {
        #region Consts

        /// <summary>
        /// The application log name
        /// </summary>
        private const string ApplicationLogName = "Application";
        /// <summary>
        /// The maximum payload length chars
        /// </summary>
        const int MaximumPayloadLengthChars = 31839;

        /// <summary>
        /// The maximum source name length chars
        /// </summary>
        const int MaximumSourceNameLengthChars = 212;

        /// <summary>
        /// The source moved event identifier
        /// </summary>
        const int SourceMovedEventId = 3;

        /// <summary>
        /// The event identifier provider
        /// </summary>
        readonly IEventIdProvider _eventIdProvider;

        /// <summary>
        /// The log
        /// </summary>
        readonly EventLog _log;

        /// <summary>
        /// The level
        /// </summary>
        private LogLevel _level;

        #endregion

        #region ~Ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogAdapter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="logName">Name of the log.</param>
        /// <param name="machineName">Name of the machine.</param>
        /// <param name="manageEventSource">if set to <c>true</c> [manage event source].</param>
        /// <param name="eventIdProvider">The event identifier provider.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        /// <exception cref="ArgumentNullException">eventIdProvider</exception>
        public EventLogAdapter(string source, string logName, string machineName, bool manageEventSource,
            IEventIdProvider eventIdProvider)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            if (source.Length > MaximumSourceNameLengthChars)
            {
                source = source.Substring(0, MaximumSourceNameLengthChars);
            }

            source = source.Replace("<", "_");
            source = source.Replace(">", "_");

            _eventIdProvider = eventIdProvider ?? throw new ArgumentNullException(nameof(eventIdProvider));

            _log = new EventLog(string.IsNullOrWhiteSpace(logName) ? ApplicationLogName : logName, machineName);

            if (manageEventSource)
            {
                ConfigureSource(_log, source);
            }
            else
            {
                _log.Source = source;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Configures the source.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="source">The source.</param>
        private static void ConfigureSource(EventLog log, string source)
        {
            var sourceData = new EventSourceCreationData(source, log.Log) { MachineName = log.MachineName };

            string oldLogName = null;

            if (EventLog.SourceExists(source, log.MachineName))
            {
                var existingLogWithSourceName = EventLog.LogNameFromSourceName(source, log.MachineName);

                if (!string.IsNullOrWhiteSpace(existingLogWithSourceName) &&
                    !log.Log.Equals(existingLogWithSourceName, StringComparison.OrdinalIgnoreCase))
                {
                    // Remove the source from the previous log so we can associate it with the current log name
                    EventLog.DeleteEventSource(source, log.MachineName);
                    oldLogName = existingLogWithSourceName;
                }
            }
            else
            {
                EventLog.CreateEventSource(sourceData);
            }

            NotifyLogSourceChange(log, source, oldLogName);

            log.Source = source;
        }

        /// <summary>
        /// Notifies the log source change.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="source">The source.</param>
        /// <param name="oldLogName">Old name of the log.</param>
        private static void NotifyLogSourceChange(EventLog log, string source, string oldLogName)
        {
            if (oldLogName != null)
            {
                var metaSource = $"serilog-{log.Log}";
                if (!EventLog.SourceExists(metaSource, log.MachineName))
                    EventLog.CreateEventSource(new EventSourceCreationData(metaSource, log.Log)
                    {
                        MachineName = log.MachineName
                    });

                log.Source = metaSource;
                log.WriteEntry(
                    $"Event source {source} was previously registered in log {oldLogName}. " +
                    $"The source has been registered with this log, {log.Log}, however a computer restart may be required " +
                    $"before event logs will appear in {log.Log} with source {source}. Until then, messages may be logged to {oldLogName}.",
                    EventLogEntryType.Warning,
                    SourceMovedEventId);
            }
        }


        /// <summary>
        /// Levels the type of to event log entry.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>EventLogEntryType.</returns>
        private static EventLogEntryType LevelToEventLogEntryType(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.DEBUG:
                case LogLevel.TRACE:
                    return EventLogEntryType.Information;

                case LogLevel.WARNING:
                    return EventLogEntryType.Warning;

                case LogLevel.ERROR:
                case LogLevel.FATAL:
                    return EventLogEntryType.Error;

                default:
                    return EventLogEntryType.Information;
            }
        }

        /// <summary>
        /// Writes the internal.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="message">The message.</param>
        private void WriteInternal(LogLevel level, string message)
        {
            if (!_level.HasFlag(level))
                return;

            var type = LevelToEventLogEntryType(level);

            if (message.Length > MaximumPayloadLengthChars)
            {
                message = message.Substring(0, MaximumPayloadLengthChars);
            }

            _log.WriteEntry(message, type, _eventIdProvider.ComputeEventId(message));
        }

        /// <summary>
        /// Writes the internal.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="exception">The exception.</param>
        private void WriteInternal(LogLevel level, Exception exception)
        {

            if (!_level.HasFlag(level))
                return;

            var type = LevelToEventLogEntryType(level);

            var message = GetMessageFromException(exception);

            if (message.Length > MaximumPayloadLengthChars)
            {
                message = message.Substring(0, MaximumPayloadLengthChars);
            }

            _log.WriteEntry(message, type, _eventIdProvider.ComputeEventId(message));

        }

        /// <summary>
        /// Gets the message from exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>System.String.</returns>
        private static string GetMessageFromException(Exception exception)
        {
            var message = string.Empty;

            do
            {
                message += exception.Message;
                message += "\r\n";
                message += exception.StackTrace;
                message += "\r\n";

                exception = exception.InnerException;
            } while (exception != null);

            return message;
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        { }

        #endregion

        #region Implementation of ILogAdapter

        /// <summary>
        /// Change the LogLevel of Log Adapter instance.
        /// </summary>
        /// <param name="level">The new <seealso cref="LogLevel" /> level of the instance</param>
        public void SetLevel(LogLevel level)
        {
            _level = level;
        }

        /// <summary>
        /// Save the serializer version of <paramref name="content" /> in the file <paramref name="identifier" />,
        /// using default SerializerFormat, or a custom serializer format provided by <paramref name="customFormat" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter <paramref name="content" /></typeparam>
        /// <param name="content">The object/instance of a class to be serialized and saved in a disk file</param>
        /// <param name="identifier">The file name to be persisted to disk with the content</param>
        /// <param name="customFormat">Whatever or not to use a custom Serializer adapter different that one that is default for type</param>
        /// <remarks>Requires LogLevel.DEBUG flag</remarks>
        public void Debug<T>(T content, string identifier, SerializerFormat customFormat = SerializerFormat.NONE)
            where T : class
        {
            if (!_level.HasFlag(LogLevel.DEBUG))
                return;

            var contentAsString = customFormat == SerializerFormat.NONE
                ? content.GetSerializer()
                : content.GetCustomSerializer(customFormat);

            Debug((string)contentAsString, identifier);
        }

        /// <summary>
        /// Save the string <paramref name="content" /> into a file with name <paramref name="fileName" />
        /// </summary>
        /// <param name="content">The file content</param>
        /// <param name="fileName">The file name</param>
        /// <remarks>Requires LogLevel.DEBUG flag</remarks>
        public void Debug(string content, string fileName)
        {
            WriteInternal(LogLevel.DEBUG, $"{fileName}: {content}");
        }

        /// <summary>
        /// Logs a message as DEBUG level
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <remarks>Requires LogLevel.DEBUG flag.</remarks>
        public void Debug(string message)
        {
            WriteInternal(LogLevel.DEBUG, message);
        }

        /// <summary>
        /// Logs the exception with trace level.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void Trace(Exception exception)
        {
            WriteInternal(LogLevel.TRACE, exception);
        }

        /// <summary>
        /// Logs the message with trace level and shows exception details.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="exception">The exception.</param>
        public void Trace(string message, Exception exception)
        {
            WriteInternal(LogLevel.TRACE, message);
            WriteInternal(LogLevel.TRACE, exception);
        }

        /// <summary>
        /// Logs a message as TRACE level
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <remarks>Requires LogLevel.TRACE flag.</remarks>
        public void Trace(string message)
        {
            WriteInternal(LogLevel.TRACE, message);
        }

        /// <summary>
        /// Logs a message as INFO level
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <remarks>Requires LogLevel.INFO flag.</remarks>
        public void Info(string message)
        {
            WriteInternal(LogLevel.INFO, message);
        }

        /// <summary>
        /// Logs a message as WARNING level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <remarks>Requires LogLevel.WARNING flag.</remarks>
        public void Warning(string message)
        {
            WriteInternal(LogLevel.WARNING, message);
        }

        /// <summary>
        /// Logs a message as ERROR level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <remarks>Requires LogLevel.ERROR flag.</remarks>
        public void Error(string message)
        {
            WriteInternal(LogLevel.ERROR, message);
        }

        /// <summary>
        /// Logs a message as FATAL level.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Fatal(string message)
        {
            WriteInternal(LogLevel.FATAL, message);
        }

        #endregion
    }
}
