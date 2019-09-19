namespace CrispyWaffle.Notification.Model
{
    /// <summary>
    /// A notification receiver interface
    /// </summary>
    public interface INotificationReceiver
    {
        /// <summary>
        /// The name of the receiver
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The user name of the receiver (e-mail, login, username, identifier)
        /// </summary>
        string UserName { get; set; }
    }
}
