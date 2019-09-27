namespace CrispyWaffle.Validations
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Class InvalidEmailAddressException. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class InvalidEmailAddressException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidEmailAddressException"/> class.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        public InvalidEmailAddressException(string emailAddress)
            : base($"The e-mail address '{emailAddress}' isn't in a valid e-mail address format")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidEmailAddressException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected InvalidEmailAddressException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
