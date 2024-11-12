using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using CrispyWaffle.Extensions;
using CrispyWaffle.Infrastructure;
using CrispyWaffle.Log;
using CrispyWaffle.Log.Providers;
using CrispyWaffle.RabbitMQ.Utils.Communications;
using CrispyWaffle.Serialization;
using RabbitMQ.Client; // Updated to version 7.0.0

namespace CrispyWaffle.RabbitMQ.Log
{
    /// <summary>
    /// Class RabbitMQLogProvider.
    /// Implements the <see cref="ILogProvider" />
    /// Implements the <see cref="IDisposable" />.
    /// </summary>
    /// <seealso cref="ILogProvider" />
    /// <seealso cref="IDisposable" />
    public class RabbitMQLogProvider : ILogProvider, IDisposable
    {
        /// <summary>
        /// The level.
        /// </summary>
        private LogLevel _level;

        /// <summary>
        /// The connector.
        /// </summary>
        private readonly RabbitMQConnector _connector;

        /// <summary>
        /// The channel.
        /// </summary>
        private readonly IModel _channel;

        /// <summary>
        /// The cancellation token.
        /// </summary>
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// The queue.
        /// </summary>
        private readonly ConcurrentQueue<string> _queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMQLogProvider" /> class.
        /// </summary>
        /// <param name="connector">The connector.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentNullException">connector.</exception>
        public RabbitMQLogProvider(RabbitMQConnector connector, CancellationToken cancellationToken)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _channel = _connector.ConnectionFactory.CreateConnection().CreateModel();
            _channel.ExchangeDeclare(_connector.DefaultExchangeName, ExchangeType.Fanout, true);
            _cancellationToken = cancellationToken;
            _queue = new ConcurrentQueue<string>();
            var thread = new Thread(Worker);
            thread.Start();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="RabbitMQLogProvider" /> class.
        /// </summary>
        ~RabbitMQLogProvider()
        {
            Dispose(false);
        }

        /// <summary>
        /// Workers this instance.
        /// </summary>
        private void Worker()
        {
            Thread.CurrentThread.Name = "Message queue RabbitMQ log provider worker";
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
        /// Propagates the message internal.
        /// </summary>
        /// <param name="message">The message.</param>
        private void PropagateMessageInternal(string message)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(message);
                _channel.BasicPublish(_connector.DefaultExchangeName, string.Empty, null, body);
            }
            catch (Exception e)
            {
                LogConsumer.Trace(e);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _channel.Close();
        }

        /// <summary>
        /// Serializes a log message into a string format.
        /// </summary>
        /// <param name="level">The log level of the message (e.g., Info, Warning, Error).</param>
        /// <param name="category">The category under which the log message falls.</param>
        /// <param name="message">The actual log message to be serialized.</param>
        /// <param name="identifier">An optional identifier for the log message.</param>
        /// <returns>A serialized string representation of the log message.</returns>
        /// <remarks>
        /// This method creates a new instance of the <see cref="LogMessage"/> class, populating its properties with relevant information such as
        /// application name, category, current date and time, hostname, unique identifier, IP addresses, log level, and other contextual data.
        /// The method then calls the <see cref="GetSerializer"/> method on the <see cref="LogMessage"/> instance to obtain the serialized string.
        /// This serialized string can be used for logging purposes or for sending log data to external systems.
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
                    Application = EnvironmentHelper.ApplicationName,
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
        /// Propagates the internal.
        /// </summary>
        /// <param name="message">The message.</param>
        private void PropagateInternal(string message)
        {
            _queue.Enqueue(message);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sets the level.
        /// </summary>
        /// <param name="level">The level.</param>
        public void SetLevel(LogLevel level)
        {
            _level = level;
        }

        /// <summary>
        /// Fatal the specified category.
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
        /// Errors the specified category.
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
        /// Warnings the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        public void Warning(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Warning))
            {
                return;
            }

            PropagateInternal(Serialize(LogLevel.Warning, category, message));
        }

        /// <summary>
        /// Information the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        public void Info(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Info))
            {
                return;
            }

            PropagateInternal(Serialize(LogLevel.Info, category, message));
        }

        /// <summary>
        /// Traces the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
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
        /// Debugs the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        public void Debug(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Debug))
            {
                return;
            }

            PropagateInternal(Serialize(LogLevel.Debug, category, message));
        }

        /// <summary>
        /// Debugs the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="content">The content.</param>
        /// <param name="identifier">The identifier.</param>
        public void Debug(string category, string content, string identifier)
        {
            if (!_level.HasFlag(LogLevel.Debug))
            {
                return;
            }

            PropagateInternal(Serialize(LogLevel.Debug, category, content, identifier));
        }

        /// <summary>
        /// Debugs the specified category.
        /// </summary>
        /// <typeparam name="T">any class that can be serialized to the <paramref name="customFormat" /> serializer format.</typeparam>
        /// <param name="category">The category.</param>
        /// <param name="content">The content.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="customFormat">The custom format.</param>
        public void Debug<T>(
            string category,
            T content,
            string identifier,
            SerializerFormat customFormat
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
}
