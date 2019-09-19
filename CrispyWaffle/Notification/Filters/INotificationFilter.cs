namespace CrispyWaffle.Notification.Filters
{
    /// <summary>
    /// The notification filter interface.
    /// </summary>
    public interface INotificationFilter
    {
        /// <summary>
        /// Filters the specified provider type.
        /// </summary>
        /// <param name="providerType">Type of the provider.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        bool Filter(string providerType, MailNotificationType type);
    }
}
