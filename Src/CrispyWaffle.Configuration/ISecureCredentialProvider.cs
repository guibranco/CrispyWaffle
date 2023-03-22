// ***********************************************************************
// Assembly         : CrispyWaffle.Configuration
// Author           : Guilherme Branco Stracini
// Created          : 09-06-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-06-2020
// ***********************************************************************
// <copyright file="ISecureCredentialProvider.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.Configuration
{
    /// <summary>
    /// Interface ISecureCredentialProvider
    /// </summary>
    public interface ISecureCredentialProvider
    {
        /// <summary>
        /// Gets or sets the password hash.
        /// </summary>
        /// <value>The password hash.</value>
        string PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets the salt key.
        /// </summary>
        /// <value>The salt key.</value>
        string SaltKey { get; set; }

        /// <summary>
        /// Gets or sets the initialization vector key.
        /// </summary>
        /// <value>The initialization vector key.</value>
        string IVKey { get; set; }
    }
}
