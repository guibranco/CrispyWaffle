# CrispyWaffle.Utils

CrispyWaffle.Utils contains communication helpers: an FTP client for uploading files and managing a remote directory, and an SMTP mailer for sending plain-text and HTML e-mail with attachments. It also has an extension that renders a `MailMessage` as raw EML and the exceptions these helpers throw.

`SmtpMailer` currently sends only when the application runs on .NET Framework (through the `netstandard2.0` build). On .NET (Core) `SendAsync` throws before sending; see [SMTP mailer](#smtp-mailer).

| | |
|---|---|
| **Package** | `CrispyWaffle.Utils` |
| **Namespace** | `CrispyWaffle.Utils.Communications`, `CrispyWaffle.Utils.Extensions`, `CrispyWaffle.Utils.GoodPractices` |
| **Install** | `dotnet add package CrispyWaffle.Utils` |
| **Depends on** | `CrispyWaffle`, `CrispyWaffle.Configuration` (no third-party package; uses `System.Net` FTP and SMTP classes) |

## How it works

### FTP client

`FtpClient` works on one remote directory, set in the constructor. A trailing `/` on the directory is removed. Every URL is built as `ftp://{host}:{port}/{remoteDirectory}/{path}`. Requests use `FtpWebRequest` in passive mode with the configured credentials. Public methods take a lock, so one instance can be shared between threads, but calls run one at a time.

Whether a path is treated as a file or a directory depends on whether it has a file extension. A path with an extension is uploaded or deleted as a file; a path without one is created or removed as a directory.

`Upload` creates the remote directory if it does not exist, deletes an existing file with the same name, and then uploads the bytes. `ClearDirectory` deletes every entry listed in the remote directory, removes the directory, and creates it again empty.

FTP errors are wrapped in `FtpClientException`. An existence check that fails with a `WebException` is treated as "does not exist".

`FtpWebRequest` is marked obsolete in .NET 6 and later, and Microsoft recommends a third-party FTP library for new development.

### SMTP mailer

`SmtpMailer` wraps one `SmtpClient` and one `MailMessage`. Create one instance per e-mail and dispose it after sending. The client always uses SSL and a timeout of 300 seconds. The sender comes from `SmtpMailerOptions.FromAddress` and `FromName`.

Build the message in this order:

1. `SetSubject`, `SetRecipients`, `SetReplyTo`, `SetReadNotificationTo`, `SetPriority` in any order.
2. `SetMessageBody` once. A second call throws `MessageException`.
3. `AddAttachment`, only after `SetMessageBody` with an HTML body; otherwise it throws (see the reference below).
4. `SendAsync`.

The plain-text body is the main body. When an HTML body is given, it is added as a `text/html` alternate view, so mail clients that support HTML show it instead.

`SendAsync` behaves as follows:

- It first renders the message with `MailMessageExtensions.RawMessage`, even when debug is off. That method only works on .NET Framework (see its reference entry), so on .NET (Core) `SendAsync` throws `NullReferenceException` before anything is sent.
- When `SmtpMailerOptions.EnableDebug` is `true`, the raw message is written as an `.eml` file through `LogConsumer.DebugTo<TextFileLogProvider>`, named `{subject} {timestamp}.{guid}.eml`.
- When `SmtpMailerOptions.IsSandbox` is `true`, nothing is sent. A trace entry is written and the method returns.
- Otherwise the message is sent with `SmtpClient.SendMailAsync`.

Send errors are tracked with `TelemetryAnalytics.TrackMetric("SMTPError", message)` and then handled by type:

- Socket errors (an inner-inner `SocketException`) and SMTP replies containing `4.7.1` or `5.0.3` store a flag in `CacheManager` for 15 minutes, keyed by the calling method. While the flag exists, `SendAsync` calls from the same method skip sending without an error. The exception is not rethrown. This only works when a cache repository is registered with `CacheManager`.
- Replies containing `4.4.2` or `4.7.0` are rethrown.
- Any other exception is passed to `LogConsumer.Handle` and not rethrown.

In other words, `SendAsync` completing does not mean the e-mail was delivered.

## Getting started

```csharp
using System.Collections.Generic;
using CrispyWaffle.Utils.Communications;

using var mailer = new SmtpMailer(
    "smtp.example.com",
    587,
    "user@example.com",
    "password",
    "Example App",
    "no-reply@example.com"
);

mailer.SetSubject("Welcome");
mailer.SetRecipients(new Dictionary<string, string> { ["Jane Doe"] = "jane@example.com" });
mailer.SetMessageBody("Welcome to Example App.", "<p>Welcome to <b>Example App</b>.</p>");

await mailer.SendAsync();
```

## Reference

### `SmtpMailer`

`public class SmtpMailer : IMailer` — builds and sends one e-mail through an SMTP server.

The class has `[ConnectionName("SMTP")]`.

| Member | Description |
|---|---|
| `SmtpMailer(IConnection connection, SmtpMailerOptions options)` | Constructor. Takes host, port, user name and password from `connection`. Throws `ArgumentNullException` when `connection` or `options` is `null`. |
| `SmtpMailer(string host, int port, string userName, string password, string senderDisplayName, string senderEmailAddress)` | Constructor. Builds a `Connection` and an `SmtpMailerOptions` (sandbox and debug off) from the arguments. |
| `SetSubject(string subject)` | Sets the subject, UTF-8 encoded. |
| `SetRecipients(Dictionary<string, string> recipients)` | Adds a `To` address for each entry. The key is the display name and the value is the address. Does nothing when `recipients` is `null`. Throws `ArgumentNullException` when an address is null or whitespace. |
| `SetMessageBody(string plainTextMessage, string htmlMessage)` | Sets the plain-text body (UTF-8) and, when `htmlMessage` is not null or whitespace, adds it as a `text/html` alternate view. Throws `MessageException` when called a second time. |
| `AddAttachment(Attachment attachment)` | Adds an attachment. Throws `NullMessageException` when `SetMessageBody` was not called. The attachment is skipped silently when the HTML body is longer than 150,000 characters. Errors while adding are passed to `LogConsumer.Handle`. The HTML-length check dereferences the HTML body, so when `SetMessageBody` was called without an HTML body this method throws `NullReferenceException`. |
| `SetReplyTo(string name, string emailAddress)` | Adds a reply-to address and a `Return-path` header. |
| `SetReadNotificationTo(string name, string emailAddress)` | Adds a `Disposition-Notification-To` header (read receipt request). |
| `SetPriority(MailPriority priority)` | Sets the message priority. |
| `SendAsync()` | Sends the message, honoring `IsSandbox` and `EnableDebug`. See How it works for error handling. Returns `Task`. |
| `Dispose()` | Disposes the message and the SMTP client. |
| `Dispose(bool disposing)` | `protected virtual`. Override to release extra resources in a subclass. |

### `IMailer`

`public interface IMailer : IDisposable` — contract for a mailer. `SmtpMailer` is the only implementation in the package.

| Member | Description |
|---|---|
| `SetSubject(string subject)` | Sets the subject. |
| `SetRecipients(Dictionary<string, string> recipients)` | Adds recipients (name to address). |
| `SetMessageBody(string plainTextMessage, string htmlMessage)` | Sets the plain-text and HTML bodies. |
| `SetReadNotificationTo(string name, string emailAddress)` | Requests a read receipt. |
| `SetPriority(MailPriority priority)` | Sets the priority. |
| `SetReplyTo(string name, string emailAddress)` | Sets the reply-to address. |
| `AddAttachment(Attachment attachment)` | Adds an attachment. |
| `SendAsync()` | Sends the message. Returns `Task`. |

### `SmtpMailerOptions`

`public class SmtpMailerOptions` — sender and behavior settings for `SmtpMailer`.

| Member | Description |
|---|---|
| `FromAddress` | Sender e-mail address. |
| `FromName` | Sender display name. |
| `IsSandbox` | When `true`, `SendAsync` does not send. Default `false`. |
| `EnableDebug` | When `true`, `SendAsync` writes the raw message as an `.eml` file through `TextFileLogProvider`. Default `false`. |

### `FtpClient`

`public class FtpClient` — uploads files to and manages one directory on an FTP server.

| Member | Description |
|---|---|
| `FtpClient(IConnection connection, string remoteDirectory)` | Constructor. Takes host, port (21 when `connection` is `null`), user name and password from `connection`. |
| `FtpClient(string host, int port, string userName, string password, string remoteDirectory)` | Constructor. Removes a trailing `/` from `remoteDirectory`. Throws `ArgumentNullException` when `remoteDirectory` is null or whitespace. |
| `Upload(string fileName, byte[] bytes)` | Creates the remote directory when it does not exist, deletes an existing `fileName`, and uploads `bytes`. Returns `true` when the server confirms the upload. Throws `ArgumentNullException` when an argument is `null`, and `FtpClientException` when the upload fails. |
| `CreateDirectory(string name)` | Creates the subdirectory `name` under the remote directory. Returns `true` when the server confirms it. Throws `FtpClientException` on failure. |
| `Remove(string path)` | Deletes `path` under the remote directory: a file when it has an extension, otherwise a directory. Throws `FtpClientException` when the server does not answer with `FileActionOK` or the request fails. |
| `ClearDirectory()` | Deletes every entry listed in the remote directory, removes the directory, and creates it again. |

### `MailMessageExtensions`

`public static class MailMessageExtensions` — extension methods for `System.Net.Mail.MailMessage`.

| Member | Description |
|---|---|
| `RawMessage(this MailMessage self)` | Returns a `MemoryStream` with the message in RFC 822 (EML) format, positioned at the start. It calls internal `System.Net.Mail` members through reflection and looks for a `MailWriter(Stream)` constructor, which exists only on .NET Framework. On .NET (Core) the constructor is `MailWriter(Stream, bool)`, so the method throws `NullReferenceException`. |

### `FtpClientException`

`public class FtpClientException : Exception` — thrown by `FtpClient` when an FTP operation fails. Marked `[Serializable]`.

| Member | Description |
|---|---|
| `FtpClientException()` | Constructor with the default message. |
| `FtpClientException(string message)` | Constructor with a message. |
| `FtpClientException(string message, Exception innerException)` | Constructor with a message and inner exception. |
| `FtpClientException(string path, string action, FtpStatusCode responseCode)` | Message: `Unable to {action} the path/file {path} in the FtpClient host. Status code: {responseCode}`. |
| `FtpClientException(string path, string action, Exception innerException)` | Same message with status code `0`, and the inner exception. Used when the request throws a `WebException`. |
| `FtpClientException(SerializationInfo info, StreamingContext context)` | `protected`. Deserialization constructor. |

### `MessageException`

`public class MessageException : Exception` — thrown by `SmtpMailer.SetMessageBody` when the body was already set. Message: `The message cannot be set more than once for the same instance of Email class`. Marked `[Serializable]`.

| Member | Description |
|---|---|
| `MessageException()` | Constructor with the default message. |
| `MessageException(SerializationInfo info, StreamingContext context)` | `protected`. Deserialization constructor. |

### `NullMessageException`

`public class NullMessageException : Exception` — thrown by `SmtpMailer.AddAttachment` when the body has not been set. Message: `Unable to set the attachment of message before the message itself`. Marked `[Serializable]`.

| Member | Description |
|---|---|
| `NullMessageException()` | Constructor with the default message. |
| `NullMessageException(SerializationInfo info, StreamingContext context)` | `protected`. Deserialization constructor. |

## Examples

### Send an e-mail with a stored connection and an attachment

Use a `Connection` from configuration and `SmtpMailerOptions` to keep sending off in test environments.

```csharp
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using CrispyWaffle.Configuration;
using CrispyWaffle.Utils.Communications;

var connection = new Connection
{
    Host = "smtp.example.com",
    Port = 587,
    Credentials = { Username = "user@example.com", Password = "password" },
};

var options = new SmtpMailerOptions
{
    FromAddress = "reports@example.com",
    FromName = "Reports",
    IsSandbox = false,
};

using var mailer = new SmtpMailer(connection, options);

mailer.SetSubject("Daily report");
mailer.SetRecipients(new Dictionary<string, string> { ["Ops"] = "ops@example.com" });
mailer.SetReplyTo("Support", "support@example.com");
mailer.SetPriority(MailPriority.High);
mailer.SetMessageBody("The daily report is attached.", "<p>The daily report is attached.</p>");

var csv = new MemoryStream(Encoding.UTF8.GetBytes("date,total\n2024-01-01,10\n"));
mailer.AddAttachment(new Attachment(csv, "report.csv", "text/csv"));

await mailer.SendAsync();
```

### Upload a file over FTP

Upload a file to `/exports` on the server, replacing any file with the same name.

```csharp
using System.IO;
using CrispyWaffle.Utils.Communications;
using CrispyWaffle.Utils.GoodPractices;

var ftp = new FtpClient("ftp.example.com", 21, "user", "password", "exports/");

try
{
    var uploaded = ftp.Upload("report.csv", File.ReadAllBytes("report.csv"));
}
catch (FtpClientException e)
{
    // e.Message: "Unable to create the path/file report.csv in the FtpClient host. Status code: 0"
}
```

### Save a message as an EML file

`RawMessage` returns the message exactly as it would be sent. This works on .NET Framework only.

```csharp
using System.IO;
using System.Net.Mail;
using CrispyWaffle.Utils.Extensions;

using var message = new MailMessage("from@example.com", "to@example.com", "Subject", "Body");
using var raw = message.RawMessage();
using var file = File.Create("message.eml");
raw.CopyTo(file);
```

## See also

- [CrispyWaffle.Configuration](configuration.md)
- [Logging](../core/logging.md)
- [Caching](../core/caching.md)
- [Telemetry](../core/telemetry.md)
