using System;

namespace CrispyWaffle.LogStream;

/// <summary>
/// Reports a LogStream delivery problem through <see cref="LogStreamOptions.OnError"/>.
/// </summary>
public class LogStreamException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogStreamException"/> class.
    /// </summary>
    public LogStreamException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogStreamException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public LogStreamException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogStreamException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LogStreamException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogStreamException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="statusCode">The HTTP status code returned by the server.</param>
    /// <param name="responseBody">The response body returned by the server.</param>
    public LogStreamException(string message, int? statusCode, string responseBody)
        : this(message, statusCode, responseBody, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogStreamException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="statusCode">The HTTP status code returned by the server.</param>
    /// <param name="responseBody">The response body returned by the server.</param>
    /// <param name="innerException">The inner exception.</param>
    public LogStreamException(
        string message,
        int? statusCode,
        string responseBody,
        Exception innerException
    )
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    /// <summary>
    /// Gets the HTTP status code returned by the server, when there was a response.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Gets the response body returned by the server, when there was a response.
    /// </summary>
    public string ResponseBody { get; }
}
