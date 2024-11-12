using System.Collections.Generic;

namespace CrispyWaffle.Utilities;

/// <summary>
/// Represents an error response that contains a list of errors, a code, and an error message.
/// This class is used to standardize error responses in JSON format for API communication.
/// </summary>
public class ErrorResponse : IJsonResponse
{
    /// <summary>
    /// Gets or sets the list of errors associated with the response.
    /// The key represents the error field or context, and the value is a collection of error messages related to that key.
    /// </summary>
    /// <value>
    /// A dictionary where the key is a string representing the error field or context, and the value is an <see cref="IEnumerable{string}"/>
    /// containing the error messages related to that field or context.
    /// </value>
    public Dictionary<string, IEnumerable<string>> ErrorList { get; set; }

    /// <summary>
    /// Gets or sets the error code associated with the response.
    /// The code is typically used to represent the type of error (e.g., 400 for bad request, 404 for not found).
    /// </summary>
    /// <value>
    /// An integer representing the error code.
    /// </value>
    public int Code { get; set; }

    /// <summary>
    /// Gets or sets a general error message that provides additional context or description of the error.
    /// This message is typically displayed to the user or logged for debugging purposes.
    /// </summary>
    /// <value>
    /// A string containing the error message.
    /// </value>
    public string ErrorMessage { get; set; }
}
