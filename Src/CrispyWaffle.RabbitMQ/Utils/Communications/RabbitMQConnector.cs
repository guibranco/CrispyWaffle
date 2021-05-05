// ***********************************************************************
// Assembly         : CrispyWaffle.RabbitMQ
// Author           : Guilherme Branco Stracini
// Created          : 03-31-2021
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 05-05-2021
// ***********************************************************************
// <copyright file="RabbitMqConnector.cs" company="Guilherme Branco Stracini ME">
//     © 2020 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.RabbitMQ.Utils.Communications
{
    using CrispyWaffle.Configuration;
    using CrispyWaffle.Infrastructure;
    using System;
    using global::RabbitMQ.Client;
    using IConnection = CrispyWaffle.Configuration.IConnection;

    /// <summary>
    /// Class RabbitMQConnector. This class cannot be inherited.
    /// </summary>
    [ConnectionName("RabbitMQ")]
    public sealed class RabbitMQConnector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMQConnector" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public RabbitMQConnector(IConnection connection)
        : this(connection, "/", $"{EnvironmentHelper.ApplicationName}.logs")
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMQConnector" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="virtualHost">The virtual host.</param>
        /// <param name="defaultExchange">The default exchange.</param>
        /// <exception cref="ArgumentNullException">connection</exception>
        /// <exception cref="ArgumentNullException">defaultExchange</exception>
        public RabbitMQConnector(IConnection connection, string virtualHost, string defaultExchange)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            ConnectionFactory = new ConnectionFactory { HostName = connection.Host, Port = connection.Port };

            if (!string.IsNullOrWhiteSpace(connection.Credentials?.UserName))
            {
                ConnectionFactory.UserName = connection.Credentials.UserName;
            }

            if (!string.IsNullOrWhiteSpace(connection.Credentials?.Password))
            {
                ConnectionFactory.Password = connection.Credentials.Password;
            }

            ConnectionFactory.VirtualHost = virtualHost ?? throw new ArgumentNullException(nameof(defaultExchange));

            DefaultExchangeName = defaultExchange;
        }

        /// <summary>
        /// Gets the connection factory.
        /// </summary>
        /// <value>The connection factory.</value>
        public IConnectionFactory ConnectionFactory { get; }

        /// <summary>
        /// Gets the default name of the exchange.
        /// </summary>
        /// <value>The default name of the exchange.</value>
        public string DefaultExchangeName { get; }
    }
}