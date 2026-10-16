# Logging

CrispyWaffle logging is a static facade, `LogConsumer`, that fans each message out to every registered log provider. Providers decide where messages go (console, rolling text files, Log4Net, Redis, RabbitMQ, Elasticsearch, Windows Event Log). Filters decide which provider receives which message, and an exception handler decides what happens to exceptions passed to `LogConsumer.Handle`.

Use it when you want one logging call site in your code and the freedom to add or swap destinations at startup without touching that code.

| | |
|---|---|
| **Package** | `CrispyWaffle` |
| **Namespace** | `CrispyWaffle.Log`, `CrispyWaffle.Log.Adapters`, `CrispyWaffle.Log.Filters`, `CrispyWaffle.Log.Handlers`, `CrispyWaffle.Log.Providers` |

## How it works

[![Logging pipeline](../images/diagrams/logging-pipeline.svg)](../images/diagrams/logging-pipeline.svg)

The pipeline has four kinds of parts:

- **`LogConsumer`** is the static entry point. It holds a list of providers, a list of filters and one exception handler.
- **Providers** (`ILogProvider`) receive a category and a message for each level. You register them with `LogConsumer.AddProvider`.
- **Adapters** (`ILogAdapter` and friends) do the actual writing for the two core providers. `ConsoleLogProvider` writes through an `IConsoleLogAdapter` and `TextFileLogProvider` writes through an `ITextFileLogAdapter`, both resolved from the [service locator](service-locator.md).
- **Filters** (`ILogFilter`) are checked per provider, per message.

### Sending a message

When you call `LogConsumer.Info("...")` (or any other level), the consumer:

1. Resolves a **category** by walking the stack trace. It skips frames whose declaring type is in `CrispyWaffle.Log*` or is a CrispyWaffle type whose name ends with `LogProvider`, then uses the declaring type's full name (for example `MyApp.Services.OrderService`). A leading `CrispyWaffle.` prefix is removed. For async methods and lambdas this is the compiler-generated type name (for example `MyApp.OrderService+<RunAsync>d__0`). If no frame qualifies, the category is `CrispyWaffle`.
2. For each provider, calls every registered filter with the provider's full type name, the level, the category and the message. The provider receives the message only if **all** filters return `true`.
3. Calls the matching method on each provider that passed, synchronously and in registration order.

`LogTo<TLogProvider>` and `DebugTo<TLogProvider>` skip the fan-out and target the one registered provider whose runtime type is exactly `TLogProvider`. They return `false` when no such provider is registered or a filter rejects the message.

The overloads that take `params object[] arguments` format the message with `string.Format(CultureInfo.InvariantCulture, ...)` before it reaches the providers.

### Levels

`LogLevel` is a `[Flags]` enum. Each provider (through its adapter) keeps its own level and drops messages whose level flag is not set. `LogConsumer` itself does not check levels. The built-in adapters start at `LogLevel.Production` (Fatal, Error, Warning, Info), so `Trace` and `Debug` messages are dropped until you call `SetLevel` on the provider:

```csharp
LogConsumer.AddProvider<ConsoleLogProvider>().SetLevel(LogLevel.All);
```

### Exceptions

`LogConsumer.Handle(exception)` (and the two event-handler overloads) forward to the current `IExceptionHandler`. The handler is chosen the first time `LogConsumer` is used (static constructor): it resolves `IExceptionHandler` from the service locator and, if that fails, falls back to `DefaultExceptionHandler` and passes it the resolution error. Without a registration, resolution normally fails because the core package already contains two implementations (`DefaultExceptionHandler` and `NullExceptionHandler`) and auto-registration refuses to pick one. Register your handler before the first logging call, or replace it later with `LogConsumer.SetHandler`.

`DefaultExceptionHandler` does **not** write to the providers registered on `LogConsumer`. It keeps its own static list of providers, added with `AddLogProvider<T>(ExceptionLogType)`, `TryAddConsoleLogProvider()` or `TryAddTextFileLogProvider()`. When it handles an exception it:

