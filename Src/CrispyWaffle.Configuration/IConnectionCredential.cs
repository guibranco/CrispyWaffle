// ***********************************************************************
// Assembly         : CrispyWaffle.Configuration
// Author           : Guilherme Branco Stracini
// Created          : 09-03-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-03-2020
// ***********************************************************************
// <copyright file="IConnectionCredential.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.Configuration
{
    /// <summary>
    /// Connection credential interface
    /// </summary>

    public interface IConnectionCredential
    {
        /// <summary>
        /// Gets or sets the password
        /// </summary>
        /// <value>The password.</value>
        string Password { get; set; }

        /// <summary>
        /// Gets or sets the user name
        /// </summary>
        /// <value>The name of the user.</value>
        string UserName { get; set; }
    }
}
