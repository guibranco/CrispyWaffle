using System;
using System.Runtime.Serialization;

namespace CrispyWaffle.Validations
{
    /// <summary>
    /// Exception thrown when an invalid email address format is encountered.
    /// </summary>
    [Serializable]
    public class InvalidEmailAddressException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidEmailAddressException"/> class.
        /// </summary>
        /// <param name="emailAddress">The invalid email address that caused the exception.</param>
        /// <remarks>
        /// This constructor provides a message indicating that the specified email address is not in a valid format.
        /// </remarks>
        public InvalidEmailAddressException(string emailAddress)
            : base($"The e-mail address '{emailAddress}' isn't in a valid e-mail address format")
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidEmailAddressException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that contains the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <remarks>
        /// This constructor is used for deserialization of the exception, restoring the exception from a serialized state.
        /// </remarks>
        protected InvalidEmailAddressException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
