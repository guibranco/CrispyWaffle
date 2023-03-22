namespace CrispyWaffle.Utils.Communications
{
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

    /// <summary>
    /// The SMTP mailer class.
    /// </summary>
    /// <seealso cref="IMailer" />
    [ConnectionName("SMTP")]
    public class SmtpMailer : IMailer
    {
        #region Private fields

        /// <summary>
        /// The <see cref="SmtpClient" />
        /// </summary>

        private readonly SmtpClient _client;

        /// <summary>
        /// The <see cref="MailMessage" /> to be sent by the <see cref="SmtpClient" />
        /// </summary>

        private readonly MailMessage _message;

        /// <summary>
        /// True if disposed.
        /// </summary>

        private bool _disposed;

        /// <summary>
        /// True if message is already defined
        /// </summary>

        private bool _messageSet;

        /// <summary>
        /// The HTML version of the message
        /// </summary>

        private string _htmlMessage;

        /// <summary>
        /// The options.
        /// </summary>
        private readonly SmtpMailerOptions _options;

        #endregion

        #region ~Ctor

        /// <summary>
        /// Initializes a new instance of Mailer class.
        /// </summary>
        /// <param name="connection"><see cref="IConnection" /></param>
        /// <param name="options"><see cref="SmtpMailerOptions" /></param>
        /// <exception cref="System.ArgumentNullException">connection</exception>
        /// <exception cref="System.ArgumentNullException">options</exception>

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
                Credentials = new NetworkCredential(connection.Credentials.UserName, connection.Credentials.Password),
                Timeout = 300000
            };
            _message = new() { From = new(_options.FromAddress, _options.FromName) };
            _disposed = false;
        }


        /// <summary>
        /// Initializes a new instance of Mailer class.
        /// </summary>
        /// <param name="host">The SMTP server address (IP or hostname)</param>
        /// <param name="port">The SMTP server port</param>
        /// <param name="userName">The SMTP username (or e-mail address to authenticate)</param>
        /// <param name="password">The password of the <paramref name="userName" /> to connect on SMTP server</param>
        /// <param name="senderDisplayName">The sender's display name</param>
        /// <param name="senderEmailAddress">The sender's e-mail address</param>

        public SmtpMailer(string host,
                  int port,
                  string userName,
                  string password,
                  string senderDisplayName,
                  string senderEmailAddress)
        : this(new Connection
        {
            Credentials = new Credentials { Password = password, UserName = userName },
            Host = host,
            Port = port
        }, new() { FromAddress = senderEmailAddress, FromName = senderDisplayName })
        { }

        /// <summary>
        /// Finalizes an instance of the <see cref="SmtpMailer" /> class.
        /// </summary>
        ~SmtpMailer() => Dispose(false);

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged
        /// resources.</param>
        private void Dispose(bool disposing)
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

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Sets the recipient.
        /// </summary>
        /// <param name="toName">To name.</param>
        /// <param name="toEmailAddress">To email address.</param>
        /// <exception cref="System.ArgumentNullException">emailAddress</exception>
        private void SetRecipient(string toName, string toEmailAddress)
        {
            if (string.IsNullOrWhiteSpace(toEmailAddress))
            {
                throw new ArgumentNullException(nameof(toEmailAddress), "The receiver's e-mail address cannot be null");
            }

            _message.To.Add(new MailAddress(toEmailAddress, toName));
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Sets the message body
        /// </summary>
        /// <param name="plainTextMessage">The plain text message.</param>
        /// <param name="htmlMessage">The HTML message.</param>
        /// <exception cref="MessageException"></exception>

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

            _message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlMessage, Encoding.UTF8, MediaTypeNames.Text.Html));
            _htmlMessage = htmlMessage;
        }

        public void AddAttachment(Attachment attachment)
        {
            if (!_messageSet)
            {
                throw new NullMessageException();
            }

            if (_htmlMessage.Length > 150000)
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
        /// Sets the subject.
        /// </summary>
        /// <param name="subject">The subject.</param>

        public void SetSubject(string subject)
        {
            _message.Subject = subject;
            _message.SubjectEncoding = Encoding.UTF8;
        }

        /// <summary>
        /// Sets the reply to.
        /// </summary>
        /// <param name="name">Name of the reply.</param>
        /// <param name="emailAddress">The reply email address.</param>

        public void SetReplyTo(string name, string emailAddress)
        {
            _message.ReplyToList.Add(new MailAddress(emailAddress, name));
            _message.Headers.Add(@"Return-path", $@"{name} <{emailAddress}>");
        }

        /// <summary>
        /// Sets the read notification to.
        /// </summary>
        /// <param name="name">To name.</param>
        /// <param name="emailAddress">To email address.</param>

        public void SetReadNotificationTo(string name, string emailAddress) => 
            _message.Headers.Add(@"Disposition-Notification-To", $@"{name} <{emailAddress}>");

        /// <summary>
        /// Sets the recipients.
        /// </summary>
        /// <param name="recipients">To list.</param>

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
        /// Sets the priority.
        /// </summary>
        /// <param name="priority">The priority.</param>

        public void SetPriority(MailPriority priority) => _message.Priority = priority;

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <returns>Task.</returns>

        public async Task SendAsync()
        {
            var cacheKey = TypeExtensions.GetCallingMethod();

            string eml;

            using (var sr = new StreamReader(_message.RawMessage()))
            {
                eml = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            var date = DateTime.Now.ToString(@"yyyy-MM-dd HH.mm.ss.ffffff", CultureInfo.InvariantCulture);

            if (_options.EnableDebug)
            {
                LogConsumer.DebugTo<TextFileLogProvider>(eml, $@"{_message.Subject} {date}.{Guid.NewGuid()}.eml");
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
        /// send internal as an asynchronous operation.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task SendInternalAsync(string cacheKey)
        {
            if (CacheManager.TryGet(cacheKey, out bool exists) && exists)
            {
                LogConsumer.Trace("E-mail sending disabled due {0}", "network error");
                return;
            }

            var receivers = _message.To.Select(d => d).ToList();
            _message.CC.ToList().ForEach(receivers.Add);

            LogConsumer.Trace("Sending email with subject {0} to the following recipients: {1}",
                _message.Subject,
                string.Join(@",", receivers.Select(d => d.DisplayName)));

            await _client.SendMailAsync(_message).ConfigureAwait(false);
        }


        /// <summary>
        /// Handles the extension.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="cacheKey">The cache key.</param>
        /// <returns><c>true</c> if handled, <c>false</c> otherwise.</returns>
        private static bool HandleExtension(Exception e, string cacheKey)
        {
            TelemetryAnalytics.TrackMetric("SMTPError", e.Message);
            if (e.InnerException?.InnerException is SocketException ||
                e.Message.IndexOf(@"4.7.1", StringComparison.InvariantCultureIgnoreCase) != -1 ||
                e.Message.IndexOf(@"5.0.3", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                CacheManager.Set(true, cacheKey, new TimeSpan(0, 15, 0));
                return true;
            }

            if (e.Message.IndexOf(@"4.4.2", StringComparison.InvariantCultureIgnoreCase) != -1 ||
                e.Message.IndexOf(@"4.7.0", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                return false;
            }

            LogConsumer.Handle(e);
            return true;
        }

        #endregion
    }
}
