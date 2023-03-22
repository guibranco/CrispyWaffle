// ***********************************************************************
// Assembly         : CrispyWaffle.Utils
// Author           : Guilherme Branco Stracini
// Created          : 22/03/2023
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 22/03/2023
// ***********************************************************************
// <copyright file="MessageException.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.Utils.GoodPractices
{
    using System.Runtime.Serialization;
    using System;

    /// <summary>
    /// Class MessageException.
    /// Implements the <see cref="Exception" />
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class MessageException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>

        public MessageException()
            : base("The message cannot be set more than once for the same instance of Email class")
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageException" /> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected MessageException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
