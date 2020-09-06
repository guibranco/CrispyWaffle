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

namespace CrispyWaffle.Utils.Communications
{
    /// <summary>
    /// The SMTP mailer class.
    /// </summary>
    /// <seealso cref="IMailer" />
    [ConnectionName("SMTP")]
    public sealed class SmtpMailer : IMailer
    {
        #region Private fields

        /// <summary>
        /// The <see cref="SmtpClient"/>
        /// </summary>

        private readonly SmtpClient _client;

        /// <summary>
        /// The <see cref="MailMessage"/> to be sent by the <see cref="SmtpClient"/>
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

        #endregion

        #region ~Ctor

        /// <summary>
        /// Initializes a new instance of Mailer class.
        /// </summary>
        /// <param name="connection"><see cref="IConnection"/></param>

        public SmtpMailer(IConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            _client = new SmtpClient
            {
                Host = connection.Host,
                Port = connection.Port,
                Credentials = new NetworkCredential(connection.Credentials.UserName, connection.Credentials.Password),
                Timeout = 300000
            };
            _message = new MailMessage
            {
                From = new MailAddress("integracao@editorainovacao.com.br", "Integração Service")
            };
            _disposed = false;
        }

        /// <summary>
        /// Initializes a new instance of Mailer class.
        /// </summary>
        /// <param name="connection"><see cref="IConnection"/></param>
        /// <param name="from"><see cref="MailAddress"/></param>

        public SmtpMailer(IConnection connection, MailAddress from)
            : this(connection)
        {
            _message = new MailMessage { From = from };
        }

        /// <summary>
        /// Initializes a new instance of Mailer class.
        /// </summary>
        /// <param name="host">The SMTP server address (IP or hostname)</param>
        /// <param name="port">The SMTP server port</param>
        /// <param name="userName">The SMTP username (or e-mail address to authenticate)</param>
        /// <param name="password">The password of the <paramref name="userName"/> to connect on SMTP server</param>
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
        },
               new MailAddress(senderEmailAddress, senderDisplayName))
        { }

        /// <summary>
        /// Finalizes an instance of the <see cref="SmtpMailer"/> class.
        /// </summary>
        ~SmtpMailer()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, 
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        ///
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
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
        /// <remarks>Guilherme B. Stracini, 06/08/2013.</remarks>

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
        /// <exception cref="ArgumentNullException">toEmailAddress</exception>
        private void SetRecipient(string toName, string toEmailAddress)
        {
            if (string.IsNullOrWhiteSpace(toEmailAddress))
            {
                throw new ArgumentNullException(nameof(toEmailAddress), Resources.SmtpMailer_SetRecipient_EmptyReceiverEmailAddress);
            }

            _message.To.Add(new MailAddress(toEmailAddress, toName));
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Sets the message body
        /// </summary>
        /// <param name="plainTextMessage"></param>
        /// <param name="htmlMessage"></param>
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

        /// <summary>
        /// Adds the attachment.
        /// </summary>
        /// <param name="attachment">The attachment.</param>
        /// <exception cref="NullMessageException"></exception>

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
        /// <param name="replyName">Name of the reply.</param>
        /// <param name="replyEmailAddress">The reply email address.</param>

        public void SetReplyTo(string replyName, string replyEmailAddress)
        {
            _message.ReplyToList.Add(new MailAddress(replyEmailAddress, replyName));
            _message.Headers.Add(@"Return-path", $@"{replyName} <{replyEmailAddress}>");
        }

        /// <summary>
        /// Sets the read notification to.
        /// </summary>
        /// <param name="toName">To name.</param>
        /// <param name="toEmailAddress">To email address.</param>

        public void SetReadNotificationTo(string toName, string toEmailAddress)
        {
            _message.Headers.Add(@"Disposition-Notification-To", $@"{toName} <{toEmailAddress}>");
        }

        /// <summary>
        /// Sets the recipients.
        /// </summary>
        /// <param name="toList">To list.</param>

        public void SetRecipients(Dictionary<string, string> toList)
        {
            if (toList == null)
            {
                return;
            }

            foreach (var receiver in toList)
            {
                SetRecipient(receiver.Key, receiver.Value);
            }
        }

        /// <summary>
        /// Sets the priority.
        /// </summary>
        /// <param name="priority">The priority.</param>

        public void SetPriority(MailPriority priority)
        {
            _message.Priority = priority;
        }

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <returns>
        /// Task.
        /// </returns>

        public async Task SendAsync()
        {
            var cacheKey = TypeExtensions.GetCallingMethod();

            string eml;

            using (var sr = new StreamReader(_message.RawMessage()))
            {
                eml = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            var date = DateTime.Now.ToString(@"yyyy-MM-dd HH.mm.ss.ffffff", CultureInfo.InvariantCulture);

            LogConsumer.DebugTo<TextFileLogProvider>(eml, $@"{_message.Subject} {date}.{Guid.NewGuid()}.eml");

            if (OperationManager.IsInTestEnvironment)
            {
                LogConsumer.Trace(Resources.SmtpMailer_SendAsync_Disabled, Resources.SmtpMailer_SendAsync_DisabledDue_TestEnvironment);
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
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private async Task SendInternalAsync(string cacheKey)
        {
            if (CacheManager.TryGet(cacheKey, out bool exists) && exists)
            {
                LogConsumer.Trace(Resources.SmtpMailer_SendAsync_Disabled,
                    Resources.SmtpMailer_SendAsync_DisabledDue_NetworkError);
                return;
            }

            var receivers = _message.To.Select(d => d).ToList();
            _message.CC.ToList().ForEach(receivers.Add);
            LogConsumer.Trace(Resources.SmtpMailer_SendAsync_Sending,
                _message.Subject.Replace(@" - Integração Service", ""),
                string.Join(@",", receivers.Select(d => d.DisplayName)));

            await _client.SendMailAsync(_message).ConfigureAwait(false);
        }


        /// <summary>
        /// Handles the extension.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="cacheKey">The cache key.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
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
