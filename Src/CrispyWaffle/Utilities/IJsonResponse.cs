namespace CrispyWaffle.Utilities
{
    /// <summary>
    /// The JSON response interface
    /// </summary>
    public interface IJsonResponse
    {
        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>The code.</value>
        int Code { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>The error message.</value>
        string ErrorMessage { get; set; }
    }
}
