using System;
using CrispyWaffle.EventLog.Log.Adapters;
using CrispyWaffle.Log.Providers;
using CrispyWaffle.Serialization;
using LogLevel = CrispyWaffle.Log.LogLevel;

namespace CrispyWaffle.EventLog.Log.Providers;

/// <summary>
/// A sealed class that provides logging functionality to the Windows Event Log.
/// This class cannot be inherited.
/// </summary>
public sealed class EventLogProvider : ILogProvider
{
    /// <summary>
    /// The event log adapter used for interacting with the event log system.
    /// </summary>
    private readonly EventLogAdapter _adapter;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventLogProvider"/> class
    /// with a default source name of "crispy-waffle".
    /// </summary>
    public EventLogProvider()
        : this("crispy-waffle") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventLogProvider"/> class
    /// with the specified source name.
    /// </summary>
    /// <param name="source">The source name to associate with the event log entries.</param>
    public EventLogProvider(string source) =>
        _adapter = new EventLogAdapter(
            source,
            string.Empty,
            Environment.MachineName,
            false,
            new EventIdProvider()
        );

    /// <summary>
    /// Initializes a new instance of the <see cref="EventLogProvider"/> class
    /// with the specified source name and log name.
    /// </summary>
    /// <param name="source">The source name to associate with the event log entries.</param>
    /// <param name="logName">The name of the event log (e.g., "Application", "System").</param>
    public EventLogProvider(string source, string logName) =>
        _adapter = new EventLogAdapter(
            source,
            logName,
            Environment.MachineName,
            false,
            new EventIdProvider()
        );

    /// <summary>
    /// Initializes a new instance of the <see cref="EventLogProvider"/> class
    /// with custom settings for source, log name, machine name, event source management, and event ID provider.
    /// </summary>
    /// <param name="source">The source name to associate with the event log entries.</param>
    /// <param name="logName">The name of the event log (e.g., "Application", "System").</param>
    /// <param name="machineName">The name of the machine where the log entries are written.</param>
    /// <param name="manageEventSource">Specifies whether to manage the event source.</param>
    /// <param name="eventIdProvider">The provider for generating event IDs.</param>
    public EventLogProvider(
        string source,
        string logName,
        string machineName,
        bool manageEventSource,
        IEventIdProvider eventIdProvider
    )
    {
        var provider = eventIdProvider ?? new EventIdProvider();
        _adapter = new EventLogAdapter(source, logName, machineName, manageEventSource, provider);
    }

    /// <summary>
    /// Sets the log level for the event log provider.
    /// </summary>
    /// <param name="level">The log level to set (e.g., Fatal, Error, Warning, etc.).</param>
    public void SetLevel(LogLevel level) => _adapter.SetLevel(level);

    /// <summary>
    /// Logs a fatal message to the event log.
    /// </summary>
    /// <param name="category">The category of the log entry.</param>
    /// <param name="message">The message to log.</param>
    public void Fatal(string category, string message) => _adapter.Fatal(message);

    /// <summary>
    /// Logs an error message to the event log.
    /// </summary>
    /// <param name="category">The category of the log entry.</param>
    /// <param name="message">The message to log.</param>
    public void Error(string category, string message) => _adapter.Error(message);

    /// <summary>
    /// Logs a warning message to the event log.
    /// </summary>
    /// <param name="category">The category of the log entry.</param>
    /// <param name="message">The message to log.</param>
    public void Warning(string category, string message) => _adapter.Warning(message);

    /// <summary>
    /// Logs an informational message to the event log.
    /// </summary>
    /// <param name="category">The category of the log entry.</param>
    /// <param name="message">The message to log.</param>
    public void Info(string category, string message) => _adapter.Info(message);

    /// <summary>
    /// Logs a trace message to the event log.
    /// </summary>
    /// <param name="category">The category of the log entry.</param>
    /// <param name="message">The message to log.</param>
    public void Trace(string category, string message) => _adapter.Trace(message);

    /// <summary>
    /// Logs a trace message with exception details to the event log.
    /// </summary>
    /// <param name="category">The category of the log entry.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception details to log.</param>
    public void Trace(string category, string message, Exception exception) =>
        _adapter.Trace(message, exception);

    /// <summary>
    /// Logs exception details with a trace message to the event log.
    /// </summary>
    /// <param name="category">The category of the log entry.</param>
    /// <param name="exception">The exception to log.</param>
    public void Trace(string category, Exception exception) => _adapter.Trace(exception);

    /// <summary>
    /// Logs a debug message to the event log.
    /// </summary>
    /// <param name="category">The category of the log entry.</param>
    /// <param name="message">The message to log.</param>
    public void Debug(string category, string message) => _adapter.Debug(message);

    /// <summary>
    /// Logs debug content to the event log. This method is a no-op.
    /// </summary>
    /// <param name="category">The category of the log entry.</param>
    /// <param name="content">The content to log (not used).</param>
    /// <param name="identifier">The file name to associate with the content (not used).</param>
    public void Debug(string category, string content, string identifier) =>
        _adapter.Debug(content, identifier);

    /// <summary>
    /// Logs debug content to the event log using custom serialization. This method is a no-op.
    /// </summary>
    /// <typeparam name="T">The type of the content to serialize (not used).</typeparam>
    /// <param name="category">The category of the log entry.</param>
    /// <param name="content">The content to log (not used).</param>
    /// <param name="identifier">The identifier for the content (not used).</param>
    /// <param name="customFormat">The custom serialization format to use (not used).</param>
    public void Debug<T>(
        string category,
        T content,
        string identifier,
        SerializerFormat customFormat = SerializerFormat.None
    )
        where T : class, new() => _adapter.Debug(content, identifier, customFormat);
}
