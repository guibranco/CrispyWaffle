using System.Xml;

namespace CrispyWaffle.GoodPractices
{
    /// <summary>
    /// Represents an exception that occurs during an XML service operation.
    /// This interface provides access to the request and response XML documents involved in the exception.
    /// </summary>
    public interface IXmlServiceException
    {
        /// <summary>
        /// Gets the XML document representing the request that caused the exception.
        /// </summary>
        /// <value>The <see cref="XmlDocument"/> representing the request.</value>
        /// <remarks>
        /// This property provides the XML data that was sent in the request, allowing for troubleshooting and analysis of the request payload.
        /// </remarks>
        XmlDocument Request { get; }

        /// <summary>
        /// Gets the XML document representing the response received from the service.
        /// </summary>
        /// <value>The <see cref="XmlDocument"/> representing the response.</value>
        /// <remarks>
        /// This property provides the XML data returned from the service, enabling investigation of the service's response.
        /// </remarks>
        XmlDocument Response { get; }
    }
}
