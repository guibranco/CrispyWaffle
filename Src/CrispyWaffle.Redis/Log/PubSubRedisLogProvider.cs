using System;
using System.Collections.Concurrent;
using System.Threading;
using CrispyWaffle.Extensions;
using CrispyWaffle.Infrastructure;
using CrispyWaffle.Log;
using CrispyWaffle.Log.Providers;
using CrispyWaffle.Redis.Log.PropagationStrategy;
using CrispyWaffle.Redis.Utils.Communications;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Redis.Log;

/// <summary>
/// A log provider that sends log messages to a Redis Pub/Sub system for propagation.
/// </summary>
public class PubSubRedisLogProvider : ILogProvider
{
    /// <summary>
    /// The Redis connector responsible for handling Redis operations.
    /// </summary>
    private readonly RedisConnector _redis;

    /// <summary>
    /// The strategy used for propagating log messages.
    /// </summary>
    private readonly IPropagationStrategy _propagationStrategy;

    /// <summary>
    /// The current logging level, determining which log messages are propagated.
    /// </summary>
    private LogLevel _level;

    /// <summary>
    /// The cancellation token used to stop the background logging worker.
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// A queue that holds log messages to be processed by the background worker.
    /// </summary>
    private readonly ConcurrentQueue<string> _queue;

    /// <summary>
    /// Initializes a new instance of the <see cref="PubSubRedisLogProvider"/> class.
    /// </summary>
    /// <param name="redis">The Redis connector instance used for interacting with Redis.</param>
    /// <param name="propagationStrategy">The strategy used for propagating messages to Redis.</param>
    /// <param name="cancellationToken">The cancellation token used to cancel the background worker.</param>
    public PubSubRedisLogProvider(
        RedisConnector redis,
        IPropagationStrategy propagationStrategy,
        CancellationToken cancellationToken
    )
    {
        _redis = redis;
        _propagationStrategy = propagationStrategy;
        _cancellationToken = cancellationToken;
        _queue = new ConcurrentQueue<string>();

        // Start a background worker to process the queue
        var thread = new Thread(Worker);
        thread.Start();
    }

