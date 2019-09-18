namespace CrispyWaffle.Extensions
{
    using Composition;
    using Notification;
    using Notification.Model;
    using Serialization;
    using System;
    using System.Diagnostics.Contracts;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
    using TemplateRendering.Repositories;

    /// <summary>
    /// The mail notification extensions class.
    /// </summary>
    public static class MailNotificationExtensions
    {
        /// <summary>
        /// Gets the notification type.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <returns></returns>
        [Pure]
        public static MailNotificationType GetNotificationType(this INotification notification)
        {
            return !(notification.GetType().GetCustomAttribute(typeof(NotificationTypeAttribute), false) is
                         NotificationTypeAttribute attribute)
                       ? MailNotificationType.NONE
                       : attribute.Type;
        }

        /// <summary>
        /// Gets the notification template.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="repository">The repository.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">type</exception>
        [Pure]
        public static string GetNotificationTemplate(this MailNotificationType type, ITemplateRepository repository)
        {
            var reportName = type.GetInternalValue();
            if (string.IsNullOrWhiteSpace(reportName))
                throw new ArgumentException($"Unable to get the template for the notification {type.GetHumanReadableValue()}", nameof(type));
            return repository.GetTemplateByName(reportName);
        }

        /// <summary>
        /// Gets the mail receivers.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        [Pure]
        public static INotificationReceiver[] GetMailReceivers(this MailNotificationType type)
        {
            return ApplicationContext.Configuration
                                     .EmailNotifications
                                     .SingleOrDefault(n => n.Type == type)
                                     ?.Receivers ?? new INotificationReceiver[] { };
        }

        /// <summary>
        /// Gets the mail reply to.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        [Pure]
        public static INotificationReceiver GetMailReplyTo(this MailNotificationType type)
        {
            return ApplicationContext.Configuration.EmailNotifications.SingleOrDefault(n => n.Type == type)?.ReplyTo;

        }

        /// <summary>
        /// Gets the mail message.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <param name="type">The type.</param>
        /// <param name="repository">The repository.</param>
        /// <returns></returns>
        [Pure]
        public static string GetMailMessage(
            this INotification notification,
            MailNotificationType type,
            ITemplateRepository repository)
        {
            var template = type.GetNotificationTemplate(repository);
            XmlDocument xml = notification.GetSerializer();
            var data = XElement.Parse(xml.InnerXml);
            return ServiceLocator.Resolve<XmlTemplateRender>().Render(template, data);
        }
    }
}
