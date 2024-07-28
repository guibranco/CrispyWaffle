﻿using System;
using System.Globalization;
using CrispyWaffle.Configuration;
using CrispyWaffle.Infrastructure;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace CrispyWaffle.ElasticSearch.Utils.Communications
{
    /// <summary>
    /// The Elasticsearch connector class.
    /// </summary>
    [ConnectionName("ElasticSearch")]
    public sealed class ElasticConnector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticConnector"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public ElasticConnector(IConnection connection)
            : this(
                connection,
                $"logs-{EnvironmentHelper.ApplicationName}-{EnvironmentHelper.Version}"
            ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticConnector"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="defaultIndexName">Default name of the index.</param>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public ElasticConnector(IConnection connection, string defaultIndexName)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(
                    nameof(connection),
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "A valid instance of {0} is required to initialize {1}!",
                        typeof(IConnection).FullName,
                        GetType().FullName
                    )
                );
            }

            var builder = new UriBuilder(
                connection.Port == 443 ? "https" : "http",
                connection.Host,
                connection.Port
            );
            var settings = new ElasticsearchClientSettings(builder.Uri).DefaultIndex(
                defaultIndexName
            );
            if (
                connection.Credentials != null
                && !string.IsNullOrWhiteSpace(connection.Credentials.Username)
            )
            {
                settings.Authentication(
                    new BasicAuthentication(
                        connection.Credentials.Username,
                        connection.Credentials.Password
                    )
                );
            }

            Client = new ElasticsearchClient(settings);
            DefaultIndexName = defaultIndexName;
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>The client.</value>
        public ElasticsearchClient Client { get; }

        /// <summary>
        /// Gets the default name of the index.
        /// </summary>
        /// <value>The default name of the index.</value>
        public string DefaultIndexName { get; }
    }
}