1. Resolves a category from the stack trace, the same way `LogConsumer` does.
2. Unwraps the exception chain (innermost first) and calls `TelemetryAnalytics.TrackException` for each inner exception type (see [Telemetry](telemetry.md)).
3. Calls `Error(category, message)` on each `ExceptionLogType.Message` provider once per exception in the chain, with just that exception's `Message`.
4. Calls `Error(category, details)` once on each `ExceptionLogType.Full` provider, where `details` contains every exception's message, type name and stack trace, with `Exception rethrow at [n]:` markers between them.

With no providers added, `DefaultExceptionHandler` only tracks telemetry.

### Integration packages

Other CrispyWaffle packages add providers and handlers that plug into the same pipeline:

| Package | Adds |
|---|---|
| [Log4Net](../packages/log4net.md) | `Log4NetLogProvider` |
| [EventLog](../packages/eventlog.md) | `EventLogProvider`, `EventLogAdapter` |
| [Redis](../packages/redis.md) | `PubSubRedisLogProvider` |
| [RabbitMQ](../packages/rabbitmq.md) | `RabbitMQLogProvider` |
| [Elasticsearch](../packages/elasticsearch.md) | `ElasticSearchLogProvider` |
| [Elmah](../packages/elmah.md) | `ElmahExceptionHandler` |

### Threading

`LogConsumer` holds providers and filters in plain collections with no locking, so register them at startup before logging from multiple threads. `StandardConsoleLogAdapter` and `RollingTextFileLogAdapter` lock around each write, so concurrent logging through them is safe.

## Getting started

Register an adapter and an exception handler, add a provider, then log.

```csharp
using System;
using CrispyWaffle.Composition;
using CrispyWaffle.Log;
using CrispyWaffle.Log.Adapters;
using CrispyWaffle.Log.Handlers;
using CrispyWaffle.Log.Providers;

public static class Program
{
    public static void Main()
    {
        // Adapter used by ConsoleLogProvider.
        ServiceLocator.Register<IConsoleLogAdapter, StandardConsoleLogAdapter>(Lifetime.Singleton);

        // Must be registered before the first LogConsumer call.
        ServiceLocator.Register<IExceptionHandler, NullExceptionHandler>(Lifetime.Singleton);

        LogConsumer.AddProvider<ConsoleLogProvider>().SetLevel(LogLevel.All);

        LogConsumer.Info("Hello world Crispy Waffle");
        LogConsumer.Debug("Current time: {0:HH:mm:ss}", DateTime.Now);
        LogConsumer.Warning("Press any key to close the program!");

        Console.ReadKey();
    }
}
```

## Reference

### `LogConsumer`

`public static class LogConsumer` — static facade that sends messages to every registered provider and exceptions to the exception handler.

