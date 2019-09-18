namespace CrispyWaffle.Notification.Providers
{
    using Model;
    using System;

    /// <summary>
    /// Interface for notification providers
    /// </summary>

    public interface INotificationProvider
    {
        /// <summary>
        /// Notifies the specified receiver.
        /// </summary>
        /// <param name="receiver">The receiver.</param>
        /// <param name="notification">The notification.</param>
        /// <param name="identifier">The identifier.</param>
        void Notify(INotificationReceiver receiver, INotification notification, Guid identifier);

        /// <summary>
        /// Notifies the specified receivers.
        /// </summary>
        /// <param name="receivers">The receivers.</param>
        /// <param name="notification">The notification.</param>
        /// <param name="identifier">The identifier.</param>
        void Notify(INotificationReceiver[] receivers, INotification notification, Guid identifier);

        /// <summary>
        /// Notifies the specified notification.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <param name="identifier">The identifier.</param>
        void Notify(INotification notification, Guid identifier);

    }
}
