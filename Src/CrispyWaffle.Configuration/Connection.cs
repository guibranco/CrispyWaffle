// ***********************************************************************
// Assembly         : CrispyWaffle.Configuration
// Author           : Guilherme Branco Stracini
// Created          : 09-03-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-03-2020
// ***********************************************************************
// <copyright file="Connection.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace CrispyWaffle.Configuration
{
    using Serialization;
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>
    /// Class Connection. This class cannot be inherited.
    /// Implements the <see cref="CrispyWaffle.Configuration.IConnection" />
    /// </summary>
    /// <seealso cref="CrispyWaffle.Configuration.IConnection" />
    /// <seealso cref="IConnection" />
    [Serializer]
    public sealed class Connection : IConnection
    {
        #region ~Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection" /> class.
        /// </summary>
        public Connection()
        {
            Credentials = new Credentials();
        }

        #endregion

        #region Public properties

        /// <summary>
        /// The credentials
        /// </summary>
        /// <value>The credentials.</value>
        [XmlIgnore]
        public IConnectionCredential Credentials { get; set; }

        /// <summary>
        /// The credentials
        /// </summary>
        /// <value>The credentials internal.</value>
        /// <remarks>For XML Serialization</remarks>
        [XmlElement("Credentials")]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Credentials CredentialsInternal
        {
            get => (Credentials)Credentials;
            set => Credentials = value;
        }

        /// <summary>
        /// The host
        /// </summary>
        /// <value>The host.</value>
        [Localizable(false)]
        public string Host { get; set; }

        /// <summary>
        /// The connection name/identifier
        /// </summary>
        /// <value>The name.</value>
        [XmlAttribute(AttributeName = "Name")]
        [Localizable(false)]
        public string Name { get; set; }

        /// <summary>
        /// The host port
        /// </summary>
        /// <value>The port.</value>
        public int Port { get; set; }

        #endregion
    }
}
