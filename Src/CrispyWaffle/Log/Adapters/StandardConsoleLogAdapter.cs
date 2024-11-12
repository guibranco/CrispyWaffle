using System;
using System.Collections.Generic;
using System.IO;
using CrispyWaffle.Extensions;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Log.Adapters;

/// <summary>
/// A concrete implementation of the <see cref="IConsoleLogAdapter"/> interface that logs messages to the console.
/// This class supports logging at various log levels and will use different console colors based on the log level.
/// It is a sealed class, meaning it cannot be inherited.
/// </summary>
/// <seealso cref="IConsoleLogAdapter" />
public sealed class StandardConsoleLogAdapter : IConsoleLogAdapter
{
    /// <summary>
    /// The default color used for log messages.
    /// </summary>
    private const ConsoleColor DefaultColor = ConsoleColor.White;

    /// <summary>
    /// The color used for debug log messages.
    /// </summary>
    private const ConsoleColor DebugColor = ConsoleColor.Gray;

    /// <summary>
    /// The color used for trace log messages.
    /// </summary>
    private const ConsoleColor TraceColor = ConsoleColor.DarkGreen;

    /// <summary>
    /// The color used for informational log messages.
    /// </summary>
    private const ConsoleColor InfoColor = ConsoleColor.Green;

    /// <summary>
    /// The color used for warning log messages.
    /// </summary>
    private const ConsoleColor WarningColor = ConsoleColor.Yellow;

    /// <summary>
    /// The color used for error log messages.
    /// </summary>
    private const ConsoleColor ErrorColor = ConsoleColor.Red;

    /// <summary>
    /// Object used to synchronize console writes to prevent race conditions.
    /// </summary>
    private static readonly object _syncRoot = new object();

    /// <summary>
    /// A dictionary mapping <see cref="LogLevel"/> values to their corresponding <see cref="ConsoleColor"/>.
    /// </summary>
    private static readonly Dictionary<LogLevel, ConsoleColor> _colorsByLevel = new Dictionary<
        LogLevel,
        ConsoleColor
    >
    {
        { LogLevel.Fatal, ErrorColor },
        { LogLevel.Error, ErrorColor },
        { LogLevel.Warning, WarningColor },
        { LogLevel.Info, InfoColor },
        { LogLevel.Trace, TraceColor },
        { LogLevel.Debug, DebugColor },
    };

    /// <summary>
    /// Indicates whether the console is available for logging.
    /// </summary>
    private readonly bool _isConsoleEnabled;

    /// <summary>
    /// The current log level that determines the logging threshold.
    /// </summary>
    private LogLevel _level;

