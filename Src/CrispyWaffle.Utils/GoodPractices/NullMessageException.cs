using System;
using System.Runtime.Serialization;

namespace CrispyWaffle.Utils.GoodPractices
{
    /// <summary>
    /// Represents an exception that is thrown when an attempt is made to set the attachment of a message before the message itself is set.
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class NullMessageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullMessageException"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor provides a default message indicating that the attachment of a message
        /// cannot be set before the message itself.
        /// </remarks>
        public NullMessageException()
            : base("Unable to set the attachment of message before the message itself") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NullMessageException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <remarks>
        /// This constructor is used for deserialization purposes and allows the exception to be properly reconstituted during
        /// deserialization from a serialized state.
        /// </remarks>
        protected NullMessageException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
