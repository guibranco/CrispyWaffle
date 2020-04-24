namespace CrispyWaffle.GoodPractices
{
    using System.Xml;

    /// <summary>
    /// Interface xml service exception
    /// </summary>
    public interface IXmlServiceException
    {
        /// <summary>
        /// Gets the request.
        /// </summary>
        /// <value>The request.</value>
        XmlDocument Request { get; }
        /// <summary>
        /// Gets the response.
        /// </summary>
        /// <value>The response.</value>
        XmlDocument Response { get; }
    }
}
