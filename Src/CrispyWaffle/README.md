# CrispyWaffle

[![NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle/) [![NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle/) [![GitHub license](https://img.shields.io/github/license/guibranco/CrispyWaffle)](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE)

The core package of [Crispy Waffle](https://github.com/guibranco/CrispyWaffle), a toolkit for .NET projects.

| | |
|---|---|
| **Package** | `CrispyWaffle` |
| **Install** | `dotnet add package CrispyWaffle` |
| **Depends on** | `Newtonsoft.Json`, `System.Text.Json`, `System.Security.Permissions`, `Tomlyn`, `YamlDotNet` |

## Features

- **Service locator**: a lightweight IoC container with lifetimes and bootstrappers.
- **Logging**: the `LogConsumer` facade with filters, multiple providers and exception handlers.
- **Caching**: the async `CacheManager` facade over prioritized cache repositories.
- **Events and commands**: one-to-many event dispatch and one-to-one command dispatch.
- **Scheduler**: cron-based job scheduling with `JobManager`.
- **Serialization**: JSON (Newtonsoft.Json and System.Text.Json), XML, YAML and TOML adapters behind `SerializerFactory`.
- **Cryptography**: AES string encryption and MD5, SHA-1, SHA-2 and SHA-3 hashing.
- **Template rendering, telemetry, validations and extension methods.**

## Supported frameworks

This package targets **.NET Standard 2.0**, **.NET Standard 2.1**, **.NET 8.0**, **.NET 9.0** and **.NET 10.0**.

## Getting started

CrispyWaffle features are exposed as static facades (`LogConsumer`, `CacheManager`, `EventsConsumer`, `CommandsConsumer`). They find their collaborators through the `ServiceLocator`, so register those first.

```csharp
using CrispyWaffle.Cache;
using CrispyWaffle.Composition;
using CrispyWaffle.Log;
using CrispyWaffle.Log.Adapters;
using CrispyWaffle.Log.Handlers;
using CrispyWaffle.Log.Providers;

ServiceLocator.Register<IConsoleLogAdapter, StandardConsoleLogAdapter>(Lifetime.Singleton);
ServiceLocator.Register<IExceptionHandler, NullExceptionHandler>(Lifetime.Singleton);

// Logging: every message goes to every provider.
LogConsumer.AddProvider<ConsoleLogProvider>().SetLevel(LogLevel.All);
LogConsumer.Info("Application started");

// Caching: writes go to every repository; reads return the first hit in priority order.
CacheManager.AddRepository<MemoryCacheRepository>();
await CacheManager.SetAsync("Crispy Waffle", "app-name");
var name = await CacheManager.GetAsync<string>("app-name");
```

Events and commands are plain classes implementing `IEvent` / `ICommand`, with handlers discovered through the `ServiceLocator`:

```csharp
using CrispyWaffle.Events;

public sealed class UserCreatedEvent : IEvent
{
    public UserCreatedEvent(string email) => Email = email;

    public string Email { get; }
}

public sealed class SendWelcomeEmail : IEventHandler<UserCreatedEvent>
{
    public void Handle(UserCreatedEvent args) =>
        LogConsumer.Info("Sending welcome e-mail to {0}", args.Email);
}

EventsConsumer.Raise(new UserCreatedEvent("someone@example.com"));
```

See the [getting started guide](https://guibranco.github.io/CrispyWaffle/getting-started/) for the full walkthrough.

## Integration packages

| Package | Purpose |
|---|---|
| [CrispyWaffle.Configuration](https://www.nuget.org/packages/CrispyWaffle.Configuration/) | Connection and credential types shared by the connectors. |
| [CrispyWaffle.Redis](https://www.nuget.org/packages/CrispyWaffle.Redis/) | Redis cache repository, Pub/Sub log provider and telemetry client. |
| [CrispyWaffle.RabbitMQ](https://www.nuget.org/packages/CrispyWaffle.RabbitMQ/) | RabbitMQ messaging and log provider. |
| [CrispyWaffle.ElasticSearch](https://www.nuget.org/packages/CrispyWaffle.ElasticSearch/) | Elasticsearch log provider and index helpers. |
| [CrispyWaffle.Log4Net](https://www.nuget.org/packages/CrispyWaffle.Log4Net/) | Bridge from `LogConsumer` to log4net. |
| [CrispyWaffle.LogStream](https://www.nuget.org/packages/CrispyWaffle.LogStream/) | Log provider for a LogStream server. |
| [CrispyWaffle.EventLog](https://www.nuget.org/packages/CrispyWaffle.EventLog/) | Windows Event Log provider. |
| [CrispyWaffle.Elmah](https://www.nuget.org/packages/CrispyWaffle.Elmah/) | ELMAH exception handler. |
| [CrispyWaffle.Utils](https://www.nuget.org/packages/CrispyWaffle.Utils/) | FTP client and SMTP mailer. |
| [CrispyWaffle.I18n.PtBr](https://www.nuget.org/packages/CrispyWaffle.I18n.PtBr/) | Brazilian Portuguese conversions (phone numbers, documents, zip codes). |

## Documentation

- [Crispy Waffle documentation](https://guibranco.github.io/CrispyWaffle/)
- [Changelog](https://guibranco.github.io/CrispyWaffle/changelog/)
- [Source code](https://github.com/guibranco/CrispyWaffle)
- [Report an issue](https://github.com/guibranco/CrispyWaffle/issues/new)

## License

Released under the [MIT License](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE).
