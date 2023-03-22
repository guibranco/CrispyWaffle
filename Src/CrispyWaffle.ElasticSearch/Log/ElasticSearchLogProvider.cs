// ***********************************************************************
// Assembly         : CrispyWaffle.ElasticSearch
// Author           : Guilherme Branco Stracini
// Created          : 10/09/2022
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 10/09/2022
// ***********************************************************************
// <copyright file="ElasticSearchLogProvider.cs" company="Guilherme Branco Stracini ME">
//     © 2022 Guilherme Branco Stracini, All Rights Reserved
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.ElasticSearch.Log
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CrispyWaffle.ElasticSearch.Utils.Communications;
    using CrispyWaffle.Extensions;
    using CrispyWaffle.Infrastructure;
    using CrispyWaffle.Log;
    using CrispyWaffle.Log.Providers;
    using CrispyWaffle.Serialization;
    using Nest;
    using LogLevel = CrispyWaffle.Log.LogLevel;

    /// <summary>
    /// The Elastic Search log provider class.
    /// </summary>
    /// <seealso cref="ILogProvider" />
    public class ElasticSearchLogProvider : ILogProvider, IDisposable
    {
        #region Private fields

        /// <summary>
        /// The level
        /// </summary>
        /// <summary>
        /// The level
        /// </summary>
        private LogLevel _level;

        /// <summary>
        /// The client
        /// </summary>
        /// <summary>
        /// The client
        /// </summary>
        private readonly ElasticClient _client;

        /// <summary>
        /// The index name
        /// </summary>
        /// <summary>
        /// The index name
        /// </summary>
        private readonly string _indexName;

        /// <summary>
        /// The token source
        /// </summary>
        private readonly CancellationTokenSource _tokenSource;

        /// <summary>
        /// The log retention days
        /// </summary>
        private readonly int _logRetentionDays;

        #endregion

        #region ~Ctor

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

        #endregion

        #region Private methods

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
                return null;
            }

            var retentionDays = _logRetentionDays;
            _client.DeleteByQuery<LogMessage>(d =>
                d.Index(_indexName)
                    .Query(q =>
                        q.DateRange(g => g
                            .Field(f => f.Date)
                            .LessThan(DateMath.Now.Subtract($@"{retentionDays}d")))));
            return null;
        }

        /// <summary>
        /// Serializes the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="category">The category</param>
        /// <param name="message">The message.</param>
        /// <param name="identifier">The identifier.</param>
        /// <returns>LogMessage.</returns>
        private static LogMessage Serialize(LogLevel level, string category, string message, string identifier = null) =>
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
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                ThreadName = Thread.CurrentThread.Name
            };

        #endregion

        #region Public methods

        /// <summary>
        /// Aborts the garbage collector.
        /// </summary>

        public void AbortGarbageCollector() => _tokenSource.Cancel();

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Implementation of ILogProvider

        /// <summary>
        /// Sets the log level of the instance
        /// </summary>
        /// <param name="level">The log level</param>
        public void SetLevel(LogLevel level) => _level = level;

        /// <summary>
        /// Logs the message with fatal level
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        public void Fatal(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Fatal))
            {
                return;
            }

            Task.Factory.StartNew(() => _client.IndexDocument(Serialize(LogLevel.Fatal, category, message)));
        }

        /// <summary>
        /// Logs the message with error level
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged</param>
        public void Error(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Error))
            {
                return;
            }

            Task.Factory.StartNew(() => _client.IndexDocument(Serialize(LogLevel.Error, category, message)));
        }

        /// <summary>
        /// Logs the message with warning level
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged</param>
        public void Warning(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Warning))
            {
                return;
            }

            Task.Factory.StartNew(() => _client.IndexDocument(Serialize(LogLevel.Warning, category, message)));
        }

        /// <summary>
        /// Logs the message with info level
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged</param>
        public void Info(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Info))
            {
                return;
            }

            Task.Factory.StartNew(() => _client.IndexDocument(Serialize(LogLevel.Info, category, message)));
        }

        /// <summary>
        /// Logs the message with trace level
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged</param>
        public void Trace(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Trace))
            {
                return;
            }

            Task.Factory.StartNew(() => _client.IndexDocument(Serialize(LogLevel.Trace, category, message)));
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

            Task.Factory.StartNew(() => _client.IndexDocument(Serialize(LogLevel.Trace, category, message)));
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
                var ex = exception;
                Task.Factory.StartNew(() =>
                {
                    _client.IndexDocument(Serialize(LogLevel.Trace, category, ex.Message));
                    _client.IndexDocument(Serialize(LogLevel.Trace, category, ex.StackTrace));
                });

                exception = exception.InnerException;

            } while (exception != null);
        }

        /// <summary>
        /// Logs the message with debug level
        /// </summary>
        /// <param name="category">The category</param>
        /// <param name="message">The message to be logged</param>
        public void Debug(string category, string message)
        {
            if (!_level.HasFlag(LogLevel.Debug))
            {
                return;
            }

            Task.Factory.StartNew(() => _client.IndexDocument(Serialize(LogLevel.Debug, category, message)));
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

            Task.Factory.StartNew(() => _client.IndexDocument(Serialize(LogLevel.Debug, category, content, identifier)));
        }

        /// <summary>
        /// Logs the message as a file/attachment with a file name/identifier with debug level using a custom serializer or default.
        /// </summary>
        /// <typeparam name="T">any class that can be serialized to the <paramref name="customFormat" /> serializer format</typeparam>
        /// <param name="category">The category</param>
        /// <param name="content">The object to be serialized</param>
        /// <param name="identifier">The filename/attachment identifier (file name or key)</param>
        /// <param name="customFormat">(Optional) the custom serializer format</param>
        public void Debug<T>(string category, T content, string identifier, SerializerFormat customFormat = SerializerFormat.None) where T : class, new()
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

            Task.Factory.StartNew(() => _client.IndexDocument(Serialize(LogLevel.Debug, category, serialized, identifier)));
        }

        #endregion
    }
}
