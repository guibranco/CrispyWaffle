using System;
using System.Runtime.Serialization;

namespace CrispyWaffle.Utilities
{
    /// <summary>
    /// Represents an error that occurs during dynamic serialization, specifically when adding a key-value pair.
    /// </summary>
    /// <remarks>
    /// This exception is thrown when an error occurs while attempting to serialize or add a key-value pair to a dynamic serialization process.
    /// The exception message includes the key and value that caused the issue, as well as the inner exception that provides more details.
    /// </remarks>
    [Serializable]
    public class DynamicSerializationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSerializationException"/> class with a specific key, value, and inner exception.
        /// </summary>
        /// <param name="key">The key that could not be serialized.</param>
        /// <param name="value">The value associated with the key that could not be serialized.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <remarks>
        /// This constructor sets the exception message to include the key and value that caused the serialization failure,
        /// along with any inner exception that provides more detailed information about the error.
        /// </remarks>
        public DynamicSerializationException(string key, object value, Exception innerException)
            : base($"Could not add key {key} with value {value}", innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSerializationException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> object that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <remarks>
        /// This constructor is used during deserialization of the exception to recreate the exception from its serialized form.
        /// </remarks>
        protected DynamicSerializationException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
