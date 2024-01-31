using System;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Reflection;

namespace CrispyWaffle.Utils.Extensions
{
    /// <summary>
    /// Class MailMessageExtensions.
    /// </summary>
    public static class MailMessageExtensions
    {
        /// <summary>
        /// The flags
        /// </summary>
#pragma warning disable S3011
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

        /// <summary>
        /// The mail writer
        /// </summary>
        private static readonly Type _mailWriter = typeof(SmtpClient).Assembly.GetType(
            @"System.Net.Mail.MailWriter"
        );

        /// <summary>
        /// The mail writer constructor
        /// </summary>
        private static readonly ConstructorInfo _mailWriterConstructor = _mailWriter.GetConstructor(
            Flags,
            null,
            new[] { typeof(Stream) },
            null
        );

        /// <summary>
        /// The close method
        /// </summary>
        private static readonly MethodInfo _closeMethod = _mailWriter.GetMethod(@"Close", Flags);

        /// <summary>
        /// The send method
        /// </summary>
        private static readonly MethodInfo _sendMethod = typeof(MailMessage).GetMethod(
            @"Send",
            Flags
        );

        /// <summary>
        /// A little hack to determine the number of parameters that we
        /// need to pass to the SaveMethod.
        /// </summary>
        private static readonly bool _isRunningInDotNetFourPointFive =
            _sendMethod.GetParameters().Length == 3;

        /// <summary>
        /// The raw contents of this MailMessage as a MemoryStream.
        /// </summary>
        /// <param name="self">The caller.</param>
        /// <returns>A MemoryStream with the raw contents of this MailMessage.</returns>
        public static MemoryStream RawMessage(this MailMessage self)
        {
            var result = new MemoryStream();
            var mailWriter = _mailWriterConstructor.Invoke(new object[] { result });
            _sendMethod.Invoke(
                self,
                Flags,
                null,
                _isRunningInDotNetFourPointFive
                    ? new[] { mailWriter, true, true }
                    : new[] { mailWriter, true },
                CultureInfo.InvariantCulture
            );
            result = new(result.ToArray());
            _closeMethod.Invoke(
                mailWriter,
                Flags,
                null,
                Array.Empty<object>(),
                CultureInfo.InvariantCulture
            );
            return result;
        }
    }
}
