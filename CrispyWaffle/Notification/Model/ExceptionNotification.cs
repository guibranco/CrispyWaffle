namespace CrispyWaffle.Notification.Model
{
    using Serialization;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;

    /// <summary>
    /// The exception notification class.
    /// </summary>

    [Serializer]
    [NotificationType(MailNotificationType.ERROR)]
    public class ExceptionNotification : GenericNotification
    {
        #region Private fields

        /// <summary>
        /// The messages list
        /// </summary>

        private IList<string> _messages;

        /// <summary>
        /// The stack trace details list
        /// </summary>
        private IList<StackTrace> _details;

        #endregion

        #region ~Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionNotification"/> class.
        /// </summary>
        /// <param name="receivers">The receivers.</param>
        public ExceptionNotification(INotificationReceiver[] receivers)
        {
            Initialize();
            _messages = new List<string>();
            _details = new List<StackTrace>();
            Receivers = receivers;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionNotification"/> class.
        /// </summary>
        public ExceptionNotification()
        {
            Initialize();
            _messages = new List<string>();
            _details = new List<StackTrace>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds a message to the messages list
        /// </summary>
        /// <param name="message">The message</param>
        public void AddMessage(string message)
        {
            _messages.Add(message);
        }

        /// <summary>
        /// Adds a stack trace detail to the details list
        /// </summary>
        /// <param name="detail"></param>
        public void AddDetail(StackTrace detail)
        {
            _details.Add(detail);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the details.
        /// </summary>
        /// <value>
        /// The details.
        /// </value>
        [XmlArray("Details")]
        [XmlArrayItem("Detail")]
        public StackTrace[] Details
        {
            get => _details.ToArray();
            set => _details = value.ToList();
        }

        /// <summary>
        /// Gets or sets the messages.
        /// </summary>
        /// <value>
        /// The messages.
        /// </value>
        [XmlArray("Messages")]
        [XmlArrayItem("Message")]
        public string[] Messages
        {
            get => _messages.ToArray();
            set => _messages = value.ToList();
        }

        /// <summary>
        /// Gets the first message.
        /// </summary>
        /// <value>
        /// The first message.
        /// </value>
        [XmlElement("FirstMessage")]
        public string FirstMessage => _messages.FirstOrDefault() ?? string.Empty;

        /// <summary>
        /// Gets the first type.
        /// </summary>
        /// <value>
        /// The first type.
        /// </value>
        [XmlElement("FirstType")]
        public string FirstType => _details.FirstOrDefault()?.Type ?? string.Empty;

        /// <summary>
        /// Gets the last message.
        /// </summary>
        /// <value>
        /// The last message.
        /// </value>
        [XmlElement("LastMessage")]
        public string LastMessage => _messages.Count > 1 ? _messages.Last() : string.Empty;

        /// <summary>
        /// Gets the last type.
        /// </summary>
        /// <value>
        /// The last type.
        /// </value>
        [XmlElement("LastType")]
        public string LastType => _details.Count > 1 ? _details.Last().Type : string.Empty;

        /// <summary>
        /// Gets or sets the json request.
        /// </summary>
        /// <value>
        /// The json request.
        /// </value>
        [XmlElement(ElementName = "JsonRequest")]
        public string JsonRequest { get; set; }

        /// <summary>
        /// Gets or sets the json response.
        /// </summary>
        /// <value>
        /// The json response.
        /// </value>
        [XmlElement(ElementName = "JsonResponse")]
        public string JsonResponse { get; set; }

        /// <summary>
        /// Gets or sets the XML request.
        /// </summary>
        /// <value>
        /// The XML request.
        /// </value>
        [XmlElement(ElementName = "XmlRequest")]
        public string XmlRequest { get; set; }

        /// <summary>
        /// Gets or sets the XML response.
        /// </summary>
        /// <value>
        /// The XML response.
        /// </value>
        [XmlElement(ElementName = "XmlResponse")]
        public string XmlResponse { get; set; }

        #endregion
    }
}
