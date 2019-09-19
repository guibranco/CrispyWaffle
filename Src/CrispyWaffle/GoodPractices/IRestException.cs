namespace CrispyWaffle.GoodPractices
{

    /// <summary>
    /// Interface rest exception
    /// </summary>
    public interface IRestException
    {
        /// <summary>
        /// Gets or sets the request.
        /// </summary>
        /// <value>The request.</value>
        string Request { get; }

        /// <summary>
        /// Gets the response.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        string Response { get; }
    }
}
