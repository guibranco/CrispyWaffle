// ***********************************************************************
// Assembly         : CrispyWaffle.Configuration
// Author           : Guilherme Branco Stracini
// Created          : 09-06-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-06-2020
// ***********************************************************************
// <copyright file="SecureCredentialProvider.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace CrispyWaffle.Configuration
{
    /// <summary>
    /// Class SecureCredentialProvider.
    /// Implements the <see cref="CrispyWaffle.Configuration.ISecureCredentialProvider" />
    /// </summary>
    /// <seealso cref="CrispyWaffle.Configuration.ISecureCredentialProvider" />
    public class SecureCredentialProvider : ISecureCredentialProvider
    {
        #region Implementation of ISecureCredentialProvider

        /// <summary>
        /// Gets or sets the password hash.
        /// </summary>
        /// <value>The password hash.</value>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets the salt key.
        /// </summary>
        /// <value>The salt key.</value>
        public string SaltKey { get; set; }

        /// <summary>
        /// Gets or sets the initialization vector key.
        /// </summary>
        /// <value>The initialization vector key.</value>
        public string IVKey { get; set; }

        #endregion
    }
}
