# CrispyWaffle.Utils

[![NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Utils.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Utils/) [![NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Utils.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Utils/) [![GitHub license](https://img.shields.io/github/license/guibranco/CrispyWaffle)](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE)

Part of [Crispy Waffle](https://github.com/guibranco/CrispyWaffle), a toolkit for .NET projects.

CrispyWaffle.Utils contains communication helpers: an FTP client for uploading files and managing a remote directory, and an SMTP mailer for sending plain-text and HTML e-mail with attachments. It also has an extension that renders a `MailMessage` as raw EML and the exceptions these helpers throw.

`SmtpMailer` currently sends only when the application runs on .NET Framework (through the `netstandard2.0` build). On .NET (Core) `SendAsync` throws before sending; see [SMTP mailer](https://guibranco.github.io/CrispyWaffle/packages/utils/#smtp-mailer).

| | |
|---|---|
| **Package** | `CrispyWaffle.Utils` |
| **Namespace** | `CrispyWaffle.Utils.Communications`, `CrispyWaffle.Utils.Extensions`, `CrispyWaffle.Utils.GoodPractices` |
| **Install** | `dotnet add package CrispyWaffle.Utils` |
| **Depends on** | `CrispyWaffle`, `CrispyWaffle.Configuration` (no third-party package; uses `System.Net` FTP and SMTP classes) |

## Supported frameworks

This package targets **.NET Standard 2.0**, **.NET Standard 2.1**, **.NET 8.0**, **.NET 9.0** and **.NET 10.0**.

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

## Documentation

The full reference and more examples for this package are on the [CrispyWaffle.Utils documentation page](https://guibranco.github.io/CrispyWaffle/packages/utils/).

- [Crispy Waffle documentation](https://guibranco.github.io/CrispyWaffle/)
- [Changelog](https://guibranco.github.io/CrispyWaffle/changelog/)
- [Source code](https://github.com/guibranco/CrispyWaffle)
- [Report an issue](https://github.com/guibranco/CrispyWaffle/issues/new)

## License

Released under the [MIT License](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE).
