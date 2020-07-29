namespace CrispyWaffle.Log.Adapters
{
    using Extensions;
    using Serialization;
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Class ConsoleLogAdapter.
    /// Redirects log to Console Window.
    /// This class cannot be inherited.
    /// </summary>
    /// <seealso cref="ILogAdapter" />
    public sealed class StandardConsoleLogAdapter : IConsoleLogAdapter
    {
        #region Consts

        /// <summary>
        /// The default color
        /// </summary>
        private const ConsoleColor DefaultColor = ConsoleColor.White;

        /// <summary>
        /// The debug color
        /// </summary>
        private const ConsoleColor DebugColor = ConsoleColor.Gray;

        /// <summary>
        /// The trace color
        /// </summary>
        private const ConsoleColor TraceColor = ConsoleColor.DarkGreen;

        /// <summary>
        /// The information color
        /// </summary>
        private const ConsoleColor InfoColor = ConsoleColor.Green;

        /// <summary>
        /// The warning color
        /// </summary>
        private const ConsoleColor WarningColor = ConsoleColor.Yellow;

        /// <summary>
        /// The error color
        /// </summary>
        private const ConsoleColor ErrorColor = ConsoleColor.Red;

        #endregion

        #region Private Members

        /// <summary>
        /// The synchronize root
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// The colors by level
        /// </summary>
        private static readonly Dictionary<LogLevel, ConsoleColor> ColorsByLevel = new Dictionary<LogLevel, ConsoleColor>
        {
            {LogLevel.FATAL, ErrorColor},
            {LogLevel.ERROR, ErrorColor},
            {LogLevel.WARNING, WarningColor},
            {LogLevel.INFO, InfoColor},
            {LogLevel.TRACE, TraceColor},
            {LogLevel.DEBUG, DebugColor}
        };

        /// <summary>
        /// The is console enabled
        /// </summary>
        private readonly bool _isConsoleEnabled;

        /// <summary>
        /// The level
        /// </summary>
        private LogLevel _level;

        #endregion

        #region ~Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardConsoleLogAdapter" /> class.
        /// </summary>
        public StandardConsoleLogAdapter()
        {
            _level = LogLevel.PRODUCTION;

            using (var stream = Console.OpenStandardInput(1))
            {
                _isConsoleEnabled = stream != Stream.Null;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="StandardConsoleLogAdapter" /> class.
        /// </summary>
        ~StandardConsoleLogAdapter()
        {
            Dispose();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Writes the internal.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="ArgumentOutOfRangeException">level - null</exception>
        private void WriteInternal(LogLevel level, string message)
        {
            if (!_level.HasFlag(level) ||
                !_isConsoleEnabled)
            {
                return;
            }

            lock (SyncRoot)
            {
                Console.ForegroundColor = ColorsByLevel[level];
                Console.Write(@"{0:HH:mm:ss} ", DateTime.Now);
                Console.WriteLine(message);
                Console.ForegroundColor = DefaultColor;
            }
        }

        /// <summary>
        /// Writes the internal.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="exception">The exception.</param>
        private void WriteInternal(LogLevel level, Exception exception)
        {
            if (!_level.HasFlag(level) ||
                !_isConsoleEnabled)
            {
                return;
            }

            lock (SyncRoot)
            {
                Console.ForegroundColor = ColorsByLevel[level];

                do
                {
                    Console.Write(@"{0:HH:mm:ss} ", DateTime.Now);
                    Console.WriteLine(exception.Message);

                    exception = exception.InnerException;

                } while (exception != null);

                Console.ForegroundColor = DefaultColor;
            }
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Implementation of ILogAdapter

        /// <summary>
        /// Change the LogLevel of Log Adapter instance.
        /// </summary>
        /// <param name="level">The new <seealso cref="LogLevel" /> level of the instance</param>

        public void SetLevel(LogLevel level)
        {
            Warning($"Switching log level from {_level.GetHumanReadableValue()} to {level.GetHumanReadableValue()}");
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

        public void Debug<T>(T content, string identifier, SerializerFormat customFormat = SerializerFormat.NONE) where T : class
        {
            if (customFormat == SerializerFormat.NONE)
            {
                WriteInternal(LogLevel.DEBUG, (string)content.GetSerializer());
            }
            else
            {
                WriteInternal(LogLevel.DEBUG, (string)content.GetCustomSerializer(customFormat));
            }
        }

        /// <summary>
        /// Save the string <paramref name="content" /> into a file with name <paramref name="filename" />
        /// </summary>
        /// <param name="content">The file content</param>
        /// <param name="filename">The file name</param>
        /// <remarks>Requires LogLevel.DEBUG flag</remarks>

        public void Debug(string content, string filename)
        {
            WriteInternal(LogLevel.DEBUG, content);
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
        /// Logs exception details as TRACE level.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <remarks>Requires LogLevel.TRACE flag.</remarks>
        public void Trace(Exception exception)
        {
            WriteInternal(LogLevel.TRACE, exception);
        }

        /// <summary>
        /// Logs a message as TRACE level with exception details.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="exception">The exception.</param>
        /// <remarks>Requires LogLevel.TRACE flag.</remarks>
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
        /// <param name="message">The message to be logged.</param>
        /// <remarks>Requires LogLevel.FATAL flag.</remarks>
        public void Fatal(string message)
        {
            WriteInternal(LogLevel.FATAL, message);
        }

        #endregion
    }
}
