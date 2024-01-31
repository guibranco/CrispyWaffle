using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CrispyWaffle.Serialization.NewtonsoftJson
{
    /// <summary>
    /// The not null observer exception class.
    /// This class cannot be inherited.
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class NotNullObserverException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotNullObserverException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <param name="path">The path.</param>
        public NotNullObserverException(JsonToken type, object value, string path)
            : base(
                $"Not null observer found a not null value in path {path} of type {type}: {value}"
            )
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotNullObserverException"/> class.
        /// </summary>
        /// <param name="parentType">Type of the parent.</param>
        /// <param name="innerException">The inner exception.</param>
        public NotNullObserverException(Type parentType, Exception innerException)
            : base(
                $"Unable to serialize type {parentType.FullName}. Constraints: Not null observer",
                innerException
            )
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotNullObserverException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected NotNullObserverException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
