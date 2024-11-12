using System;
using System.Runtime.Serialization;

namespace CrispyWaffle.Utils.GoodPractices;

/// <summary>
/// Represents an exception that is thrown when an attempt is made to set the message more than once for the same instance of the <see cref="Email"/> class.
/// This exception is specific to scenarios where the message cannot be modified after it has already been set.
/// </summary>
/// <seealso cref="Exception" />
[Serializable]
public class MessageException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageException"/> class with the default error message.
    /// </summary>
    /// <remarks>
    /// This constructor is typically used when the exception is thrown without a specific message. The default message indicates that
    /// the message cannot be set more than once for the same <see cref="Email"/> instance.
    /// </remarks>
    public MessageException()
        : base("The message cannot be set more than once for the same instance of Email class") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageException"/> class with the serialized data.
    /// This constructor is used during deserialization to reconstruct the exception from a stream of data.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> object that holds the serialized object data for the exception.</param>
    /// <param name="context">The <see cref="StreamingContext"/> object that provides contextual information about the source or destination of the exception.</param>
    /// <remarks>
    /// This constructor is used when the exception is being deserialized, for example, during cross-application domain transfers or
    /// when an exception is thrown across different system boundaries.
    /// </remarks>
    protected MessageException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
