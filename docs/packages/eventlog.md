# CrispyWaffle.EventLog

CrispyWaffle.EventLog writes CrispyWaffle log messages to the Windows Event Log. Use it for Windows services and desktop applications whose operators read logs in Event Viewer.

The package uses `System.Diagnostics.EventLog`, which only works on Windows. The library targets .NET Standard and .NET and has no platform annotation, so it builds anywhere, but on Linux and macOS the `System.Diagnostics.EventLog` calls throw `PlatformNotSupportedException`.

| | |
|---|---|
| **Package** | `CrispyWaffle.EventLog` |
| **Namespace** | `CrispyWaffle.EventLog.Log.Providers`, `CrispyWaffle.EventLog.Log.Adapters` |
| **Install** | `dotnet add package CrispyWaffle.EventLog` |
| **Depends on** | `CrispyWaffle`, `System.Diagnostics.EventLog` |

## How it works

`EventLogProvider` is the `ILogProvider` you register with `LogConsumer`. It forwards every call to an `EventLogAdapter`, which writes entries to a `System.Diagnostics.EventLog`. The `category` argument is not used.

**Log and source.** The adapter writes to the log named by `logName`, or to `Application` when `logName` is null, empty or whitespace. The source name is cut to 212 characters and `<` and `>` are replaced by `_`.

**Source registration.** Creating an event source requires administrator rights. By default (`manageEventSource: false`) the adapter only sets the source name; register the source beforehand, for example during installation with `EventLog.CreateEventSource`. With `manageEventSource: true` the adapter creates the source when it does not exist. If the source exists in a different log, the adapter deletes it from that log and writes a warning entry, under a helper source named `serilog-{logName}`, saying that a restart may be required before entries appear in the new log.

**Levels and entry types.** The adapter starts with no level enabled; call `SetLevel` after you register the provider. Levels map to entry types as follows:

| `LogLevel` | `EventLogEntryType` |
|---|---|
| `Fatal`, `Error` | `Error` |
| `Warning` | `Warning` |
| `Info`, `Trace`, `Debug` | `Information` |

**Message size.** Messages longer than 31,839 characters are truncated. Exceptions are written as one entry that contains the message and stack trace of the exception and of each inner exception, separated by `\r\n`.

**Event IDs.** Each entry gets an event ID from an `IEventIdProvider`. The default `EventIdProvider` hashes the message text into a 16-bit number, so the same message always gets the same ID.

[![Logging pipeline](../images/diagrams/logging-pipeline.svg)](../images/diagrams/logging-pipeline.svg)

## Getting started

```csharp
using CrispyWaffle.EventLog.Log.Providers;
using CrispyWaffle.Log;

LogConsumer.AddProvider(new EventLogProvider("my-service")).SetLevel(LogLevel.Production);

LogConsumer.Info("Service started");
```

## Reference

### `EventLogProvider`

`public sealed class EventLogProvider : ILogProvider` — log provider that writes to the Windows Event Log through an `EventLogAdapter`.

| Member | Description |
|---|---|
| `EventLogProvider()` | Constructor. Uses source `crispy-waffle`, the `Application` log, the local machine, no source management, and `EventIdProvider`. |
| `EventLogProvider(string source)` | Constructor. Uses `source`, the `Application` log, the local machine, no source management, and `EventIdProvider`. |
| `EventLogProvider(string source, string logName)` | Constructor. Same as above with the given log name. |
| `EventLogProvider(string source, string logName, string machineName, bool manageEventSource, IEventIdProvider eventIdProvider)` | Constructor. Uses `EventIdProvider` when `eventIdProvider` is `null`. Throws `ArgumentNullException` when `source` is `null`. |
| `SetLevel(LogLevel level)` | Sets the enabled levels on the adapter. |
| `Fatal(category, message)` / `Error(category, message)` / `Warning(category, message)` / `Info(category, message)` | Writes `message` at that level. |
| `Trace(category, message)` | Writes `message` at trace level. |
| `Trace(category, message, exception)` | Writes `message`, then the exception details, as two entries at trace level. |
| `Trace(category, exception)` | Writes the exception details as one entry at trace level. |
| `Debug(category, message)` | Writes `message` at debug level. |
| `Debug(category, content, identifier)` | Writes `"{identifier}: {content}"` at debug level. |
| `Debug<T>(string category, T content, string identifier, SerializerFormat customFormat = SerializerFormat.None)` | Serializes `content` with its `[Serializer]` format (or with `customFormat` when it is not `None`) and writes `"{identifier}: {serialized}"` at debug level. `T` must be `class, new()`. |

