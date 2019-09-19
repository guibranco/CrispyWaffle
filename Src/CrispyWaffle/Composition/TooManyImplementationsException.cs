namespace CrispyWaffle.Composition
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The too many implementations exception class.
    /// This exception is thrown when there is too many implementation of a type available for auto registration.
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class TooManyImplementationsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TooManyImplementationsException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public TooManyImplementationsException(Type type)
            : base($"The type {type.FullName} has many implementations available, please consider registering one of them")
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TooManyImplementationsException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected TooManyImplementationsException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
