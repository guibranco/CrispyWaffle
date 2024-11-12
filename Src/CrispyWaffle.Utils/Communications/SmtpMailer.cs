using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CrispyWaffle.Cache;
using CrispyWaffle.Configuration;
using CrispyWaffle.Extensions;
using CrispyWaffle.Log;
using CrispyWaffle.Log.Providers;
using CrispyWaffle.Telemetry;
using CrispyWaffle.Utils.Extensions;
using CrispyWaffle.Utils.GoodPractices;

namespace CrispyWaffle.Utils.Communications
{
    /// <summary>
    /// Represents an SMTP mailer that facilitates the sending of emails using an SMTP server.
    /// </summary>
    /// <seealso cref="IMailer"/>
    [ConnectionName("SMTP")]
    public class SmtpMailer : IMailer
    {
        /// <summary>
        /// The <see cref="SmtpClient"/> used for sending emails.
        /// </summary>
        private readonly SmtpClient _client;

        /// <summary>
        /// The <see cref="MailMessage"/> to be sent via the <see cref="SmtpClient"/>.
        /// </summary>
        private readonly MailMessage _message;

        /// <summary>
        /// Flag indicating whether the mailer has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Flag indicating whether the message has been set.
        /// </summary>
        private bool _messageSet;

        /// <summary>
        /// The HTML body content of the email message.
        /// </summary>
        private string _htmlMessage;

        /// <summary>
        /// Configuration options for the <see cref="SmtpMailer"/>.
        /// </summary>
        private readonly SmtpMailerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpMailer"/> class with the specified connection and options.
        /// </summary>
        /// <param name="connection">The SMTP connection details.</param>
        /// <param name="options">The SMTP mailer options.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="connection"/> or <paramref name="options"/> is <see langword="null"/>.
        /// </exception>
        public SmtpMailer(IConnection connection, SmtpMailerOptions options)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            _options = options ?? throw new ArgumentNullException(nameof(options));

            _client = new()
            {
                Host = connection.Host,
                Port = connection.Port,
                Credentials = new NetworkCredential(
                    connection.Credentials.Username,
                    connection.Credentials.Password
                ),
                Timeout = 300000,
                EnableSsl = true,
            };
            _message = new() { From = new(_options.FromAddress, _options.FromName) };
            _disposed = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpMailer"/> class using simple connection parameters.
        /// </summary>
        /// <param name="host">The SMTP server host.</param>
        /// <param name="port">The SMTP server port.</param>
        /// <param name="userName">The username for SMTP authentication.</param>
        /// <param name="password">The password for SMTP authentication.</param>
        /// <param name="senderDisplayName">The display name of the sender.</param>
        /// <param name="senderEmailAddress">The sender's email address.</param>
        public SmtpMailer(
            string host,
            int port,
            string userName,
            string password,
            string senderDisplayName,
            string senderEmailAddress
        )
            : this(
                new Connection
                {
                    Credentials = new Credentials { Password = password, Username = userName },
                    Host = host,
                    Port = port,
                },
                new() { FromAddress = senderEmailAddress, FromName = senderDisplayName }
            ) { }

        /// <summary>
        /// Finalizer for the <see cref="SmtpMailer"/> class.
        /// Ensures that resources are properly cleaned up when the object is collected by the garbage collector.
        /// </summary>
        ~SmtpMailer() => Dispose(false);

        /// <summary>
        /// Releases both managed and unmanaged resources used by the <see cref="SmtpMailer"/>.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _message?.Dispose();
                _client?.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// Releases the resources used by the <see cref="SmtpMailer"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sets the recipient of the email message.
        /// </summary>
        /// <param name="toName">The name of the recipient.</param>
        /// <param name="toEmailAddress">The email address of the recipient.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="toEmailAddress"/> is <see langword="null"/> or empty.
        /// </exception>
        private void SetRecipient(string toName, string toEmailAddress)
        {
            if (string.IsNullOrWhiteSpace(toEmailAddress))
            {
                throw new ArgumentNullException(
                    nameof(toEmailAddress),
                    "The receiver's e-mail address cannot be null or empty."
                );
            }

            _message.To.Add(new MailAddress(toEmailAddress, toName));
        }

        /// <summary>
        /// Sets the body content of the email message.
        /// </summary>
        /// <param name="plainTextMessage">The plain text message body.</param>
        /// <param name="htmlMessage">The HTML message body.</param>
        /// <exception cref="CrispyWaffle.Utils.GoodPractices.MessageException">
        /// Thrown when attempting to set the message body after it has already been set.
        /// </exception>
        public void SetMessageBody(string plainTextMessage, string htmlMessage)
        {
            if (_messageSet)
            {
                throw new MessageException();
            }

            _message.Body = plainTextMessage;
            _message.IsBodyHtml = false;
            _message.BodyEncoding = Encoding.UTF8;
            _messageSet = true;
            if (string.IsNullOrWhiteSpace(htmlMessage))
            {
                return;
            }

            _message.AlternateViews.Add(
                AlternateView.CreateAlternateViewFromString(
                    htmlMessage,
                    Encoding.UTF8,
                    MediaTypeNames.Text.Html
                )
            );
            _htmlMessage = htmlMessage;
        }