    /// <summary>
    /// Background worker that processes log messages from the queue and propagates them.
    /// </summary>
    private void Worker()
    {
        Thread.CurrentThread.Name = "Message queue Redis log provider worker";
        Thread.Sleep(1000);

        while (true)
        {
            // Process messages in the queue
            while (_queue.Count > 0)
            {
                if (!_queue.TryDequeue(out var message))
                {
                    break;
                }

                PropagateMessageInternal(message);
            }

            // Exit loop if cancellation is requested
            if (_cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Serializes a log message into a string format suitable for logging and propagation.
    /// </summary>
    /// <param name="level">The log level indicating the severity of the message.</param>
    /// <param name="category">The category of the log message.</param>
    /// <param name="message">The log message content.</param>
    /// <param name="identifier">An optional identifier for the log message.</param>
    /// <returns>A serialized string representation of the log message.</returns>
    /// <remarks>
    /// This method creates a <see cref="LogMessage"/> instance and serializes it into a string representation.
    /// The serialized message includes contextual information such as the date, hostname, process ID,
    /// and other relevant data.
    /// </remarks>
    private static string Serialize(
        LogLevel level,
        string category,
        string message,
        string identifier = null
    )
    {
        return (string)
            new LogMessage
            {
                Category = category,
                Date = DateTime.Now,
                Hostname = EnvironmentHelper.Host,
                Id = Guid.NewGuid().ToString(),
                IpAddress = EnvironmentHelper.IpAddress,
                IpAddressRemote = EnvironmentHelper.IpAddressExternal,
                Level = level.GetHumanReadableValue(),
                Message = message,
                MessageIdentifier = identifier,
                Operation = EnvironmentHelper.Operation,
                ProcessId = EnvironmentHelper.ProcessId,
                UserAgent = EnvironmentHelper.UserAgent,
                ThreadId = Environment.CurrentManagedThreadId,
                ThreadName = Thread.CurrentThread.Name,
            }.GetSerializer();
    }

    /// <summary>
    /// Propagates a log message to Redis using the specified propagation strategy.
    /// </summary>
    /// <param name="message">The serialized log message to be propagated.</param>
    private void PropagateMessageInternal(string message)
    {
        try
        {
            _propagationStrategy.Propagate(message, _redis.QueuePrefix, _redis.Subscriber);
        }
        catch (Exception e)
        {
            LogConsumer.Debug("Message: {0} | Stack Trace: {1}", e.Message, e.StackTrace);
        }
    }

    /// <summary>
    /// Enqueues a log message for processing by the background worker.
    /// </summary>
    /// <param name="message">The serialized log message to be processed.</param>
    private void PropagateInternal(string message) => _queue.Enqueue(message);

    /// <summary>
    /// Sets the log level for the provider, determining which messages are processed.
    /// </summary>
    /// <param name="level">The log level to set.</param>
    public void SetLevel(LogLevel level) => _level = level;

    // The following methods represent the logging functionality at various severity levels.
    // They serialize the log message and add it to the internal queue for propagation.

    /// <summary>
    /// Logs a fatal message.
    /// </summary>
    /// <param name="category">The category of the log message.</param>
    /// <param name="message">The log message content.</param>
    public void Fatal(string category, string message)
    {
        if (!_level.HasFlag(LogLevel.Fatal))
        {
            return;
        }

        PropagateInternal(Serialize(LogLevel.Fatal, category, message));
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="category">The category of the log message.</param>
    /// <param name="message">The log message content.</param>
    public void Error(string category, string message)
    {
        if (!_level.HasFlag(LogLevel.Error))
        {
            return;
        }

        PropagateInternal(Serialize(LogLevel.Error, category, message));
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="category">The category of the log message.</param>
    /// <param name="message">The log message content.</param>
    public void Warning(string category, string message)
    {
        if (!_level.HasFlag(LogLevel.Warning))
        {
            return;
        }

        PropagateInternal(Serialize(LogLevel.Warning, category, message));
    }

    /// <summary>
    /// Logs an info message.
    /// </summary>
    /// <param name="category">The category of the log message.</param>
    /// <param name="message">The log message content.</param>
    public void Info(string category, string message)
    {
        if (!_level.HasFlag(LogLevel.Info))
        {
            return;
        }

        PropagateInternal(Serialize(LogLevel.Info, category, message));
    }

    /// <summary>
    /// Logs a trace message.
    /// </summary>
    /// <param name="category">The category of the log message.</param>
    /// <param name="message">The log message content.</param>
    public void Trace(string category, string message)
    {
        if (!_level.HasFlag(LogLevel.Trace))
        {
            return;
        }

        PropagateInternal(Serialize(LogLevel.Trace, category, message));
    }

    /// <summary>
    /// Logs a trace message, including exception details.
    /// </summary>
    /// <param name="category">The category of the log message.</param>
    /// <param name="message">The log message content.</param>
    /// <param name="exception">The exception associated with the trace.</param>
    public void Trace(string category, string message, Exception exception)
    {
        if (!_level.HasFlag(LogLevel.Trace))
        {
            return;
        }

        PropagateInternal(Serialize(LogLevel.Trace, category, message));
        Trace(category, exception);
    }

    /// <summary>
    /// Logs an exception trace.
    /// </summary>
    /// <param name="category">The category of the log message.</param>
    /// <param name="exception">The exception to log.</param>
    public void Trace(string category, Exception exception)
    {
        if (!_level.HasFlag(LogLevel.Trace))
        {
            return;
        }

        do
        {
            PropagateInternal(Serialize(LogLevel.Trace, category, exception.Message));
            PropagateInternal(Serialize(LogLevel.Trace, category, exception.StackTrace));

            exception = exception.InnerException;
        } while (exception != null);
    }

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="category">The category of the log message.</param>
    /// <param name="message">The log message content.</param>
    public void Debug(string category, string message)
    {
        if (!_level.HasFlag(LogLevel.Debug))
        {
            return;
        }

        PropagateInternal(Serialize(LogLevel.Debug, category, message));
    }

    /// <summary>
    /// Logs debug content as a file/attachment, using a custom identifier.
    /// </summary>
    /// <param name="category">The category of the log message.</param>
    /// <param name="content">The content to be stored.</param>
    /// <param name="identifier">The file/attachment identifier (e.g., filename, key).</param>
    public void Debug(string category, string content, string identifier)
    {
        if (!_level.HasFlag(LogLevel.Debug))
        {
            return;
        }

        PropagateInternal(Serialize(LogLevel.Debug, category, content, identifier));
    }

    /// <summary>
    /// Logs debug content using a custom serializer or default serialization.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="category">The category of the log message.</param>
    /// <param name="content">The object to be serialized.</param>
    /// <param name="identifier">The file/attachment identifier.</param>
    /// <param name="customFormat">The custom serializer format to use.</param>
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

        string serialized;
        if (customFormat == SerializerFormat.None)
        {
            serialized = (string)content.GetSerializer();
        }
        else
        {
            serialized = (string)content.GetCustomSerializer(customFormat);
        }

        PropagateInternal(Serialize(LogLevel.Debug, category, serialized, identifier));
    }
}
