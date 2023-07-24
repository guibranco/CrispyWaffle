namespace CrispyWaffle.Redis.Log
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using Extensions;
    using Infrastructure;
    using CrispyWaffle.Log;
    using CrispyWaffle.Log.Providers;
    using PropagationStrategy;
    using Utils.Communications;
    using Serialization;

    public class PubSubRedisLogProvider : ILogProvider
    {
        #region Private fields

        /// <summary>
        /// The Redis connector
        /// </summary>
        private readonly RedisConnector _redis;

        /// <summary>
        /// The propagation strategy
        /// </summary>
        private readonly IPropagationStrategy _propagationStrategy;

        /// <summary>
        /// The level
        /// </summary>
        private LogLevel _level;

        /// <summary>
        /// The cancellation token
        /// </summary>
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// The queue
        /// </summary>
        private readonly ConcurrentQueue<string> _queue;

        #endregion

        #region ~Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="PubSubRedisLogProvider" /> class.
        /// </summary>
        /// <param name="redis">The redis.</param>
        /// <param name="propagationStrategy">The propagation strategy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
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
            var thread = new Thread(Worker);
            thread.Start();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Workers this instance.
        /// </summary>
        private void Worker()
        {
            Thread.CurrentThread.Name = "Message queue Redis log provider worker";
            Thread.Sleep(1000);

            while (true)
            {
                while (_queue.Count > 0)
                {
                    if (!_queue.TryDequeue(out var message))
                    {
                        break;
                    }

                    PropagateMessageInternal(message);
                }

                if (_cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Serializes the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        /// <param name="identifier">The identifier.</param>
        /// <returns>System.String.</returns>
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
                    ThreadId = Thread.CurrentThread.ManagedThreadId,
                    ThreadName = Thread.CurrentThread.Name
                }.GetSerializer();
        }

        /// <summary>
        /// Propagates the message internal.
        /// </summary>
        /// <param name="message">The message.</param>
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
        /// Propagates the internal.
        /// </summary>
        /// <param name="message">The message.</param>
        private void PropagateInternal(string message)
        {
            _queue.Enqueue(message);
        }

        #endregion

        #region Implementation of ILogProvider

        /// <summary>
        /// Sets the level.
        /// </summary>
        /// <param name="level">The level.</param>
        public void SetLevel(LogLevel level)
        {
            _level = level;
        }

        /// <summary>
        /// Logs the message with fatal level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        public void Fatal(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Fatal))
            {
                return;
            }

            PropagateInternal(Serialize(LogLevel.Fatal, category, message));
        }

        /// <summary>
        /// Logs the message with error level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        public void Error(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Error))
            {
                return;
            }

            PropagateInternal(Serialize(LogLevel.Error, category, message));
        }

        /// <summary>
        /// Logs the message with warning level.
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged</param>
        public void Warning(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Warning))
            {
                return;
            }

            PropagateInternal(Serialize(LogLevel.Warning, category, message));
        }

        /// <summary>
        /// Logs the message with info level.
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged</param>
        public void Info(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Info))
            {
                return;
            }

            PropagateInternal(Serialize(LogLevel.Info, category, message));
        }

        /// <summary>
        /// Logs the message with trace level.
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged</param>
        public void Trace(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Trace))
            {
                return;
            }

            PropagateInternal(Serialize(LogLevel.Trace, category, message));
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

            PropagateInternal(Serialize(LogLevel.Trace, category, message));

            Trace(category, exception);
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
                PropagateInternal(Serialize(LogLevel.Trace, category, exception.Message));
                PropagateInternal(Serialize(LogLevel.Trace, category, exception.StackTrace));

                exception = exception.InnerException;
            } while (exception != null);
        }

        /// <summary>
        /// Logs the message with debug level.
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged</param>
        public void Debug(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Debug))
            {
                return;
            }

            PropagateInternal(Serialize(LogLevel.Debug, category, message));
        }

        /// <summary>
        /// Logs the message as a file/attachment with a file name/identifier with debug level
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="content">The content to be stored</param>
        /// <param name="identifier">The file name of the content. This can be a filename, a key, a identifier. Depends upon each implementation</param>
        public void Debug(string category, string content, string identifier)
        {
            if (!_level.HasFlag(LogLevel.Debug))
            {
                return;
            }

            PropagateInternal(Serialize(LogLevel.Debug, category, content, identifier));
        }

        /// <summary>
        /// Logs the message as a file/attachment with a file name/identifier with debug level using a custom serializer or default.
        /// </summary>
        /// <typeparam name="T">any class that can be serialized to the <paramref name="customFormat" /> serializer format</typeparam>
        /// <param name="category">The category</param>
        /// <param name="content">The object to be serialized</param>
        /// <param name="identifier">The filename/attachment identifier (file name or key)</param>
        /// <param name="customFormat">(Optional) the custom serializer format</param>
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

        #endregion
    }
}
