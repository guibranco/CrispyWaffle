using System;
using System.Globalization;
using System.Net;
using System.Runtime.Serialization;
using CrispyWaffle.Utils.Communications;

namespace CrispyWaffle.Utils.GoodPractices;

/// <summary>
/// Represents errors that occur during FTP operations in the <see cref="FtpClient"/>.
/// This class provides several constructors to help create exceptions with relevant messages and inner exceptions.
/// </summary>
/// <seealso cref="Exception" />
[Serializable]
public class FtpClientException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FtpClientException"/> class.
    /// This constructor is used when no specific error message is provided.
    /// </summary>
    public FtpClientException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FtpClientException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that describes the reason for the exception.</param>
    /// <remarks>
    /// Use this constructor when you want to specify an error message without providing additional context or inner exceptions.
    /// </remarks>
    public FtpClientException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FtpClientException"/> class with a specified error message and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or <see langword="null"/> if no inner exception is specified.</param>
    /// <remarks>
    /// Use this constructor to provide additional context about the exception by passing an inner exception, which can be helpful in debugging.
    /// </remarks>
    public FtpClientException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FtpClientException"/> class with a formatted message indicating the failed FTP operation.
    /// </summary>
    /// <param name="path">The path or file that caused the error.</param>
    /// <param name="action">The FTP action (e.g., create, remove, retrieve) that was attempted.</param>
    /// <param name="responseCode">The FTP server's response code that indicates the result of the action.</param>
    /// <remarks>
    /// This constructor is typically used when an FTP operation fails, providing detailed information about the operation, including the path, action, and response code.
    /// </remarks>
    public FtpClientException(string path, string action, FtpStatusCode responseCode)
        : base(
            string.Format(
                CultureInfo.CurrentCulture,
                "Unable to {1} the path/file {0} in the FtpClient host. Status code: {2}",
                path,
                action,
                responseCode
            )
        ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FtpClientException"/> class with a formatted message and an inner exception.
    /// </summary>
    /// <param name="path">The path or file that caused the error.</param>
    /// <param name="action">The FTP action (e.g., create, remove, retrieve) that was attempted.</param>
    /// <param name="innerException">The inner exception that caused the current exception.</param>
    /// <remarks>
    /// This constructor allows you to provide a specific message along with an inner exception, which can help in debugging by tracing the root cause.
    /// </remarks>
    public FtpClientException(string path, string action, Exception innerException)
        : base(
            string.Format(
                CultureInfo.CurrentCulture,
                "Unable to {1} the path/file {0} in the FtpClient host. Status code: {2}",
                path,
                action,
                0
            ),
            innerException
        ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FtpClientException"/> class from serialized data.
    /// This constructor is used during deserialization to recreate the exception from a stream of data.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> object that holds the serialized object data for the exception.</param>
    /// <param name="context">The <see cref="StreamingContext"/> object that provides contextual information about the source or destination of the exception.</param>
    /// <remarks>
    /// This constructor is called during deserialization and is used to rehydrate the exception when it is transferred across application domains.
    /// </remarks>
    protected FtpClientException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
