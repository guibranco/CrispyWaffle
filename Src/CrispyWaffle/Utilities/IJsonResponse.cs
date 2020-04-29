// ***********************************************************************
// Assembly         : CrispyWaffle
// Author           : Guilherme Branco Stracini
// Created          : 19/04/2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 19/04/2020
// ***********************************************************************
// <copyright file="IJsonResponse.cs" company="Guilherme Branco Stracini ME">
//     © 2020 Guilherme Branco Stracini, All Rights Reserved
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.Utilities
{
    /// <summary>
    /// The JSON response interface
    /// </summary>
    public interface IJsonResponse
    {
        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>The code.</value>
        int Code { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>The error message.</value>
        string ErrorMessage { get; set; }
    }
}
