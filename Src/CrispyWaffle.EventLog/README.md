# CrispyWaffle.EventLog

[![NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.EventLog.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.EventLog/) [![NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.EventLog.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.EventLog/) [![GitHub license](https://img.shields.io/github/license/guibranco/CrispyWaffle)](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE)

Part of [Crispy Waffle](https://github.com/guibranco/CrispyWaffle), a toolkit for .NET projects.

CrispyWaffle.EventLog writes CrispyWaffle log messages to the Windows Event Log. Use it for Windows services and desktop applications whose operators read logs in Event Viewer.

The package uses `System.Diagnostics.EventLog`, which only works on Windows. The library targets .NET Standard and .NET and has no platform annotation, so it builds anywhere, but on Linux and macOS the `System.Diagnostics.EventLog` calls throw `PlatformNotSupportedException`.

| | |
|---|---|
| **Package** | `CrispyWaffle.EventLog` |
| **Namespace** | `CrispyWaffle.EventLog.Log.Providers`, `CrispyWaffle.EventLog.Log.Adapters` |
| **Install** | `dotnet add package CrispyWaffle.EventLog` |
| **Depends on** | `CrispyWaffle`, `System.Diagnostics.EventLog` |

## Supported frameworks

This package targets **.NET Standard 2.0**, **.NET Standard 2.1**, **.NET 8.0**, **.NET 9.0** and **.NET 10.0**.

## Getting started

```csharp
using CrispyWaffle.EventLog.Log.Providers;
using CrispyWaffle.Log;

LogConsumer.AddProvider(new EventLogProvider("my-service")).SetLevel(LogLevel.Production);

LogConsumer.Info("Service started");
```

## Documentation

The full reference and more examples for this package are on the [CrispyWaffle.EventLog documentation page](https://guibranco.github.io/CrispyWaffle/packages/eventlog/).

- [Crispy Waffle documentation](https://guibranco.github.io/CrispyWaffle/)
- [Changelog](https://guibranco.github.io/CrispyWaffle/changelog/)
- [Source code](https://github.com/guibranco/CrispyWaffle)
- [Report an issue](https://github.com/guibranco/CrispyWaffle/issues/new)

## License

Released under the [MIT License](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE).
