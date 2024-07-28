using System;
using System.Threading;
using System.Threading.Tasks;
using CrispyWaffle.ElasticSearch.Utils.Communications;
using CrispyWaffle.Extensions;
using CrispyWaffle.Infrastructure;
using CrispyWaffle.Log;
using CrispyWaffle.Log.Providers;
using CrispyWaffle.Serialization;
using Elastic.Clients.Elasticsearch;
using LogLevel = CrispyWaffle.Log.LogLevel;

namespace CrispyWaffle.ElasticSearch.Log
{
    /// <summary>
    /// The Elasticsearch log provider class.
    /// </summary>
    /// <seealso cref="ILogProvider" />
    public class ElasticSearchLogProvider : ILogProvider, IDisposable
    {
        /// <summary>
        /// The level.
        /// </summary>
        private LogLevel _level;

        /// <summary>
        /// The client.
        /// </summary>
        private readonly ElasticsearchClient _client;

        /// <summary>
        /// The index name.
        /// </summary>
        private readonly string _indexName;

        /// <summary>
        /// The token source.
        /// </summary>
        private readonly CancellationTokenSource _tokenSource;

        /// <summary>
        /// The log retention days.
        /// </summary>
        private readonly int _logRetentionDays;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticSearchLogProvider"/> class.
        /// </summary>
        /// <param name="elastic">The elastic.</param>
        /// <param name="logRetentionDays">The log retention days.</param>
        public ElasticSearchLogProvider(ElasticConnector elastic, int logRetentionDays)
        {
            _client = elastic.Client;
            _indexName = elastic.DefaultIndexName;
            _tokenSource = new CancellationTokenSource();
            Task.Delay(10000).ContinueWith(_ => GarbageCollector());
            _logRetentionDays = logRetentionDays;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ElasticSearchLogProvider" /> class.
        /// </summary>
        ~ElasticSearchLogProvider() => Dispose(false);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged
        /// resources.</param>
        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _tokenSource.Cancel();
        }

        /// <summary>
        /// Garbage collector.
        /// </summary>
        /// <returns>Task.</returns>
        private Task GarbageCollector()
        {
            if (_tokenSource.Token.IsCancellationRequested)
            {
                return Task.FromCanceled(_tokenSource.Token);
            }

            var retentionDays = _logRetentionDays;

            _client.DeleteByQueryAsync<LogMessage>(
                _indexName,
                d =>
                    d.Query(q =>
                        q.Range(r =>
                            r.DateRange(dr =>
                                dr.Field(f => f.Date)
                                    .Lt(DateMath.Now.Subtract($@"{retentionDays}d"))
                            )
                        )
                    )
            );

            return Task.CompletedTask;
        }

        /// <summary>
        /// Serializes the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        /// <param name="identifier">The identifier.</param>
        /// <returns>LogMessage.</returns>
        private static LogMessage Serialize(
            LogLevel level,
            string category,
            string message,
            string identifier = null
        ) =>
            new LogMessage
            {
                Application = EnvironmentHelper.ApplicationName,
                Category = category,
                Date = DateTime.Now,
                Hostname = EnvironmentHelper.Host,
                IpAddress = EnvironmentHelper.IpAddress,
                IpAddressRemote = EnvironmentHelper.IpAddressExternal,
                Level = level.GetHumanReadableValue(),
                Message = message,
                MessageIdentifier = identifier,
                Operation = EnvironmentHelper.Operation,
                ProcessId = EnvironmentHelper.ProcessId,
                UserAgent = EnvironmentHelper.UserAgent,
                ThreadId = Environment.CurrentManagedThreadId,
                ThreadName = Thread.CurrentThread.Name
            };

        /// <summary>
        /// Aborts the garbage collector.
        /// </summary>
        public void AbortGarbageCollector() => _tokenSource.Cancel();

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sets the log level of the instance.
        /// </summary>
        /// <param name="level">The log level.</param>
        public void SetLevel(LogLevel level) => _level = level;

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

            _client.IndexAsync(Serialize(LogLevel.Fatal, category, message));
        }

        /// <summary>
        /// Logs the message with error level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        public void Error(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Error))
            {
                return;
            }

            _client.IndexAsync(Serialize(LogLevel.Error, category, message));
        }

        /// <summary>
        /// Logs the message with warning level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        public void Warning(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Warning))
            {
                return;
            }

            _client.IndexAsync(Serialize(LogLevel.Warning, category, message));
        }

        /// <summary>
        /// Logs the message with info level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        public void Info(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Info))
            {
                return;
            }

            _client.IndexAsync(Serialize(LogLevel.Info, category, message));
        }

        /// <summary>
        /// Logs the message with trace level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        public void Trace(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Trace))
            {
                return;
            }

            _client.IndexAsync(Serialize(LogLevel.Trace, category, message));
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

            _client.IndexAsync(Serialize(LogLevel.Trace, category, message));

            Trace(category, exception);
        }

        /// <summary>
        /// Traces the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="exception">The exception.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.LayoutRules",
            "SA1500:Braces for multi-line statements should not share line",
            Justification = "Enhances readability."
        )]
        public void Trace(string category, Exception exception)
        {
            if (!_level.HasFlag(LogLevel.Trace))
            {
                return;
            }

            do
            {
                var ex = exception;
                _client.IndexAsync(Serialize(LogLevel.Trace, category, ex.Message));
                _client.IndexAsync(Serialize(LogLevel.Trace, category, ex.StackTrace));
                exception = exception.InnerException;
            } while (exception != null);
        }

        /// <summary>
        /// Logs the message with debug level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message to be logged.</param>
        public void Debug(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Debug))
            {
                return;
            }

            _client.IndexAsync(Serialize(LogLevel.Debug, category, message));
        }

        /// <summary>
        /// Logs the message as a file/attachment with a file name/identifier with debug level.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="content">The content to be stored.</param>
        /// <param name="identifier">The file name of the content. This can be a filename, a key, a identifier. Depends upon each implementation.</param>
        public void Debug(string category, string content, string identifier)
        {
            if (!_level.HasFlag(LogLevel.Debug))
            {
                return;
            }

            _client.IndexAsync(Serialize(LogLevel.Debug, category, content, identifier));
        }

        /// <summary>
        /// Logs the message as a file/attachment with a file name/identifier with debug level using a custom serializer or default.
        /// </summary>
        /// <typeparam name="T">any class that can be serialized to the <paramref name="customFormat" /> serializer format.</typeparam>
        /// <param name="category">The category.</param>
        /// <param name="content">The object to be serialized.</param>
        /// <param name="identifier">The filename/attachment identifier (file name or key).</param>
        /// <param name="customFormat">(Optional) the custom serializer format.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Critical Code Smell",
            "S1006:Method overrides should not change parameter defaults",
            Justification = "Keeping it for now."
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

            string serialized;

            if (customFormat == SerializerFormat.None)
            {
                serialized = (string)content.GetSerializer();
            }
            else
            {
                serialized = (string)content.GetCustomSerializer(customFormat);
            }

            _client.IndexAsync(Serialize(LogLevel.Debug, category, serialized, identifier));
        }
    }
}
