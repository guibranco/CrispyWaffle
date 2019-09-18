namespace CrispyWaffle.Notification
{
    using System;

    /// <summary>
    /// Class NotificationTypeAttribute. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public sealed class NotificationTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationTypeAttribute"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public NotificationTypeAttribute(MailNotificationType type)
        {
            Type = type;
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>

        public MailNotificationType Type { get; private set; }
    }
}
