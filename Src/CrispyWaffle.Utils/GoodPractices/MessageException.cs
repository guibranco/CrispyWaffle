namespace CrispyWaffle.Utils.GoodPractices
{
    using System.Runtime.Serialization;
    using System;

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
        /// Initializes a new instance of the <see cref="MessageException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected MessageException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
