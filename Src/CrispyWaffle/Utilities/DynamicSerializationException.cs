namespace CrispyWaffle.Utilities
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The dynamic serialization exception class.
    /// This class cannot be inherited.
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class DynamicSerializationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSerializationException"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="innerException">The inner exception.</param>
        public DynamicSerializationException(string key, object value, Exception innerException)
            : base($"Could not add key {key} with value {value}", innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSerializationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected DynamicSerializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
