using System;
using System.Collections.Generic;
using System.Net;

namespace CrispyWaffle.HttpClient;

/// <summary>
/// Generic response wrapper returned by the wrapper methods.
/// </summary>
/// <typeparam name="T">The type the response body is deserialized into.</typeparam>
public class HttpResponse<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the request succeeded, meaning that a success status code
    /// was returned and the response body was deserialized without error.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code of the response, or <c>null</c> when no response was received.
    /// </summary>
    public HttpStatusCode? StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the deserialized response body. It holds the default value of <typeparamref name="T"/>
    /// when the request failed or the body was empty.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets the response body as text.
    /// </summary>
    public string? RawContent { get; set; }

    /// <summary>
    /// Gets or sets the failure messages, or <c>null</c> when the request succeeded.
    /// </summary>
    public IList<string>? Errors { get; set; }

    /// <summary>
    /// Gets or sets the response and content headers, or <c>null</c> when no response was received.
    /// </summary>
    public IDictionary<string, IEnumerable<string>>? Headers { get; set; }

    /// <summary>
    /// Gets or sets the time spent on all attempts and retry delays, up to the arrival of the final response headers.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    /// <param name="data">The deserialized response body.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="rawContent">The response body as text.</param>
    /// <param name="headers">The response and content headers.</param>
    /// <param name="duration">The time spent on the request.</param>
    /// <returns>An <see cref="HttpResponse{T}"/> with <see cref="IsSuccess"/> set to <c>true</c>.</returns>
    public static HttpResponse<T> Success(
        T? data,
        HttpStatusCode statusCode,
        string? rawContent,
        IDictionary<string, IEnumerable<string>>? headers,
        TimeSpan duration
    )
    {
        return new HttpResponse<T>
        {
            IsSuccess = true,
            StatusCode = statusCode,
            Data = data,
            RawContent = rawContent,
            Headers = headers,
            Duration = duration,
        };
    }

    /// <summary>
    /// Creates a failed response.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response, or <c>null</c> when no response was received.</param>
    /// <param name="rawContent">The response body as text, when one is available.</param>
    /// <param name="errors">The failure messages.</param>
    /// <param name="headers">The response and content headers, when a response was received.</param>
    /// <param name="duration">The time spent on the request.</param>
    /// <returns>An <see cref="HttpResponse{T}"/> with <see cref="IsSuccess"/> set to <c>false</c>.</returns>
    public static HttpResponse<T> Failure(
        HttpStatusCode? statusCode,
        string? rawContent,
        IList<string>? errors,
        IDictionary<string, IEnumerable<string>>? headers,
        TimeSpan duration
    )
    {
        return new HttpResponse<T>
        {
            IsSuccess = false,
            StatusCode = statusCode,
            RawContent = rawContent,
            Errors = errors,
            Headers = headers,
            Duration = duration,
        };
    }
}
