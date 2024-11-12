using System;
using System.Runtime.Serialization;

namespace CrispyWaffle.Validations
{
    /// <summary>
    /// Represents an exception that is thrown when a document's value is invalid for a specific document type.
    /// This exception cannot be inherited.
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class InvalidDocumentException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDocumentException"/> class with a specified document type and value.
        /// </summary>
        /// <param name="documentType">The type of the document (e.g., "ID card", "Passport").</param>
        /// <param name="value">The invalid value that was provided for the document.</param>
        /// <remarks>
        /// This constructor generates an error message indicating that the provided value is not valid for the specified document type.
        /// </remarks>
        public InvalidDocumentException(string documentType, string value)
            : base($"The value '{value}' isn't a valid value for a document of type {documentType}")
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDocumentException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <remarks>
        /// This constructor is used during the deserialization process when the exception is being transmitted over a network or saved to a file.
        /// </remarks>
        protected InvalidDocumentException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
