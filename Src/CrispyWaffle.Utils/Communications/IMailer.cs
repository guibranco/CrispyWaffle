namespace CrispyWaffle.Utils.Communications
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;
    using System.Threading.Tasks;

    public interface IMailer : IDisposable
    {
        /// <summary>
        /// Sets the subject.
        /// </summary>
        /// <param name="subject">The subject.</param>
        void SetSubject(string subject);

        /// <summary>
        /// Sets the recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        void SetRecipients(Dictionary<string, string> recipients);

        /// <summary>
        /// Sets the message body
        /// </summary>
        /// <param name="plainTextMessage"></param>
        /// <param name="htmlMessage"></param>
        void SetMessageBody(string plainTextMessage, string htmlMessage);

        /// <summary>
        /// Sets the read notification to.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="emailAddress">The email address.</param>
        void SetReadNotificationTo(string name, string emailAddress);

        /// <summary>
        /// Sets the priority.
        /// </summary>
        /// <param name="priority">The priority.</param>
        void SetPriority(MailPriority priority);

        /// <summary>
        /// Sets the reply to.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="emailAddress">The email address.</param>
        void SetReplyTo(string name, string emailAddress);

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <returns>Task.</returns>
        Task SendAsync();

        /// <summary>
        /// Adds the attachment.
        /// </summary>
        /// <param name="attachment">The attachment.</param>
        void AddAttachment(Attachment attachment);

    }
}