    /// <summary>
    /// Initializes a new instance of the <see cref="StandardConsoleLogAdapter"/> class.
    /// The constructor checks if the console is available and sets the default log level to <see cref="LogLevel.Production"/>.
    /// </summary>
    public StandardConsoleLogAdapter()
    {
        _level = LogLevel.Production;

        using (var stream = Console.OpenStandardInput(1))
        {
            _isConsoleEnabled = stream != Stream.Null;
        }
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="StandardConsoleLogAdapter"/> class and suppresses finalization.
    /// </summary>
    ~StandardConsoleLogAdapter()
    {
        Dispose();
    }

    /// <summary>
    /// Writes the specified log message to the console with the appropriate color based on the log level.
    /// </summary>
    /// <param name="level">The <see cref="LogLevel"/> of the log message.</param>
    /// <param name="message">The log message to be written to the console.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="level"/> is not valid.</exception>
    private void WriteInternal(LogLevel level, string message)
    {
        if (!_level.HasFlag(level) || !_isConsoleEnabled)
        {
            return;
        }

        lock (_syncRoot)
        {
            Console.ForegroundColor = _colorsByLevel[level];
            Console.Write(@"{0:HH:mm:ss} ", DateTime.Now);
            Console.WriteLine(message);
            Console.ForegroundColor = DefaultColor;
        }
    }

    /// <summary>
    /// Writes the exception details to the console with the appropriate color based on the log level.
    /// </summary>
    /// <param name="level">The <see cref="LogLevel"/> of the log message.</param>
    /// <param name="exception">The exception to be logged.</param>
    private void WriteInternal(LogLevel level, Exception exception)
    {
        if (!_level.HasFlag(level) || !_isConsoleEnabled)
        {
            return;
        }

        lock (_syncRoot)
        {
            Console.ForegroundColor = _colorsByLevel[level];

            do
            {
                Console.Write(@"{0:HH:mm:ss} ", DateTime.Now);
                Console.WriteLine(exception.Message);

                exception = exception.InnerException;
            } while (exception != null);

            Console.ForegroundColor = DefaultColor;
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="StandardConsoleLogAdapter"/> instance.
    /// </summary>
    public void Dispose() => GC.SuppressFinalize(this);

    /// <summary>
    /// Sets the logging threshold by changing the log level for this adapter.
    /// </summary>
    /// <param name="level">The new <see cref="LogLevel"/> for this adapter.</param>
    public void SetLevel(LogLevel level)
    {
        Warning(
            $"Switching log level from {_level.GetHumanReadableValue()} to {level.GetHumanReadableValue()}"
        );
        _level = level;
    }

    /// <summary>
    /// Logs a debug-level message and optionally serializes the content to a file using the specified format.
    /// </summary>
    /// <typeparam name="T">The type of the <paramref name="content"/> object.</typeparam>
    /// <param name="content">The object to be serialized and logged.</param>
    /// <param name="identifier">The identifier for the serialized content.</param>
    /// <param name="customFormat">Optional custom serializer format.</param>
    /// <remarks>Requires <see cref="LogLevel.Debug"/> flag.</remarks>
    public void Debug<T>(
        T content,
        string identifier,
        SerializerFormat customFormat = SerializerFormat.None
    )
        where T : class
    {
        if (customFormat == SerializerFormat.None)
        {
            WriteInternal(LogLevel.Debug, (string)content.GetSerializer());
        }
        else
        {
            WriteInternal(LogLevel.Debug, (string)content.GetCustomSerializer(customFormat));
        }
    }

    /// <summary>
    /// Logs a debug-level message with the specified content saved to a file.
    /// </summary>
    /// <param name="content">The content to be logged.</param>
    /// <param name="fileName">The name of the file to which the content is saved.</param>
    /// <remarks>Requires <see cref="LogLevel.Debug"/> flag.</remarks>
    public void Debug(string content, string fileName) => WriteInternal(LogLevel.Debug, content);

    /// <summary>
    /// Logs a debug-level message.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    /// <remarks>Requires <see cref="LogLevel.Debug"/> flag.</remarks>
    public void Debug(string message) => WriteInternal(LogLevel.Debug, message);

    /// <summary>
    /// Logs exception details at the trace level.
    /// </summary>
    /// <param name="exception">The exception to be logged.</param>
    /// <remarks>Requires <see cref="LogLevel.Trace"/> flag.</remarks>
    public void Trace(Exception exception) => WriteInternal(LogLevel.Trace, exception);

    /// <summary>
    /// Logs a message and its associated exception details at the trace level.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    /// <param name="exception">The exception to be logged.</param>
    /// <remarks>Requires <see cref="LogLevel.Trace"/> flag.</remarks>
    public void Trace(string message, Exception exception)
    {
        WriteInternal(LogLevel.Trace, message);
        WriteInternal(LogLevel.Trace, exception);
    }

    /// <summary>
    /// Logs a trace-level message.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    /// <remarks>Requires <see cref="LogLevel.Trace"/> flag.</remarks>
    public void Trace(string message) => WriteInternal(LogLevel.Trace, message);

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    /// <remarks>Requires <see cref="LogLevel.Info"/> flag.</remarks>
    public void Info(string message) => WriteInternal(LogLevel.Info, message);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    /// <remarks>Requires <see cref="LogLevel.Warning"/> flag.</remarks>
    public void Warning(string message) => WriteInternal(LogLevel.Warning, message);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    /// <remarks>Requires <see cref="LogLevel.Error"/> flag.</remarks>
    public void Error(string message) => WriteInternal(LogLevel.Error, message);

    /// <summary>
    /// Logs a fatal error message.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    /// <remarks>Requires <see cref="LogLevel.Fatal"/> flag.</remarks>
    public void Fatal(string message) => WriteInternal(LogLevel.Fatal, message);
}
