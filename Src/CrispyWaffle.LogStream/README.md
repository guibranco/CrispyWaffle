# CrispyWaffle.LogStream

[![NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.LogStream.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.LogStream/) [![NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.LogStream.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.LogStream/) [![GitHub license](https://img.shields.io/github/license/guibranco/CrispyWaffle)](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE)

Part of [Crispy Waffle](https://github.com/guibranco/CrispyWaffle), a toolkit for .NET projects.

CrispyWaffle.LogStream sends CrispyWaffle log messages to a [LogStream](https://github.com/guibranco/logstream-server) server. Use it when your services already report to LogStream and you want messages written through `LogConsumer` to show up there as structured entries, with their category, level, host details, exception chain and trace ID.

| | |
|---|---|
| **Package** | `CrispyWaffle.LogStream` |
| **Namespace** | `CrispyWaffle.LogStream`, `CrispyWaffle.LogStream.Log` |
| **Install** | `dotnet add package CrispyWaffle.LogStream` |
| **Depends on** | `CrispyWaffle`, `CrispyWaffle.Configuration`, `Newtonsoft.Json` |

## Supported frameworks

This package targets **.NET Standard 2.0**, **.NET Standard 2.1**, **.NET 8.0**, **.NET 9.0** and **.NET 10.0**.

## Getting started

```csharp
using CrispyWaffle.Log;
using CrispyWaffle.LogStream;
using CrispyWaffle.LogStream.Log;

var options = new LogStreamOptions
{
    ServerUrl = new Uri("https://logs.example.com"),
    ApiKey = "billing-api",
    ApiToken = Environment.GetEnvironmentVariable("LOGSTREAM_TOKEN"),
    AppId = "production",
    OnError = e => Console.Error.WriteLine(e.Message),
};
options.StaticContext["region"] = "eu-west-1";

var provider = new LogStreamLogProvider(options);
LogConsumer.AddProvider(provider).SetLevel(LogLevel.Production);

LogConsumer.Info("Application started");

// On shutdown
provider.Dispose();
```

With a CrispyWaffle `Connection`, the host (or a full URL) and port give the server address. A user name and password are sent as `X-Api-Key` and `X-Api-Token`; a password alone is the shared `API_SECRET`:

```csharp
var connection = new Connection
{
    Host = "logs.example.com",
    Port = 443,
    Credentials = new Credentials { Username = "billing-api", Password = token },
};

var connector = new LogStreamConnector(connection, appKey: null, appId: "production");
LogConsumer.AddProvider(new LogStreamLogProvider(connector)).SetLevel(LogLevel.All);
```

To use an `HttpClient` from `IHttpClientFactory` (or a test handler), pass it to `new LogStreamConnector(options, httpClient)`. The connector does not dispose a client it did not create.

## Documentation

The full reference and more examples for this package are on the [CrispyWaffle.LogStream documentation page](https://guibranco.github.io/CrispyWaffle/packages/logstream/).

- [Crispy Waffle documentation](https://guibranco.github.io/CrispyWaffle/)
- [Changelog](https://guibranco.github.io/CrispyWaffle/changelog/)
- [Source code](https://github.com/guibranco/CrispyWaffle)
- [Report an issue](https://github.com/guibranco/CrispyWaffle/issues/new)

## License

Released under the [MIT License](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE).
