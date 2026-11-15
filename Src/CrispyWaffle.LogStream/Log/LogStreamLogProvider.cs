using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CrispyWaffle.Extensions;
using CrispyWaffle.Infrastructure;
using CrispyWaffle.Log;
using CrispyWaffle.Log.Providers;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.LogStream.Log;

/// <summary>
/// Log provider that ships CrispyWaffle log messages to a LogStream server.
/// </summary>
/// <remarks>
/// Log calls only build an entry and queue it; a background task sends the queue to
/// <c>POST /api/logs</c> in batches (see <see cref="LogStreamOptions"/>). Like the other providers,
/// it starts with no level enabled, so call <see cref="SetLevel"/> after registering it.
/// Dispose the provider on shutdown so queued entries are sent.
/// </remarks>
/// <seealso cref="ILogProvider" />
public sealed class LogStreamLogProvider : ILogProvider, IDisposable
{
    private const int MaxCategoryLength = 100;
    private const int MaxExceptionDepth = 10;
    private const string DefaultCategory = "general";
    private const string DebugContentTemplate = "Debug content {identifier}";

    private readonly LogStreamOptions _options;
    private readonly LogStreamSender _sender;
    private LogLevel _level;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogStreamLogProvider"/> class.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <exception cref="ArgumentNullException">connector.</exception>
    public LogStreamLogProvider(LogStreamConnector connector)
    {
        if (connector == null)
        {
            throw new ArgumentNullException(nameof(connector));
        }

        _options = connector.Options;
        _sender = new LogStreamSender(connector);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogStreamLogProvider"/> class.
    /// </summary>
    /// <param name="options">The settings.</param>
    public LogStreamLogProvider(LogStreamOptions options)
        : this(new LogStreamConnector(options)) { }

    /// <summary>
    /// Gets the number of entries waiting to be sent.
    /// </summary>
    public int PendingCount => _sender.Count;

    /// <summary>
    /// Gets the number of entries dropped because the queue was full, the server rejected them,
    /// or they could not be delivered after the configured retries.
    /// </summary>
    public long DroppedCount => _sender.Dropped;

    /// <summary>
    /// Sets the log level of the instance.
    /// </summary>
    /// <param name="level">The log level.</param>
    public void SetLevel(LogLevel level) => _level = level;

    /// <summary>
    /// Sends every queued entry now.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the queue is empty.</returns>
    public Task FlushAsync(CancellationToken cancellationToken = default) =>
        _sender.FlushAsync(cancellationToken);

    /// <summary>
    /// Sends every queued entry now, blocking the caller.
    /// </summary>
    public void Flush() => _sender.FlushAsync(CancellationToken.None).GetAwaiter().GetResult();

    /// <summary>
    /// Logs the message with fatal level (<c>critical</c> in LogStream).
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="message">The message.</param>
    public void Fatal(string category, string message) => Write(LogLevel.Fatal, category, message);

    /// <summary>
    /// Logs the message with error level.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="message">The message.</param>
    public void Error(string category, string message) => Write(LogLevel.Error, category, message);

    /// <summary>
    /// Logs the message with warning level.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="message">The message to be logged.</param>
    public void Warning(string category, string message) =>
        Write(LogLevel.Warning, category, message);

    /// <summary>
    /// Logs the message with info level.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="message">The message to be logged.</param>
    public void Info(string category, string message) => Write(LogLevel.Info, category, message);

    /// <summary>
    /// Logs the message with trace level (<see cref="LogStreamOptions.TraceLevel"/> in LogStream).
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="message">The message to be logged.</param>
    public void Trace(string category, string message) => Write(LogLevel.Trace, category, message);

    /// <summary>
    /// Logs the message and the exception as one entry with <see cref="LogStreamOptions.ExceptionLevel"/>.
    /// The exception chain goes to the entry's <c>context.exception</c>.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="message">The message to be logged.</param>
    /// <param name="exception">The exception.</param>
    public void Trace(string category, string message, Exception exception) =>
        Write(LogLevel.Trace, category, message, exception);

    /// <summary>
    /// Logs the exception as one entry with <see cref="LogStreamOptions.ExceptionLevel"/>; the message is
    /// the exception message and the exception chain goes to the entry's <c>context.exception</c>.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="exception">The exception.</param>
    public void Trace(string category, Exception exception) =>
        Write(LogLevel.Trace, category, exception?.Message, exception);

    /// <summary>
    /// Logs the message with debug level.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="message">The message to be logged.</param>
    public void Debug(string category, string message) => Write(LogLevel.Debug, category, message);

    /// <summary>
    /// Logs content with debug level. The entry's message is <c>Debug content {identifier}</c>, and
    /// <c>context.identifier</c> and <c>context.content</c> hold the identifier and the content.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="content">The content to be stored.</param>
    /// <param name="identifier">The file name of the content. This can be a filename, a key, a identifier. Depends upon each implementation.</param>
    public void Debug(string category, string content, string identifier) =>
        WriteContent(category, content, identifier);

    /// <summary>
    /// Serializes the object and logs it with debug level, as <see cref="Debug(string, string, string)"/>.
    /// </summary>
    /// <typeparam name="T">any class that can be serialized to the <paramref name="customFormat" /> serializer format.</typeparam>
    /// <param name="category">The category.</param>
    /// <param name="content">The object to be serialized.</param>
    /// <param name="identifier">The filename/attachment identifier (file name or key).</param>
    /// <param name="customFormat">(Optional) the custom serializer format.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Critical Code Smell",
        "S1006:Method overrides should not change parameter defaults",
        Justification = "Matches the other CrispyWaffle log providers."
    )]
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

        var serialized =
            customFormat == SerializerFormat.None
                ? (string)content.GetSerializer()
                : (string)content.GetCustomSerializer(customFormat);

        WriteContent(category, serialized, identifier);
    }

    /// <summary>
    /// Stops the background sender and sends the queued entries, waiting at most
    /// <see cref="LogStreamOptions.ShutdownTimeout"/>.
    /// </summary>
    public void Dispose() => _sender.Dispose();

    /// <summary>
    /// Describes an exception and its inner exceptions as nested dictionaries for the entry context.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="depth">The current nesting depth.</param>
    /// <returns>The description.</returns>
    internal static IDictionary<string, object> DescribeException(Exception exception, int depth = 0)
    {
        var details = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["type"] = exception.GetType().FullName,
            ["message"] = exception.Message,
        };

        if (exception.StackTrace != null)
        {
            details["stack_trace"] = exception.StackTrace;
        }

        if (exception.Source != null)
        {
            details["source"] = exception.Source;
        }

        if (exception is AggregateException aggregate && depth < MaxExceptionDepth)
        {
            var inner = new List<object>();
            foreach (var item in aggregate.InnerExceptions)
            {
                inner.Add(DescribeException(item, depth + 1));
            }

            details["inner_exceptions"] = inner;
        }
        else if (exception.InnerException != null && depth < MaxExceptionDepth)
        {
            details["inner_exception"] = DescribeException(exception.InnerException, depth + 1);
        }

        return details;
    }

    private static string NormalizeCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return DefaultCategory;
        }

        return category.Length > MaxCategoryLength ? category.Substring(0, MaxCategoryLength) : category;
    }

    private static string CurrentTraceId(out string spanId)
    {
        spanId = null;
        var activity = Activity.Current;
        if (activity == null)
        {
            return null;
        }

        if (activity.IdFormat == ActivityIdFormat.W3C)
        {
            spanId = activity.SpanId.ToHexString();
            return activity.TraceId.ToHexString();
        }

        return activity.RootId;
    }

    private void Write(LogLevel level, string category, string message, Exception exception = null)
    {
        if (!_level.HasFlag(level))
        {
            return;
        }

        var context = BuildContext();
        var lsLevel = LogStreamLevels.From(level, _options);

        if (exception != null)
        {
            context["exception"] = DescribeException(exception);
            lsLevel = _options.ExceptionLevel;
        }

        Enqueue(lsLevel, category, message, null, context);
    }

    private void WriteContent(string category, string content, string identifier)
    {
        if (!_level.HasFlag(LogLevel.Debug))
        {
            return;
        }

        var context = BuildContext();
        context["identifier"] = identifier;
        context["content"] = content;

        Enqueue(
            LogStreamLevels.Debug,
            category,
            "Debug content " + (identifier ?? "-"),
            DebugContentTemplate,
            context
        );
    }

    private void Enqueue(
        string level,
        string category,
        string message,
        string template,
        IDictionary<string, object> context
    )
    {
        string traceId = null;
        if (_options.UseActivityTraceId)
        {
            traceId = CurrentTraceId(out var spanId);
            if (spanId != null)
            {
                context["span_id"] = spanId;
            }
        }

        _sender.TryEnqueue(
            new LogStreamEntry
            {
                TraceId = traceId,
                Level = level,
                Category = NormalizeCategory(category),
                Message = message ?? string.Empty,
                MessageTemplate = template,
                Context = context,
                Timestamp = DateTimeOffset.UtcNow,
            }
        );
    }

    private Dictionary<string, object> BuildContext()
    {
        var context = new Dictionary<string, object>(StringComparer.Ordinal);

        foreach (var pair in _options.StaticContext)
        {
            context[pair.Key] = pair.Value;
        }

        if (!_options.IncludeEnvironment)
        {
            return context;
        }

        context["application"] = EnvironmentHelper.ApplicationName;
        context["host"] = EnvironmentHelper.Host;
        context["ip_address"] = EnvironmentHelper.IpAddress;
        context["process_id"] = EnvironmentHelper.ProcessId;
        context["thread_id"] = Environment.CurrentManagedThreadId;

        var threadName = Thread.CurrentThread.Name;
        if (!string.IsNullOrEmpty(threadName))
        {
            context["thread_name"] = threadName;
        }

        if (!string.IsNullOrEmpty(EnvironmentHelper.Operation))
        {
            context["operation"] = EnvironmentHelper.Operation;
        }

        return context;
    }
}
