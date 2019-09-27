namespace CrispyWaffle.Validations
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Class InvalidDocumentException. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class InvalidDocumentException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDocumentException"/> class.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="value">The value.</param>
        public InvalidDocumentException(string documentType, string value)
            : base($"The value '{value}' isn't a valid value for a document of type {documentType} ")
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDocumentException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected InvalidDocumentException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