### `IEventIdProvider`

`public interface IEventIdProvider` — computes the event ID for a log entry. Implement it to control event IDs, for example to map known messages to fixed IDs.

| Member | Description |
|---|---|
| `ushort ComputeEventId(string message)` | Returns the event ID for `message`. |

### `EventIdProvider`

`public class EventIdProvider : IEventIdProvider` — default implementation that hashes the message.

| Member | Description |
|---|---|
| `ComputeEventId(string message)` | Returns a 16-bit hash of `message` (a Jenkins one-at-a-time hash truncated to `ushort`). Throws `ArgumentNullException` when `message` is `null`. |

### `EventLogAdapter`

`public sealed class EventLogAdapter : ILogAdapter` — writes to a `System.Diagnostics.EventLog`. `EventLogProvider` creates one internally; use it directly only if you build your own provider.

| Member | Description |
|---|---|
| `EventLogAdapter(string source, string logName, string machineName, bool manageEventSource, IEventIdProvider eventIdProvider)` | Constructor. Normalizes the source name, opens the log (`Application` when `logName` is null or whitespace) and either registers the source (`manageEventSource: true`) or only sets it. Throws `ArgumentNullException` when `source` or `eventIdProvider` is `null`. |
| `SetLevel(LogLevel level)` | Sets the enabled levels. |
| `Fatal(message)` / `Error(message)` / `Warning(message)` / `Info(message)` | Writes `message` at that level. |
| `Trace(message)` | Writes `message` at trace level. |
| `Trace(message, exception)` | Writes `message`, then the exception details, as two entries at trace level. |
| `Trace(exception)` | Writes the exception details as one entry at trace level. |
| `Debug(message)` | Writes `message` at debug level. |
| `Debug(string content, string fileName)` | Writes `"{fileName}: {content}"` at debug level. |
| `Debug<T>(T content, string identifier, SerializerFormat customFormat = SerializerFormat.None)` | Serializes `content` and writes `"{identifier}: {serialized}"` at debug level. `T` must be a class. |
| `Dispose()` | Does nothing. The underlying `EventLog` is not disposed. |

## Examples

### Write to a custom log with source management

Run once with administrator rights so the source can be created in the `MyCompany` log.

```csharp
using System;
using CrispyWaffle.EventLog.Log.Providers;
using CrispyWaffle.Log;

var provider = new EventLogProvider(
    "billing-service",
    "MyCompany",
    Environment.MachineName,
    manageEventSource: true,
    eventIdProvider: null
);

LogConsumer.AddProvider(provider).SetLevel(LogLevel.All);

LogConsumer.Warning("Invoice queue is above {0} items", 1000);
```

### Use fixed event IDs

Return the same event ID for every entry from a source.

```csharp
using System;
using CrispyWaffle.EventLog.Log.Providers;

var provider = new EventLogProvider(
    "billing-service",
    "Application",
    Environment.MachineName,
    false,
    new FixedEventIdProvider()
);

public sealed class FixedEventIdProvider : IEventIdProvider
{
    public ushort ComputeEventId(string message) => 1000;
}
```

## See also

- [Logging](../core/logging.md)
- [CrispyWaffle.Log4Net](log4net.md)
