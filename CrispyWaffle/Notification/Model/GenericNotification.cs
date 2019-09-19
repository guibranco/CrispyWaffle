namespace CrispyWaffle.Notification.Model
{
    using Attributes;
    using Serialization;
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// The generic notification class, implements <see cref="INotification"/>
    /// This is a abstract implementation for notifications.
    /// </summary>

    [Serializer]
    public abstract class GenericNotification : INotification
    {
        #region Implementation of INotification

        /// <summary>
        /// Gets the receivers.
        /// </summary>
        /// <value>
        /// The receivers.
        /// </value>
        [XmlIgnore]
        public INotificationReceiver[] Receivers { get; protected set; }

        /// <summary>
        /// 	Gets or sets the operation.
        /// </summary>
        ///
        /// <value>
        /// 	The operation.
        /// </value>
        public string Operation { get; set; }

        /// <summary>
        /// 	Gets or sets the execution triggered origin.
        /// </summary>
        ///
        /// <value>
        /// 	The execution trigger
        /// </value>

        [XmlIgnore]
        public Execution TriggeredBy { get; set; }

        /// <summary>
        /// 	Gets or sets the host.
        /// </summary>
        ///
        /// <value>
        /// 	The host.
        /// </value>

        public string Host { get; set; }

        /// <summary>
        /// 	Gets or sets the Date/Time of the date.
        /// </summary>
        ///
        /// <value>
        /// 	The date.
        /// </value>

        public DateTime Date { get; set; }

        /// <summary>
        /// Initializes the object
        /// </summary>

        public void Initialize()
        {
            Operation = OperationManager.Operation;
            TriggeredBy = OperationManager.Execution;
            Host = EnvironmentHelper.Host;
            Date = DateTime.Now;
        }

        #endregion
    }
}
