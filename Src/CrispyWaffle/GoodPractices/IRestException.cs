namespace CrispyWaffle.GoodPractices;

/// <summary>
/// Represents an exception that occurs during an HTTP REST request/response cycle.
/// This interface is used to capture the details of both the request and the response
/// for logging, debugging, or error handling purposes.
/// </summary>
public interface IRestException
{
    /// <summary>
    /// Gets the request that caused the exception.
    /// This typically contains the HTTP request details such as the URL, headers,
    /// request body, or other relevant information about the request.
    /// </summary>
    /// <value>
    /// A string representing the request information.
    /// </value>
    string Request { get; }

    /// <summary>
    /// Gets the response received after the request was made, typically containing
    /// error details, status codes, or any relevant information about the failure.
    /// </summary>
    /// <value>
    /// A string representing the response details, such as the HTTP status code,
    /// error message, or any other information returned from the server.
    /// </value>
    string Response { get; }
}
