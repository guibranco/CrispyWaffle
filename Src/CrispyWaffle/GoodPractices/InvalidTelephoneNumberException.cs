namespace CrispyWaffle.GoodPractices
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Class InvalidTelephoneNumberException. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class InvalidTelephoneNumberException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTelephoneNumberException"/> class.
        /// </summary>
        /// <param name="telephoneNumber">The telephone number.</param>
        public InvalidTelephoneNumberException(string telephoneNumber)
            : base($"The value '{telephoneNumber}' isn't a valid telephone number")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTelephoneNumberException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected InvalidTelephoneNumberException(SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }
    }
}
