# CrispyWaffle.LogStream

CrispyWaffle.LogStream sends CrispyWaffle log messages to a [LogStream](https://github.com/guibranco/logstream-server) server. Use it when your services already report to LogStream and you want messages written through `LogConsumer` to show up there as structured entries, with their category, level, host details, exception chain and trace ID.

| | |
|---|---|
| **Package** | `CrispyWaffle.LogStream` |
| **Namespace** | `CrispyWaffle.LogStream`, `CrispyWaffle.LogStream.Log` |
| **Install** | `dotnet add package CrispyWaffle.LogStream` |
| **Depends on** | `CrispyWaffle`, `CrispyWaffle.Configuration`, `Newtonsoft.Json` |

## How it works

`LogStreamLogProvider` implements `ILogProvider`. A log call builds an entry and puts it in an in-memory queue; it never waits for the network. A background task sends the queue to `POST /api/logs` as batches of up to `BatchSize` entries, as soon as a full batch is waiting or every `FlushInterval`.

Failed requests are retried with an exponential delay when the failure is transient (network error, timeout, HTTP 408, 429 or 5xx), up to `MaxRetries` times. Other responses (400, 401, 403…) drop the batch at once. Entries the server rejects one by one (the `errors` list of a 202/201 response) are counted as dropped too. Dropped entries never throw: they are counted in `DroppedCount` and reported to the `OnError` callback, or to `System.Diagnostics.Trace` when there is none. The provider does not log its own failures through `LogConsumer`, which would loop back into it.

When the queue holds `QueueLimit` entries, new entries are dropped until it drains. Dispose the provider on shutdown: `Dispose` stops the background task and sends what is left. One `ShutdownTimeout` deadline covers the whole shutdown, including a request already in flight; entries still unsent then, and entries logged after `Dispose`, are counted as dropped.

Like the other providers, it starts with no level enabled, so call `SetLevel` after you register it.

| CrispyWaffle call | LogStream level | Notes |
|---|---|---|
| `Fatal` | `critical` | |
| `Error` | `error` | |
| `Warning` | `warning` | |
| `Info` | `info` | |
| `Trace(category, message)` | `TraceLevel` (default `debug`) | |
| `Trace(category, message, exception)` | `ExceptionLevel` (default `error`) | One entry; the exception chain goes to `context.exception` |
| `Trace(category, exception)` | `ExceptionLevel` (default `error`) | The message is the exception message |
| `Debug(category, message)` | `debug` | |
| `Debug(category, content, identifier)` and `Debug<T>` | `debug` | Message `Debug content {identifier}`, sent as a message template; `context.identifier` and `context.content` hold the values |

Each entry's `context` carries the `StaticContext` properties and, unless `IncludeEnvironment` is off, `application`, `host`, `ip_address`, `process_id`, `thread_id`, `thread_name` and `operation`. When `UseActivityTraceId` is on (the default) and there is a current `System.Diagnostics.Activity`, its trace ID is sent as `trace_id` and its span ID as `context.span_id`, so LogStream's trace view groups the entries of one request. Without an activity, the server generates a trace ID per entry.

Categories longer than 100 characters are truncated; an empty category becomes `general`. The `User-Agent` header is `<ApplicationName>/<Version> (CrispyWaffle.LogStream/<package version>)`, which LogStream records as the application identifier.

## Authentication

LogStream has two kinds of write credentials, and the options choose between them:

| Server storage | Options | Sent as |
|---|---|---|
| File (JSONL) | `ApiSecret` + `AppKey` | `Authorization: Bearer <ApiSecret>`, and `app_key` in the body |
| MariaDB | `ApiKey` + `ApiToken` | `X-Api-Key` and `X-Api-Token`; the server uses the key as the `app_key` |

`AppId` (the instance or environment, for example `production`) is always required.

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

## Reference

### `LogStreamOptions`

| Property | Default | Description |
|---|---|---|
| `ServerUrl` | — | Base address of the server; entries are posted to `{ServerUrl}/api/logs`. Required. |
| `AppKey` | — | Application slug. Required with `ApiSecret`. |
| `AppId` | — | Instance or environment identifier. Required. |
| `ApiSecret` | — | Shared write secret (file storage). |
| `ApiKey`, `ApiToken` | — | Client credentials (MariaDB storage). |
| `BatchSize` | `100` | Entries per request (1–1000). |
| `FlushInterval` | 2 s | Longest wait before a partial batch is sent (at most about 24.8 days). |
| `QueueLimit` | `10000` | Entries kept in memory; newer ones are dropped while full. |
| `MaxRetries` | `3` | Retries after a transient failure. |
| `RetryDelay` | 1 s | First retry delay; doubles after each attempt. |
| `RequestTimeout` | 30 s | Timeout of the `HttpClient` the connector creates. |
| `ShutdownTimeout` | 10 s | Deadline for `Dispose` to send the queue, in-flight request included. |
| `TraceLevel` | `debug` | LogStream level for `Trace` messages. |
| `ExceptionLevel` | `error` | LogStream level for `Trace` calls with an exception. |
| `IncludeEnvironment` | `true` | Adds host, process and thread details to the context. |
| `UseActivityTraceId` | `true` | Sends the current `Activity` trace ID. |
| `StaticContext` | empty | Properties added to every entry's context. |
| `OnError` | `null` | Called with a `LogStreamException` when entries are dropped. |

`Validate()` throws `InvalidOperationException` for a missing or out-of-range setting; the connector calls it.

### `LogStreamConnector`

`[ConnectionName("LogStream")] public sealed class LogStreamConnector` — holds the options, the `HttpClient` and the ingest URI.

| Member | Description |
|---|---|
| `LogStreamConnector(LogStreamOptions options)` | Validates the options and creates an `HttpClient`. |
| `LogStreamConnector(LogStreamOptions options, HttpClient httpClient)` | Uses the given client. |
| `LogStreamConnector(IConnection connection, string appKey, string appId)` | Builds the options from a connection, as described above. |
| `Options`, `HttpClient`, `IngestUri` | The settings, client and `POST` endpoint. |

### `LogStreamLogProvider`

`public sealed class LogStreamLogProvider : ILogProvider, IDisposable`

| Member | Description |
|---|---|
| `LogStreamLogProvider(LogStreamConnector connector)` | Starts the background sender. |
| `LogStreamLogProvider(LogStreamOptions options)` | Creates a connector and starts the background sender. |
| `SetLevel(LogLevel level)` | Sets the enabled levels. |
| `Flush()`, `FlushAsync()`, `FlushAsync(CancellationToken)` | Sends every queued entry now. |
| `PendingCount` | Entries waiting to be sent. |
| `DroppedCount` | Entries dropped so far. |
| `Dispose()` | Stops the sender and sends what is left. |

### `LogStreamLevels`

Static properties for the six LogStream levels (`debug`, `info`, `notice`, `warning`, `error`, `critical`), `IsValid(string)` and `From(LogLevel, LogStreamOptions)`.

### `LogStreamException`

Passed to `OnError`. `StatusCode` and `ResponseBody` hold the server's answer when there was one.

## See also

- [Logging](../core/logging.md)
- [CrispyWaffle.Log4Net](log4net.md)
- [LogStream server](https://github.com/guibranco/logstream-server)
- [Serilog.Sinks.LogStream](https://github.com/guibranco/serilog-sinks-logstream), the Serilog sink for LogStream
