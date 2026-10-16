# CrispyWaffle.Log4Net

CrispyWaffle.Log4Net is a bridge that sends CrispyWaffle log calls to an Apache log4net logger. Use it when your application already configures log4net appenders and you want messages written through `LogConsumer` to go to the same destinations.

| | |
|---|---|
| **Package** | `CrispyWaffle.Log4Net` |
| **Namespace** | `CrispyWaffle.Log4Net` |
| **Install** | `dotnet add package CrispyWaffle.Log4Net` |
| **Depends on** | `CrispyWaffle`, `log4net` |

## How it works

`Log4NetLogProvider` implements `ILogProvider` and wraps a log4net `ILog`. You configure log4net (appenders, layouts, thresholds) as usual, get an `ILog` from `LogManager`, and register the provider with `LogConsumer`.

The provider filters calls by its own CrispyWaffle `LogLevel` first. It starts with no level enabled, so call `SetLevel` after you register it. Calls that pass the filter go to the log4net method shown below. log4net then applies its own level thresholds.

| CrispyWaffle call | log4net call |
|---|---|
| `Fatal` | `ILog.Fatal` |
| `Error` | `ILog.Error` |
| `Warning` | `ILog.Warn` |
| `Info` | `ILog.Info` |
| `Trace` (all overloads) | `ILog.Info` |
| `Debug` (all overloads) | `ILog.Debug` |

The `category` argument is not forwarded; log4net uses the logger name of the `ILog` you pass in. Exceptions are not passed to log4net as exception objects. The provider writes the message and stack trace of the exception and of each inner exception as separate `Info` entries.

[![Logging pipeline](../images/diagrams/logging-pipeline.svg)](../images/diagrams/logging-pipeline.svg)

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

## Reference

### `Log4NetLogProvider`

`public sealed class Log4NetLogProvider : ILogProvider` — log provider that writes to a log4net `ILog`.

| Member | Description |
|---|---|
| `Log4NetLogProvider(ILog adapter)` | Constructor. The argument is not checked for `null`. |
| `SetLevel(LogLevel level)` | Sets the enabled levels. Calls for levels that are not enabled are ignored. |
| `Fatal(category, message)` | Writes `message` with `ILog.Fatal`. |
| `Error(category, message)` | Writes `message` with `ILog.Error`. |
| `Warning(category, message)` | Writes `message` with `ILog.Warn`. |
| `Info(category, message)` | Writes `message` with `ILog.Info`. |
| `Trace(category, message)` | Writes `message` with `ILog.Info`. |
| `Trace(category, message, exception)` | Writes `message`, then the message and stack trace of `exception` and each inner exception, with `ILog.Info`. |
| `Trace(category, exception)` | Writes the message and stack trace of `exception` and each inner exception with `ILog.Info`. |
| `Debug(category, message)` | Writes `message` with `ILog.Debug`. |
| `Debug(category, content, identifier)` | Writes `identifier`, then `content`, as two `ILog.Debug` entries. |
| `Debug<T>(string category, T content, string identifier, SerializerFormat customFormat = SerializerFormat.None)` | Writes `identifier`, then `content` serialized with its `[Serializer]` format (or with `customFormat` when it is not `None`), as two `ILog.Debug` entries. `T` must be `class, new()`. |

## Examples

### Use a named logger and a narrower level

Only errors and fatal messages reach log4net.

```csharp
using CrispyWaffle.Log;
using CrispyWaffle.Log4Net;
using log4net;

var provider = new Log4NetLogProvider(LogManager.GetLogger("CrispyWaffle"));
provider.SetLevel(LogLevel.Fatal | LogLevel.Error);
LogConsumer.AddProvider(provider);

LogConsumer.Error("Payment gateway returned {0}", 503);
```

## See also

- [Logging](../core/logging.md)
- [CrispyWaffle.EventLog](eventlog.md)
- [CrispyWaffle.Elmah](elmah.md)
