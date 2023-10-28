// ***********************************************************************
// Assembly         : CrispyWaffle.Redis
// Author           : Guilherme Branco Stracini
// Created          : 09-06-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-06-2020
// ***********************************************************************
// <copyright file="RedisConnector.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Diagnostics.CodeAnalysis;
using CrispyWaffle.Configuration;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;

namespace CrispyWaffle.Redis.Utils.Communications
{
    /// <summary>
    /// Class RedisConnector.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    [ConnectionName("Redis")]
    public class RedisConnector : IDisposable
    {
        #region Private fields

        /// <summary>
        /// The disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The connection pool manager
        /// </summary>
        private readonly IRedisConnectionPoolManager _connectionPoolManager;

        #endregion

        #region ~Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisConnector" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="serializer">The serializer.</param>
        public RedisConnector(MasterSlaveConfiguration configuration, ISerializer serializer)
            : this(configuration.HostsList, configuration.Password, serializer) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisConnector" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="queuePrefix">The queue prefix.</param>
        /// <exception cref="System.ArgumentNullException">queuePrefix</exception>
        /// <exception cref="ArgumentNullException">queuePrefix</exception>
        public RedisConnector(
            MasterSlaveConfiguration configuration,
            ISerializer serializer,
            string queuePrefix
        )
            : this(configuration.HostsList, configuration.Password, serializer) =>
            QueuePrefix = queuePrefix ?? throw new ArgumentNullException(nameof(queuePrefix));

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisConnector" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="serializer">The serializer.</param>
        public RedisConnector(IConnection connection, ISerializer serializer)
            : this(
                string.Concat(connection.Host, @":", connection.Port),
                connection.Credentials.Password,
                serializer
            ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisConnector" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="queuePrefix">The queue prefix.</param>
        /// <exception cref="System.ArgumentNullException">queuePrefix</exception>
        /// <exception cref="ArgumentNullException">queuePrefix</exception>
        public RedisConnector(IConnection connection, ISerializer serializer, string queuePrefix)
            : this(
                string.Concat(connection.Host, @":", connection.Port),
                connection.Credentials.Password,
                serializer
            ) => QueuePrefix = queuePrefix ?? throw new ArgumentNullException(nameof(queuePrefix));

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisConnector" /> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="password">The password.</param>
        /// <param name="serializer">The serializer.</param>
        public RedisConnector(string host, int port, string password, ISerializer serializer)
            : this(string.Concat(host, @":", port), password, serializer) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisConnector" /> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="password">The password.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="queuePrefix">The queue prefix.</param>
        /// <exception cref="System.ArgumentNullException">queuePrefix</exception>
        /// <exception cref="ArgumentNullException">queuePrefix</exception>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "TooManyDependencies")]
        public RedisConnector(
            string host,
            int port,
            string password,
            ISerializer serializer,
            string queuePrefix
        )
            : this(host, port, password, serializer) =>
            QueuePrefix = queuePrefix ?? throw new ArgumentNullException(nameof(queuePrefix));

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisConnector" /> class.
        /// </summary>
        /// <param name="hostsList">The hosts list.</param>
        /// <param name="password">The password.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="queuePrefix">The queue prefix.</param>
        /// <exception cref="System.ArgumentNullException">queuePrefix</exception>
        /// <exception cref="ArgumentNullException">queuePrefix</exception>
        public RedisConnector(
            string hostsList,
            string password,
            ISerializer serializer,
            string queuePrefix
        )
            : this(hostsList, password, serializer) =>
            QueuePrefix = queuePrefix ?? throw new ArgumentNullException(nameof(queuePrefix));

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisConnector" /> class.
        /// </summary>
        /// <param name="hostsList">The hosts list.</param>
        /// <param name="password">The password.</param>
        /// <param name="serializer">The serializer.</param>
        public RedisConnector(string hostsList, string password, ISerializer serializer)
        {
            var configuration = new RedisConfiguration
            {
                AbortOnConnectFail = false,
                AllowAdmin = true,
                Password = password,
                Hosts = hostsList
                    .Split(',')
                    .Select(host =>
                    {
                        var split = host.Split(':');

                        var hostname = split[0];
                        var port = split.Length > 1 ? int.Parse(split[1]) : 6379;
                        return new RedisHost { Host = hostname, Port = port };
                    })
                    .ToArray(),
                ServerEnumerationStrategy = new ServerEnumerationStrategy
                {
                    Mode = ServerEnumerationStrategy.ModeOptions.All,
                    TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
                    UnreachableServerAction = ServerEnumerationStrategy
                        .UnreachableServerActionOptions
                        .Throw
                }
            };

            //TODO add LogConsumer to logger parameter.
            _connectionPoolManager = new RedisConnectionPoolManager(configuration);

            Serializer = serializer;
            Cache = new RedisClient(_connectionPoolManager, serializer, configuration);
            QueuePrefix = "crispy-waffle";
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="RedisConnector" /> class.
        /// </summary>
        ~RedisConnector()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the specified disposing.
        /// </summary>
        /// <param name="disposing">if set to <c>true</c> [disposing].</param>
        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (_disposed)
            {
                return;
            }

            _connectionPoolManager.Dispose();
            _disposed = true;
        }

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

        #region Public properties

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <value>The serializer.</value>
        public ISerializer Serializer { get; }

        /// <summary>
        /// Gets the cache.
        /// </summary>
        /// <value>The cache.</value>
        public IRedisClient Cache { get; }

        /// <summary>
        /// Gets the subscriber.
        /// </summary>
        /// <value>The subscriber.</value>
        public ISubscriber Subscriber => _connectionPoolManager.GetConnection().GetSubscriber();

        /// <summary>
        /// Gets the default server.
        /// </summary>
        /// <returns>IServer.</returns>
        public IServer GetDefaultServer() =>
            _connectionPoolManager
                .GetConnection()
                .GetServer(_connectionPoolManager.GetConnection().GetEndPoints(true)[0]);

        /// <summary>
        /// Gets the default database.
        /// </summary>
        /// <value>The default database.</value>
        public IDatabase DefaultDatabase => _connectionPoolManager.GetConnection().GetDatabase();

        /// <summary>
        /// Gets the queue prefix.
        /// </summary>
        /// <value>The queue prefix.</value>
        public string QueuePrefix { get; }

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <param name="databaseNumber">The database number.</param>
        /// <param name="asyncState">State of the asynchronous.</param>
        /// <returns>IDatabase.</returns>
        public IDatabase GetDatabase(int databaseNumber, object asyncState = null) =>
            _connectionPoolManager.GetConnection().GetDatabase(databaseNumber, asyncState);

        #endregion
    }
}
