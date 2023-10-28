// ***********************************************************************
// Assembly         : CrispyWaffle.Utils
// Author           : Guilherme Branco Stracini
// Created          : 22/03/2023
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 22/03/2023
// ***********************************************************************
// <copyright file="NullMessageException.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Runtime.Serialization;

namespace CrispyWaffle.Utils.GoodPractices
{
    /// <summary>
    /// Class NullMessageException.
    /// Implements the <see cref="Exception" />
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class NullMessageException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public NullMessageException()
            : base("Unable to set the attachment of message before the message itself") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NullMessageException" /> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected NullMessageException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
