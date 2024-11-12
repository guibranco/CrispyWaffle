namespace CrispyWaffle.Utilities
{
    /// <summary>
    /// Represents a response in JSON format, typically used to standardize the structure of API responses.
    /// This interface defines the properties for the status code and error message that may be included in the response.
    /// </summary>
    public interface IJsonResponse
    {
        /// <summary>
        /// Gets or sets the status code of the JSON response.
        /// The code typically represents the outcome of the request (e.g., HTTP status codes like 200, 400, 500).
        /// </summary>
        /// <value>The status code associated with the response.</value>
        int Code { get; set; }

        /// <summary>
        /// Gets or sets the error message associated with the JSON response.
        /// This message provides additional details about the response, particularly when the code indicates an error.
        /// </summary>
        /// <value>A string containing the error message, if applicable.</value>
        string ErrorMessage { get; set; }
    }
}
