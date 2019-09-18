namespace CrispyWaffle.Notification.Model
{
    using Serialization;
    using System;

    /// <summary>
    /// 	Interface for notification.
    /// </summary>
    [Serializer]
    public interface INotification
    {
        /// <summary>
        /// Gets the receivers.
        /// </summary>
        /// <value>
        /// The receivers.
        /// </value>
        INotificationReceiver[] Receivers { get; }

        /// <summary>
        /// 	Gets or sets the operation.
        /// </summary>
        ///
        /// <value>
        /// 	The operation.
        /// </value>
        string Operation { get; }

        /// <summary>
        /// 	Gets or sets the execution trigger
        /// </summary>
        ///
        /// <value>
        /// 	The execution trigger
        /// </value>
        Execution TriggeredBy { get; }

        /// <summary>
        /// 	Gets or sets the host.
        /// </summary>
        ///
        /// <value>
        /// 	The host.
        /// </value>
        string Host { get; }

        /// <summary>
        /// 	Gets or sets the Date/Time of the date.
        /// </summary>
        ///
        /// <value>
        /// 	The date.
        /// </value>
        DateTime Date { get; }

        /// <summary>
        /// Initializes the object
        /// </summary>
        void Initialize();
    }
}
