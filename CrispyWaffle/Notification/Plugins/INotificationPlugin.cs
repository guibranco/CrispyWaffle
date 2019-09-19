namespace CrispyWaffle.Notification.Plugins
{

    /// <summary>
    /// The notification plugin interface.
    /// </summary>
    public interface IMailNotificationPlugin
    {
        /// <summary>
        /// Does the work.
        /// </summary>
        /// <param name="mailer">The mailer.</param>
        /// <param name="subject">The notification subject.</param>
        /// <param name="message">The notification message.</param>
        void DoWork(IMailer mailer, string subject, string message);
    }
}
