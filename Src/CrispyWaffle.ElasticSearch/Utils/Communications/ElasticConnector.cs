// ***********************************************************************
// Assembly         : CrispyWaffle.ElasticSearch
// Author           : Guilherme Branco Stracini
// Created          : 10/09/2022
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 10/09/2022
// ***********************************************************************
// <copyright file="ElasticConnector.cs" company="Guilherme Branco Stracini ME">
//     © 2022 Guilherme Branco Stracini, All Rights Reserved
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.ElasticSearch.Utils.Communications
{
    using System;
    using System.Globalization;
    using CrispyWaffle.Configuration;
    using CrispyWaffle.Infrastructure;
    using Nest;

    /// <summary>
    /// The Elastic Search connector class
    /// </summary>
    [ConnectionName("ElasticSearch")]
    public sealed class ElasticConnector
    {
        #region ~Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticConnector" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public ElasticConnector(IConnection connection)
            : this(connection, $"logs-{EnvironmentHelper.ApplicationName}-{EnvironmentHelper.Version}")
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticConnector" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="defaultIndexName">Default name of the index.</param>
        /// <exception cref="ArgumentNullException">connection</exception>
        public ElasticConnector(IConnection connection, string defaultIndexName)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection), string.Format(CultureInfo.CurrentCulture, "A valid instance of {0} is required to initialize {1}!", typeof(IConnection).FullName, GetType().FullName));
            }

            var builder = new UriBuilder(connection.Port == 443 ? "https" : "http", connection.Host, connection.Port);
            var settings = new ConnectionSettings(builder.Uri).DefaultIndex(defaultIndexName);
            if (connection.Credentials != null &&
                !string.IsNullOrWhiteSpace(connection.Credentials.UserName))
            {
                settings.BasicAuthentication(connection.Credentials.UserName, connection.Credentials.Password);
            }

            Client = new ElasticClient(settings);
            DefaultIndexName = defaultIndexName;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>The client.</value>
        public ElasticClient Client { get; }

        /// <summary>
        /// Gets the default name of the index.
        /// </summary>
        /// <value>The default name of the index.</value>
        public string DefaultIndexName { get; }

        #endregion
    }
}
