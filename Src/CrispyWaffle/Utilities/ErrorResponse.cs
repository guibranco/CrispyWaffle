// ***********************************************************************
// Assembly         : CrispyWaffle
// Author           : Guilherme Branco Stracini
// Created          : 19/04/2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 19/04/2020
// ***********************************************************************
// <copyright file="ErrorResponse.cs" company="Guilherme Branco Stracini ME">
//     © 2020 Guilherme Branco Stracini, All Rights Reserved
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.Utilities
{
    using System.Collections.Generic;

    /// <summary>
    /// The error response class.
    /// </summary>
    public class ErrorResponse : IJsonResponse
    {
        /// <summary>
        /// Gets or sets the error list.
        /// </summary>
        /// <value>The error list.</value>
        public Dictionary<string, IEnumerable<string>> ErrorList { get; set; }

        #region Implementation of IAPIResponse

        /// <summary>
        /// Gets the code.
        /// </summary>
        /// <value>The code.</value>
        public int Code { get; set; }
        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage { get; set; }

        #endregion
    }
}
