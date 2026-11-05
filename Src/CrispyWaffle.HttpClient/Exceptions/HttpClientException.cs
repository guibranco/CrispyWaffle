using System;
using System.Net;

namespace CrispyWaffle.HttpClient.Exceptions;

/// <summary>
/// The exception thrown by <see cref="HttpClientWrapper"/> when a request fails and
/// <see cref="HttpRequestOptions.ThrowOnError"/> is <c>true</c>.
/// </summary>
public class HttpClientException : Exception
{
    /// <summary>
    /// Gets the HTTP status code of the failed response, or <c>null</c> when no response was received.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Gets the response body as text, when one is available.
    /// </summary>
    public string? ResponseContent { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the failure.</param>
    /// <param name="statusCode">The HTTP status code of the failed response, or <c>null</c> when no response was received.</param>
    /// <param name="responseContent">The response body as text, when one is available.</param>
    /// <param name="inner">The exception that caused the failure, if any.</param>
    public HttpClientException(
        string message,
        HttpStatusCode? statusCode = null,
        string? responseContent = null,
        Exception? inner = null
    )
        : base(message, inner)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }
}