Methods without a `To` suffix return `void` and write to every provider that passes the filters. The category is resolved from the caller; see [How it works](#how-it-works).

| Member | Description |
|---|---|
| `StorageDirectory` | `public static readonly string`. `<AppDomain base directory>/Logs`. Set by the static constructor; not used by the core library. |
| `DebugDirectory` | `public static readonly string`. `StorageDirectory/Debug`. Not used by the core library. |
| `AddProvider<TLogProvider>()` | Resolves `TLogProvider` from `ServiceLocator`, adds it and returns it as `ILogProvider`. Constraint: `TLogProvider : ILogProvider`. |
| `AddProvider(ILogProvider provider)` | Adds an existing provider instance and returns it. |
| `AddFilter(ILogFilter filter)` | Adds a filter. Every filter must return `true` for a provider to receive a message. |
| `SetHandler(IExceptionHandler handler)` | Replaces the exception handler used by `Handle`. |
| `LogTo<TLogProvider>(LogLevel level, string message)` | Logs to the single registered provider whose type is exactly `TLogProvider`. Returns `false` if none is registered or a filter rejects it. Throws `InvalidOperationException` if more than one provider of that type is registered, and `ArgumentOutOfRangeException` if `level` is a combined value such as `Production`. |
| `DebugTo<TLogProvider>(string content, string identifier)` | Sends content with an identifier at Debug level to one provider (`ILogProvider.Debug(category, content, identifier)`). Same return rules as `LogTo`. |
| `DebugTo<TLogProvider, T>(T content, string identifier, SerializerFormat customFormat = SerializerFormat.None)` | Sends an object at Debug level to one provider for serialization. Constraint: `T : class, new()`. Filters receive an empty message. |
| `Debug(string message)` / `Debug(string message, params object[] arguments)` | Logs at Debug level. |
| `Debug(string content, string identifier)` | Sends content with an identifier at Debug level (`ILogProvider.Debug(category, content, identifier)`). Note that `Debug("text {0}", "value")` binds to this overload, not to the formatting one. |
| `Debug<T>(T content, string identifier, SerializerFormat customFormat = SerializerFormat.None)` | Sends an object at Debug level for serialization. Constraint: `T : class, new()`. Filters receive an empty message. |
| `Trace(string message)` / `Trace(string message, params object[] arguments)` | Logs at Trace level. |
| `Trace(Exception exception)` | Logs exception details at Trace level. Filters receive `exception.Message`. |
| `Trace(Exception exception, string message)` / `Trace(Exception exception, string message, params object[] arguments)` | Logs a message plus exception details at Trace level. |
| `Info(string message)` / `Info(string message, params object[] arguments)` | Logs at Info level. |
| `Warning(string message)` / `Warning(string message, params object[] arguments)` | Logs at Warning level. |
| `Error(string message)` / `Error(string message, params object[] arguments)` | Logs at Error level. |
| `Fatal(string message)` / `Fatal(string message, params object[] arguments)` | Logs at Fatal level. |
| `Handle(Exception exception)` | Passes the exception to the current `IExceptionHandler`. |
| `Handle(object sender, UnhandledExceptionEventArgs args)` | Passes the event to the handler. Matches `AppDomain.UnhandledException`. |
| `Handle(object sender, ThreadExceptionEventArgs args)` | Passes the event to the handler. Matches `Application.ThreadException` (WinForms). |

### `LogLevel`

`[Flags] public enum LogLevel` — severity flags. Each value has a `[HumanReadable]` name used in log output.

| Value | Description |
|---|---|
| `Fatal` (1) | Fatal errors. Human-readable name `Fatal`. |
| `Error` (2) | Errors. `Error`. |
| `Warning` (4) | Warnings. `Warning`. |
| `Info` (8) | Informational messages. `Information`. |
| `Trace` (16) | Detailed diagnostic messages and exception traces. `Trace`. |
| `Debug` (32) | Debug messages and content dumps. `Debug`. |
| `Production` | `Fatal \| Error \| Warning \| Info`. Default level of the built-in adapters. |
| `Development` | `Production \| Trace`. |
| `All` | `Development \| Debug`. |

### `LogMessage`

`[Serializer(SerializerFormat.Json)] public sealed class LogMessage` — the record `RollingTextFileLogAdapter` writes for each entry. Also usable by custom providers.

All properties have public getters and setters.

| Member | Description |
|---|---|
| `Id` | `string`. Unique id of the entry (the rolling adapter uses a new GUID). |
| `Level` | `string`. Human-readable level name. |
| `Application` | `string`. Application name. |
| `Category` | `string`. Log category. |
| `ProcessId` | `int`. Process id. |
| `ThreadId` | `int`. Managed thread id. |
| `ThreadName` | `string`. Thread name. |
| `Hostname` | `string`. Machine host name. |
| `IpAddress` | `string`. Local IP address. |
| `IpAddressRemote` | `string`. External IP address. |
| `UserAgent` | `string`. User agent. |
| `Date` | `DateTime`. Local time of the entry. |
| `Message` | `string`. Message text. |
| `MessageIdentifier` | `string`. Identifier passed to a `Debug(content, identifier)` call, otherwise the current log file name. |
| `Operation` | `string`. Current operation. |

The rolling adapter fills `Application`, `Hostname`, `IpAddress`, `IpAddressRemote`, `Operation`, `ProcessId` and `UserAgent` from `EnvironmentHelper` (see [Infrastructure](infrastructure.md)).

### `ILogProvider`

`public interface ILogProvider` — a log destination registered with `LogConsumer`. Implement it to add your own destination.

| Member | Description |
|---|---|
| `SetLevel(LogLevel level)` | Sets the levels this provider writes. |
| `Fatal(string category, string message)` | Writes at Fatal level. |
| `Error(string category, string message)` | Writes at Error level. |
| `Warning(string category, string message)` | Writes at Warning level. |
| `Info(string category, string message)` | Writes at Info level. |
| `Trace(string category, string message)` | Writes at Trace level. |
| `Trace(string category, string message, Exception exception)` | Writes a message and exception details at Trace level. |
| `Trace(string category, Exception exception)` | Writes exception details at Trace level. |
| `Debug(string category, string message)` | Writes at Debug level. |
| `Debug(string category, string content, string identifier)` | Stores content under an identifier (file name, key, and so on; meaning depends on the implementation) at Debug level. |
| `Debug<T>(string category, T content, string identifier, SerializerFormat customFormat)` | Serializes `content` and stores it under an identifier at Debug level. Constraint: `T : class, new()`. |

### `IExceptionHandler`

`public interface IExceptionHandler` — handles exceptions passed to `LogConsumer.Handle`.

| Member | Description |
|---|---|
| `Handle(Exception exception)` | Handles an exception. |
| `Handle(object sender, UnhandledExceptionEventArgs args)` | Handles an `AppDomain.UnhandledException` event. |
| `Handle(object sender, ThreadExceptionEventArgs args)` | Handles a thread exception event. |
| `AddLogProvider<TLogProvider>(ExceptionLogType type)` | Adds a provider (resolved from `ServiceLocator`) that exceptions are written to, and returns it. Constraint: `TLogProvider : ILogProvider`. |

### `ILogFilter`

`public interface ILogFilter` — decides whether a provider receives a message.

| Member | Description |
|---|---|
| `Filter(string providerType, LogLevel level, string category, string message)` | Returns `true` to let the message through to the provider, `false` to drop it. `providerType` is the provider's full type name. For the `Debug<T>`/`DebugTo<TLogProvider, T>` overloads `message` is empty. |

### `ILogAdapter`

`public interface ILogAdapter : IDisposable` — low-level writer used by providers.

| Member | Description |
|---|---|
| `SetLevel(LogLevel level)` | Sets the levels the adapter writes. |
| `Debug<T>(T content, string identifier, SerializerFormat customFormat = SerializerFormat.None)` | Serializes `content` (with its default serializer, or `customFormat` when not `None`) and writes it at Debug level. Constraint: `T : class`. |
| `Debug(string content, string fileName)` | Writes content with an identifier at Debug level. |
| `Debug(string message)` | Writes at Debug level. |
| `Trace(Exception exception)` | Writes exception details at Trace level. |
| `Trace(string message, Exception exception)` | Writes a message, then exception details, at Trace level. |
| `Trace(string message)` | Writes at Trace level. |
| `Info(string message)` | Writes at Info level. |
| `Warning(string message)` | Writes at Warning level. |
| `Error(string message)` | Writes at Error level. |
| `Fatal(string message)` | Writes at Fatal level. |

### `ICategorizedLogAdapter`

`public interface ICategorizedLogAdapter : ILogAdapter` — adapter that also records a category with each entry.

| Member | Description |
|---|---|
| `CategorizedFatal(string category, string message)` | Writes at Fatal level under `category`. |
| `CategorizedError(string category, string message)` | Writes at Error level. |
| `CategorizedWarning(string category, string message)` | Writes at Warning level. |
| `CategorizedInfo(string category, string message)` | Writes at Info level. |
| `CategorizedTrace(string category, string message)` | Writes at Trace level. |
| `CategorizedTrace(string category, string message, Exception exception)` | Writes a message and exception details at Trace level. |
| `CategorizedTrace(string category, Exception exception)` | Writes exception details at Trace level. |
| `CategorizedDebug(string category, string message)` | Writes at Debug level. |
| `CategorizedDebug(string category, string content, string identifier)` | Writes content with an identifier. |
| `CategorizedDebug<T>(string category, T content, string identifier, SerializerFormat customFormat = SerializerFormat.None)` | Serializes and writes `content` at Debug level. Constraint: `T : class`. |

### `IConsoleLogAdapter`

`public interface IConsoleLogAdapter : ILogAdapter` — marker interface for console adapters. `ConsoleLogProvider` depends on it. Adds no members.

### `ITextFileLogAdapter`

`public interface ITextFileLogAdapter : ICategorizedLogAdapter` — marker interface for text file adapters. `TextFileLogProvider` depends on it. Adds no members.

### `ConsoleLogProvider`

`public sealed class ConsoleLogProvider : ILogProvider` — writes to the console through an `IConsoleLogAdapter`.

The adapter is stored in a **static** field: the first `ConsoleLogProvider` constructed keeps its adapter for the life of the process, and later instances reuse it and ignore the adapter passed to them. The category is ignored; console output does not include it.

| Member | Description |
|---|---|
| `ConsoleLogProvider(IConsoleLogAdapter adapter)` | Constructor. Usually called by `ServiceLocator` through `LogConsumer.AddProvider<ConsoleLogProvider>()`, so register an `IConsoleLogAdapter` first. |
| `SetLevel(LogLevel level)` | Forwards to the adapter. |
| `Fatal`, `Error`, `Warning`, `Info`, `Trace`, `Debug` (all `ILogProvider` overloads) | Forward to the matching adapter method, dropping `category`. `Debug(category, content, identifier)` calls `adapter.Debug(content, identifier)`; `Debug<T>(...)` calls `adapter.Debug(content, identifier, customFormat)`. `Debug<T>` defaults `customFormat` to `SerializerFormat.None`. |

### `TextFileLogProvider`

`public sealed class TextFileLogProvider : ILogProvider` — writes to files through an `ITextFileLogAdapter`, keeping the category.

| Member | Description |
|---|---|
| `TextFileLogProvider(ITextFileLogAdapter adapter)` | Constructor. Register an `ITextFileLogAdapter` (for example `RollingTextFileLogAdapter`) before `LogConsumer.AddProvider<TextFileLogProvider>()`. |
| `SetLevel(LogLevel level)` | Forwards to the adapter. |
| `Fatal`, `Error`, `Warning`, `Info`, `Trace`, `Debug` (all `ILogProvider` overloads) | Forward to the matching `Categorized*` adapter method with the category. `Debug<T>` defaults `customFormat` to `SerializerFormat.None`. |

### `StandardConsoleLogAdapter`

`public sealed class StandardConsoleLogAdapter : IConsoleLogAdapter` — writes colored lines to the console.

Each line is `HH:mm:ss message`. Colors: Fatal and Error red, Warning yellow, Info green, Trace dark green, Debug gray; the foreground color is reset to white after each write. Writes are serialized with a static lock. The constructor checks whether standard input is available; if it is not (for example in a Windows service), nothing is written.

| Member | Description |
|---|---|
| `StandardConsoleLogAdapter()` | Constructor. Starts at `LogLevel.Production`. |
| `SetLevel(LogLevel level)` | Writes a "Switching log level from X to Y" warning (subject to the old level), then changes the level. |
| `Debug<T>(T content, string identifier, SerializerFormat customFormat = SerializerFormat.None)` | Serializes `content` and prints it. `identifier` is ignored. |
| `Debug(string content, string fileName)` | Prints `content`. `fileName` is ignored. |
| `Debug(string message)`, `Trace(string message)`, `Info`, `Warning`, `Error`, `Fatal` | Print the message at that level. |
| `Trace(Exception exception)` | Prints the `Message` of the exception and each inner exception, one per line. No stack trace. |
| `Trace(string message, Exception exception)` | Prints the message, then the exception messages. |
| `Dispose()` | No resources to release. |

### `RollingTextFileLogAdapter`

`public sealed class RollingTextFileLogAdapter : ITextFileLogAdapter` — writes entries to files in a folder, starting a new file when a message count or size limit is reached.

Files are named `LogFile-{fileNameSeed}-[{n}].txt` (or `.json`). On start and on each roll, `n` is increased until the name does not collide with an existing file. Before each write the adapter checks the current file: when it holds `maxMessageCount` messages or its length has reached `maxFileSize`, it closes it and opens the next one, so a file can exceed the size limit by one entry. Writes are serialized with a lock per instance. Use one instance per seed; two instances with the same seed and folder write to different numbers but may race on creating them.

Each entry is a `LogMessage`. In `LogFileType.Text` files, entries are separated by a `----------` line and each property is written as `Name: value` (`N.A.` for empty values). In `LogFileType.Json` files, the file is kept as a valid JSON array of entries. Exceptions are written as `[MainException][Type]: message` followed by one `[InnerException][Type]: message` line per inner exception, without stack traces. The default category is `N.A.`.

| Member | Description |
|---|---|
| `RollingTextFileLogAdapter(string folderPath, string fileNameSeed, int maxMessageCount, (Unit unit, uint size) maxFileSize, LogFileType fileType = LogFileType.Text)` | Constructor. Creates the first file immediately. Throws `ArgumentOutOfRangeException` when `maxFileSize` is under 1 KB or over 10 GB, or `maxMessageCount` is less than 1. Throws `DirectoryNotFoundException` when `folderPath` does not exist. Starts at `LogLevel.Production`. |
| `SetLevel(LogLevel level)` | Changes the level, then writes a "Level updated" warning. |
| `Debug(string message)`, `Trace(string message)`, `Info`, `Warning`, `Error`, `Fatal` | Write an entry at that level with the default category. |
| `Trace(Exception exception)` / `Trace(string message, Exception exception)` | Write exception details (and the message as a separate entry) at Trace level. |
| `Debug(string content, string fileName)` | Writes `content` as a Debug entry in the current log file, with `MessageIdentifier` set to `fileName`. No separate file is created. |
| `Debug<T>(T content, string identifier, SerializerFormat customFormat = SerializerFormat.None)` | Serializes `content` and writes it as a Debug entry with `MessageIdentifier` set to `identifier`. |
| `CategorizedFatal`, `CategorizedError`, `CategorizedWarning`, `CategorizedInfo`, `CategorizedTrace`, `CategorizedDebug(category, message)`, `CategorizedDebug<T>(...)` | Same as the uncategorized methods, with `Category` set to `category`. |
| `CategorizedDebug(string category, string content, string identifier)` | Writes `content` with `MessageIdentifier` set to `identifier`. Note: the entry is written at **Fatal** level, not Debug. |
| `Dispose()` | Closes the current file stream. Also called from the finalizer. |

### `Unit`

`public enum Unit` — size unit for `RollingTextFileLogAdapter`'s `maxFileSize`. The numeric value is the number of bytes per unit.

| Value | Description |
|---|---|
| `Byte` (1) | Bytes. |
| `KByte` (1024) | Kilobytes. |
| `MByte` (`1024 * 1024`) | Megabytes. |
| `GByte` (`1024 * 1024 * 1024`) | Gigabytes. |

### `LogFileType`

`public enum LogFileType` — output format of `RollingTextFileLogAdapter`. The `[InternalValue]` is the file extension.

| Value | Description |
|---|---|
| `Text` (0) | Plain text entries, `.txt` extension. |
| `JSON` | A JSON array of `LogMessage` objects, `.json` extension. |

### `CategoryLogFilter`

`public class CategoryLogFilter : ILogFilter` — filters messages by category, optionally for one provider type only.

| Member | Description |
|---|---|
| `CategoryLogFilter(Type provider = null, bool isExclusive = true)` | Constructor. When `provider` is set, the filter applies only to that provider type; messages for other providers always pass. When `isExclusive` is `true`, messages whose category is in the list are **dropped**. When `false`, only messages whose category is in the list pass. |
| `AddCategory(string category)` | Adds a category (exact, case-sensitive match). Returns the same filter for chaining. |
| `Filter(string providerType, LogLevel level, string category, string message)` | Applies the rules above. `level` and `message` are ignored. The provider comparison is case-insensitive on the full type name. |

### `DefaultExceptionHandler`

`public class DefaultExceptionHandler : IExceptionHandler` — logs exceptions to its own list of providers and tracks exception telemetry. Used when no `IExceptionHandler` is registered. See [Exceptions](#exceptions) for the exact output.

The provider list is **static**, shared by all instances. Adding the same provider twice makes it log each exception twice.

| Member | Description |
|---|---|
| `Handle(Exception exception)` | Handles the exception as described above. |
| `Handle(object sender, UnhandledExceptionEventArgs args)` | Casts `args.ExceptionObject` to `Exception` and handles it. |
| `Handle(object sender, ThreadExceptionEventArgs args)` | Handles `args.Exception`. |
| `AddLogProvider<TLogProvider>(ExceptionLogType type)` | Resolves `TLogProvider` from `ServiceLocator`, adds it with the given output type and returns it. |
| `TryAddConsoleLogProvider()` | Static. If a console is available and `ConsoleLogProvider` can be resolved, adds it as `ExceptionLogType.Message`. Swallows all errors. |
| `TryAddTextFileLogProvider()` | Static. If `TextFileLogProvider` can be resolved, adds it as `ExceptionLogType.Full`. Swallows all errors. |

### `NullExceptionHandler`

`public sealed class NullExceptionHandler : IExceptionHandler` — ignores all exceptions.

| Member | Description |
|---|---|
| `Handle(Exception exception)` / `Handle(object sender, UnhandledExceptionEventArgs args)` / `Handle(object sender, ThreadExceptionEventArgs args)` | Do nothing. |
| `AddLogProvider<TLogProvider>(ExceptionLogType type)` | Resolves and returns `TLogProvider` from `ServiceLocator` without storing it. |

### `FailoverExceptionHandler`

`public static class FailoverExceptionHandler` — last-resort exception logging that falls back to a file if logging itself fails.

| Member | Description |
|---|---|
| `Handle(Exception exception)` | Resolves `DefaultExceptionHandler`, adds `TextFileLogProvider` to it as `ExceptionLogType.Full`, then calls `LogConsumer.Handle(exception)`. If anything throws (for example no `ITextFileLogAdapter` is registered), writes the original and the new exception (messages, stack traces, inner exceptions and `ReflectionTypeLoadException` loader exceptions) to `fatal-yyyyMMddHHmmss.log` in the current working directory. Each call adds another `TextFileLogProvider`, so call it once, not on every exception. |

### `ExceptionLogType`

`public enum ExceptionLogType` — how an exception handler writes to a provider.

| Value | Description |
|---|---|
| `Message` | The provider receives one `Error` entry per exception in the chain, containing only that exception's message. |
| `Full` | The provider receives one `Error` entry with every message, type name and stack trace in the chain. |

## Examples

### Rolling text files

Write to text files that roll every 100 messages or 10 KB. Register the adapter as a singleton so every resolution of `TextFileLogProvider` shares the same files.

```csharp
using System;
using CrispyWaffle.Composition;
using CrispyWaffle.Log;
using CrispyWaffle.Log.Adapters;
using CrispyWaffle.Log.Handlers;
using CrispyWaffle.Log.Providers;

ServiceLocator.Register<ITextFileLogAdapter>(
    () => new RollingTextFileLogAdapter(
        AppDomain.CurrentDomain.BaseDirectory, // folder, must exist
        "textFileLogs",                        // file name seed
        100,                                   // max messages per file
        (Unit.KByte, 10),                      // max file size
        LogFileType.Text),
    Lifetime.Singleton);

ServiceLocator.Register<IExceptionHandler, NullExceptionHandler>(Lifetime.Singleton);

LogConsumer.AddProvider<TextFileLogProvider>();

LogConsumer.Info("Hello world Crispy Waffle");
LogConsumer.Warning("Warning message!");
```

### Logging unhandled exceptions

Use the default handler, write exception messages to the console and full details to text files, and hook process-wide unhandled exceptions.

```csharp
using System;
using CrispyWaffle.Composition;
using CrispyWaffle.Log;
using CrispyWaffle.Log.Adapters;
using CrispyWaffle.Log.Handlers;
using CrispyWaffle.Log.Providers;

ServiceLocator.Register<IConsoleLogAdapter, StandardConsoleLogAdapter>(Lifetime.Singleton);
ServiceLocator.Register<ITextFileLogAdapter>(
    () => new RollingTextFileLogAdapter("Logs", "app", 1000, (Unit.MByte, 5), LogFileType.Json),
    Lifetime.Singleton);
ServiceLocator.Register<IExceptionHandler, DefaultExceptionHandler>(Lifetime.Singleton);

DefaultExceptionHandler.TryAddConsoleLogProvider();
DefaultExceptionHandler.TryAddTextFileLogProvider();

AppDomain.CurrentDomain.UnhandledException += LogConsumer.Handle;

try
{
    throw new InvalidOperationException("Something failed");
}
catch (Exception e)
{
    LogConsumer.Handle(e);
}
```

The `Logs` folder must exist before the adapter is created.

### Filtering by category

Drop messages from one namespace for the console provider only.

```csharp
using CrispyWaffle.Log;
using CrispyWaffle.Log.Filters;
using CrispyWaffle.Log.Providers;

LogConsumer.AddFilter(
    new CategoryLogFilter(typeof(ConsoleLogProvider), isExclusive: true)
        .AddCategory("MyApp.Services.NoisyService"));
```

The category is the caller's full type name, so add one entry per type you want to filter.

### Dumping an object for debugging

Serialize an object to every provider at Debug level.

```csharp
using CrispyWaffle.Log;
using CrispyWaffle.Serialization;

LogConsumer.Debug(new Order { Id = 1, Total = 9.90m }, "order-1", SerializerFormat.Json);

[Serializer(SerializerFormat.Json)]
public class Order
{
    public int Id { get; set; }
    public decimal Total { get; set; }
}
```

The provider must have `LogLevel.Debug` enabled (for example `SetLevel(LogLevel.All)`) for the entry to be written.

### Writing a custom provider

Implement `ILogProvider` and add an instance.

```csharp
using System;
using CrispyWaffle.Log;
using CrispyWaffle.Log.Providers;
using CrispyWaffle.Serialization;

LogConsumer.AddProvider(new DebugOutputLogProvider()).SetLevel(LogLevel.All);
LogConsumer.Info("Custom provider registered");

public sealed class DebugOutputLogProvider : ILogProvider
{
    private LogLevel _level = LogLevel.Production;

    public void SetLevel(LogLevel level) => _level = level;

    private void Write(LogLevel level, string category, string message)
    {
        if (_level.HasFlag(level))
        {
            System.Diagnostics.Debug.WriteLine($"[{level}] {category}: {message}");
        }
    }

    public void Fatal(string category, string message) => Write(LogLevel.Fatal, category, message);
    public void Error(string category, string message) => Write(LogLevel.Error, category, message);
    public void Warning(string category, string message) => Write(LogLevel.Warning, category, message);
    public void Info(string category, string message) => Write(LogLevel.Info, category, message);
    public void Trace(string category, string message) => Write(LogLevel.Trace, category, message);
    public void Trace(string category, string message, Exception exception) =>
        Write(LogLevel.Trace, category, $"{message}: {exception}");
    public void Trace(string category, Exception exception) =>
        Write(LogLevel.Trace, category, exception.ToString());
    public void Debug(string category, string message) => Write(LogLevel.Debug, category, message);
    public void Debug(string category, string content, string identifier) =>
        Write(LogLevel.Debug, category, $"{identifier}: {content}");
    public void Debug<T>(string category, T content, string identifier, SerializerFormat customFormat)
        where T : class, new() =>
        Write(LogLevel.Debug, category, $"{identifier}: {content}");
}
```

## See also

- [Service locator](service-locator.md)
- [Telemetry](telemetry.md)
- [Serialization](serialization.md)
- [Infrastructure](infrastructure.md)
- [Diagrams](../diagrams.md)
