namespace CrispyWaffle.Notification.Model
{
    using Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// The stack trace details class
    /// </summary>
    [Serializer]
    public class StackTrace
    {
        /// <summary>
        /// The type of stack
        /// </summary>
        [XmlElement(ElementName = "Type")]
        public string Type { get; set; }

        /// <summary>
        /// The stack of the exception
        /// </summary>
        [XmlElement(ElementName = "Stack")]
        public string Stack { get; set; }
    }
}
