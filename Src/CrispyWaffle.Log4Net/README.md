# CrispyWaffle.Log4Net

[![NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Log4Net.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Log4Net/) [![NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Log4Net.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Log4Net/) [![GitHub license](https://img.shields.io/github/license/guibranco/CrispyWaffle)](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE)

Part of [Crispy Waffle](https://github.com/guibranco/CrispyWaffle), a toolkit for .NET projects.

CrispyWaffle.Log4Net is a bridge that sends CrispyWaffle log calls to an Apache log4net logger. Use it when your application already configures log4net appenders and you want messages written through `LogConsumer` to go to the same destinations.

| | |
|---|---|
| **Package** | `CrispyWaffle.Log4Net` |
| **Namespace** | `CrispyWaffle.Log4Net` |
| **Install** | `dotnet add package CrispyWaffle.Log4Net` |
| **Depends on** | `CrispyWaffle`, `log4net` |

## Supported frameworks

This package targets **.NET Standard 2.0**, **.NET Standard 2.1**, **.NET 8.0**, **.NET 9.0** and **.NET 10.0**.

## Getting started

```csharp
using System.IO;
using System.Reflection;
using CrispyWaffle.Log;
using CrispyWaffle.Log4Net;
using log4net;
using log4net.Config;

var repository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));

var log = LogManager.GetLogger(typeof(Program));

LogConsumer.AddProvider(new Log4NetLogProvider(log)).SetLevel(LogLevel.All);

LogConsumer.Info("Application started");
```

## Documentation

The full reference and more examples for this package are on the [CrispyWaffle.Log4Net documentation page](https://guibranco.github.io/CrispyWaffle/packages/log4net/).

- [Crispy Waffle documentation](https://guibranco.github.io/CrispyWaffle/)
- [Changelog](https://guibranco.github.io/CrispyWaffle/changelog/)
- [Source code](https://github.com/guibranco/CrispyWaffle)
- [Report an issue](https://github.com/guibranco/CrispyWaffle/issues/new)

## License

Released under the [MIT License](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE).
