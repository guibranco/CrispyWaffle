// ***********************************************************************
// Assembly         : CrispyWaffle.Utils
// Author           : Guilherme Branco Stracini
// Created          : 22/03/2023
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 22/03/2023
// ***********************************************************************
// <copyright file="FtpClientException.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace CrispyWaffle.Utils.GoodPractices
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Runtime.Serialization;

    /// <summary>
    /// Class FtpClientException.
    /// Implements the <see cref="Exception" />
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class FtpClientException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpClientException" /> class.
        /// </summary>
        public FtpClientException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpClientException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public FtpClientException(string message)
            : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpClientException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public FtpClientException(string message, Exception innerException)
            : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpClientException" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="action">The action.</param>
        /// <param name="responseCode">The response code.</param>
        public FtpClientException(string path, string action, FtpStatusCode responseCode)
            : base(
                string.Format(
                    CultureInfo.CurrentCulture,
                    "Unable to {1} the path/file {0} in the FtpClient host. Status code: {2}",
                    path,
                    action,
                    responseCode
                )
            ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpClientException" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="action">The action (create,remove,etc).</param>
        /// <param name="innerException">The inner exception.</param>
        public FtpClientException(string path, string action, Exception innerException)
            : base(
                string.Format(
                    CultureInfo.CurrentCulture,
                    "Unable to {1} the path/file {0} in the FtpClient host. Status code: {2}",
                    path,
                    action,
                    0
                ),
                innerException
            ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpClientException" /> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected FtpClientException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
