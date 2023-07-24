// ***********************************************************************
// Assembly         : CrispyWaffle.Configuration
// Author           : Guilherme Branco Stracini
// Created          : 09-03-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-03-2020
// ***********************************************************************
// <copyright file="IConnection.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace CrispyWaffle.Configuration
{
    using System.ComponentModel;

    /// <summary>
    /// Connection data interface
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// The connection name/identifier
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the host
        /// </summary>
        /// <value>The host.</value>

        [Localizable(false)]
        string Host { get; set; }

        /// <summary>
        /// Gets or sets the port
        /// </summary>
        /// <value>The port.</value>

        int Port { get; set; }

        /// <summary>
        /// Gets or sets the credentials
        /// </summary>
        /// <value>The credentials.</value>

        IConnectionCredential Credentials { get; set; }
    }
}