        /// <summary>
        /// Adds an attachment to the email message.
        /// </summary>
        /// <param name="attachment">The attachment to add to the message.</param>
        /// <exception cref="NullMessageException">
        /// Thrown when the message body has not been set before attempting to add an attachment.
        /// </exception>
        public void AddAttachment(Attachment attachment)
        {
            if (!_messageSet)
            {
                throw new NullMessageException();
            }

            if (_htmlMessage.Length > 150_000)
            {
                return;
            }

            try
            {
                _message.Attachments.Add(attachment);
            }
            catch (Exception e)
            {
                LogConsumer.Handle(e);
            }
        }

        /// <summary>
        /// Sets the subject of the email message.
        /// </summary>
        /// <param name="subject">The subject of the email message.</param>
        public void SetSubject(string subject)
        {
            _message.Subject = subject;
            _message.SubjectEncoding = Encoding.UTF8;
        }

        /// <summary>
        /// Sets the reply-to address for the email message.
        /// </summary>
        /// <param name="name">The name of the reply-to person.</param>
        /// <param name="emailAddress">The email address to use for replies.</param>
        public void SetReplyTo(string name, string emailAddress)
        {
            _message.ReplyToList.Add(new MailAddress(emailAddress, name));
            _message.Headers.Add(@"Return-path", $@"{name} <{emailAddress}>");
        }

        /// <summary>
        /// Sets the read receipt notification address for the email message.
        /// </summary>
        /// <param name="name">The name of the recipient to notify upon read receipt.</param>
        /// <param name="emailAddress">The email address to notify upon read receipt.</param>
        public void SetReadNotificationTo(string name, string emailAddress) =>
            _message.Headers.Add(@"Disposition-Notification-To", $@"{name} <{emailAddress}>");

        /// <summary>
        /// Sets multiple recipients for the email message.
        /// </summary>
        /// <param name="recipients">A dictionary of recipient names and email addresses.</param>
        public void SetRecipients(Dictionary<string, string> recipients)
        {
            if (recipients == null)
            {
                return;
            }

            foreach (var receiver in recipients)
            {
                SetRecipient(receiver.Key, receiver.Value);
            }
        }

        /// <summary>
        /// Sets the priority of the email message.
        /// </summary>
        /// <param name="priority">The priority level of the email message.</param>
        public void SetPriority(MailPriority priority) => _message.Priority = priority;

        /// <summary>
        /// Sends the email message asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendAsync()
        {
            var cacheKey = TypeExtensions.GetCallingMethod();

            string eml;

            using (var sr = new StreamReader(_message.RawMessage()))
            {
                eml = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            var date = DateTime.Now.ToString(
                @"yyyy-MM-dd HH.mm.ss.ffffff",
                CultureInfo.InvariantCulture
            );

            if (_options.EnableDebug)
            {
                LogConsumer.DebugTo<TextFileLogProvider>(
                    eml,
                    $@"{_message.Subject} {date}.{Guid.NewGuid()}.eml"
                );
            }

            if (_options.IsSandbox)
            {
                LogConsumer.Trace("E-mail sending disabled due {0}", "test environment");
                return;
            }

            try
            {
                await SendInternalAsync(cacheKey).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (!HandleExtension(e, cacheKey))
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Sends the email internally asynchronously.
        /// </summary>
        /// <param name="cacheKey">The cache key used to prevent multiple sends.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task SendInternalAsync(string cacheKey)
        {
            if (CacheManager.TryGet(cacheKey, out bool exists) && exists)
            {
                LogConsumer.Trace("E-mail sending disabled due {0}", "network error");
                return;
            }

            var receivers = _message.To.Select(d => d).ToList();
            _message.CC.ToList().ForEach(receivers.Add);

            LogConsumer.Trace(
                "Sending email with subject {0} to the following recipients: {1}",
                _message.Subject,
                string.Join(@",", receivers.Select(d => d.DisplayName))
            );

            await _client.SendMailAsync(_message).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles exceptions that occur during email sending.
        /// </summary>
        /// <param name="e">The exception that occurred.</param>
        /// <param name="cacheKey">The cache key to prevent repeated failures.</param>
        /// <returns><see langword="true"/> if the exception was handled; otherwise, <see langword="false"/>.</returns>
        private static bool HandleExtension(Exception e, string cacheKey)
        {
            TelemetryAnalytics.TrackMetric("SMTPError", e.Message);
            if (
                e.InnerException?.InnerException is SocketException
                || e.Message.IndexOf(@"4.7.1", StringComparison.InvariantCultureIgnoreCase) != -1
                || e.Message.IndexOf(@"5.0.3", StringComparison.InvariantCultureIgnoreCase) != -1
            )
            {
                CacheManager.Set(true, cacheKey, new TimeSpan(0, 15, 0));
                return true;
            }

            if (
                e.Message.IndexOf(@"4.4.2", StringComparison.InvariantCultureIgnoreCase) != -1
                || e.Message.IndexOf(@"4.7.0", StringComparison.InvariantCultureIgnoreCase) != -1
            )
            {
                return false;
            }

            LogConsumer.Handle(e);
            return true;
        }
    }
}
