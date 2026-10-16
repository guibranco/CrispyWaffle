# CrispyWaffle.ElasticSearch

CrispyWaffle.ElasticSearch connects CrispyWaffle to [Elasticsearch](https://www.elastic.co/elasticsearch) through the official `Elastic.Clients.Elasticsearch` client. It provides:

- A log provider that indexes every log message as a document and deletes old log documents once, shortly after startup.
- A small wrapper for reading and writing documents.
- Index helpers that derive index names from your types.

Use it when you want CrispyWaffle logs searchable in Elasticsearch or Kibana, or when you need simple document storage without writing client code yourself.

| | |
|---|---|
| **Package** | `CrispyWaffle.ElasticSearch` |
| **Namespace** | `CrispyWaffle.ElasticSearch.Log`, `CrispyWaffle.ElasticSearch.Helpers`, `CrispyWaffle.ElasticSearch.Utils.Communications` |
| **Install** | `dotnet add package CrispyWaffle.ElasticSearch` |
| **Depends on** | `CrispyWaffle`, `CrispyWaffle.Configuration`, `Elastic.Clients.Elasticsearch` |

## How it works

### Connecting

`ElasticConnector` builds an `ElasticsearchClient` from an `IConnection`:

- The scheme is `https` when `Port` is `443`, and `http` otherwise.
- Basic authentication is added only when `Credentials` isn't `null` and `Credentials.Username` isn't blank.
- The client's default index is the `defaultIndexName` constructor argument. When you don't pass one, it's `logs-{EnvironmentHelper.ApplicationName}-{EnvironmentHelper.Version}`.

Register the connector as a singleton. `ElasticWrapper` and `ElasticSearchLogProvider` take it in their constructors. The static `Extensions` class resolves it from `ServiceLocator`.

### Log provider

[![Logging pipeline](../images/diagrams/logging-pipeline.svg)](../images/diagrams/logging-pipeline.svg)

`ElasticSearchLogProvider` is an `ILogProvider`. Each logging call that passes the level check builds a `LogMessage` and indexes it in the connector's default index. The `LogMessage` carries:

- The application name, date, host name and process ID.
- The local and remote IP addresses.
- The operation, user agent, thread ID and thread name.

Indexing calls are started but not awaited. Logging never blocks, and indexing errors aren't reported back to the caller.

The provider's level starts with no flags set, so call `SetLevel` after adding it.

Ten seconds after it's constructed, the provider runs a single delete-by-query against the default index. The query removes `LogMessage` documents whose `Date` is older than `logRetentionDays` days. This cleanup runs once per provider instance, not on a schedule.

`AbortGarbageCollector()` and `Dispose()` cancel the cleanup if it hasn't run yet.

### Index names

`Extensions.GetIndexName<T>()` decides which index a type belongs to:

- If the type has an `[IndexName]` attribute, that name is used.
- Otherwise, the type name in lowercase is used (with spaces replaced by `-`).

`ElasticWrapper`'s `*WithResolver` methods and the `Extensions` methods use this name. Types used with them must implement the marker interface `IIndexable`.

## Getting started

Start Elasticsearch locally. The repository's `docker-compose.yml` runs `elasticsearch:8.8.0` as a single node on port 9200 with security disabled.

```csharp
using CrispyWaffle.Composition;
using CrispyWaffle.Configuration;
using CrispyWaffle.ElasticSearch.Log;
using CrispyWaffle.ElasticSearch.Utils.Communications;
using CrispyWaffle.Log;

var connection = new Connection { Host = "localhost", Port = 9200 };

var connector = new ElasticConnector(connection, "logs-my-app");
ServiceLocator.Register(connector);

// Index log messages and delete log documents older than 30 days.
LogConsumer
    .AddProvider(new ElasticSearchLogProvider(connector, 30))
    .SetLevel(LogLevel.Production);

LogConsumer.Info("Application started");
```

## Reference

### `ElasticConnector`

`public sealed class ElasticConnector` creates and holds the `ElasticsearchClient`. It has the `[ConnectionName("ElasticSearch")]` attribute.

| Member | Description |
|---|---|
| `ElasticConnector(IConnection connection)` | Uses `logs-{EnvironmentHelper.ApplicationName}-{EnvironmentHelper.Version}` as the default index. |
| `ElasticConnector(IConnection connection, string defaultIndexName)` | Builds the client for `connection.Host:connection.Port` with `defaultIndexName` as the default index. Throws `ArgumentNullException` when `connection` is `null`. |
| `Client` | The `ElasticsearchClient`. Use it for anything the wrapper doesn't cover. |
| `DefaultIndexName` | The default index name passed to the client. |

### `ElasticSearchLogProvider`

`public class ElasticSearchLogProvider : ILogProvider, IDisposable` indexes log messages in Elasticsearch.

| Member | Description |
|---|---|
| `ElasticSearchLogProvider(ElasticConnector elastic, int logRetentionDays)` | Constructor. Uses `elastic.Client` and `elastic.DefaultIndexName`, and schedules the one-time retention cleanup to run 10 seconds later. |
| `SetLevel(LogLevel level)` | Sets which levels are indexed. Nothing is indexed until this is called. |
| `Fatal(category, message)` / `Error(category, message)` / `Warning(category, message)` / `Info(category, message)` / `Debug(category, message)` | Indexes a message at that level when the level is enabled. |
| `Trace(category, message)` / `Trace(category, message, exception)` / `Trace(category, exception)` | Indexes a trace message. The exception overloads also index the message and the stack trace of the exception and of each inner exception, as separate documents. |
| `Debug(string category, string content, string identifier)` | Indexes `content` at debug level with `identifier` as the `MessageIdentifier`. |
| `Debug<T>(string category, T content, string identifier, SerializerFormat customFormat = SerializerFormat.None)` | Serializes `content` with its default serializer (when `customFormat` is `None`) or with `customFormat`, and indexes it at debug level. `T` must be `class, new()`. |
| `AbortGarbageCollector()` | Cancels the pending retention cleanup. |
| `Dispose()` | Cancels the pending retention cleanup. The provider can still index messages afterwards. |

### `ElasticWrapper`

`public sealed class ElasticWrapper` has convenience methods for single-document reads and writes. The synchronous methods block on the asynchronous client calls with `.Result` or `.Wait()`. Prefer the `*Async` versions.

The `id` parameter is typed as `object` but is cast to `Elastic.Clients.Elasticsearch.Id`, so pass an `Id` instance such as `new Id("42")`. A plain string or number throws `InvalidCastException`.

| Member | Description |
|---|---|
| `ElasticWrapper(ElasticConnector elastic)` | Constructor. Uses `elastic.Client`. |
| `IndexExists<T>()` / `IndexExistsAsync<T>()` | Returns whether the index for `T` (from `GetIndexName<T>()`) exists. `T : class, IIndexable, new()`. |
| `GetDocument<T>(object id)` / `GetDocumentAsync<T>(object id)` | Gets document `id` from the index the client infers for `T` (the default index unless you configured a mapping), and returns its source. `T : class, new()`. |
| `GetDocumentWithResolver<T>(object id)` / `GetDocumentWithResolverAsync<T>(object id)` | Gets document `id` from the index named by `GetIndexName<T>()`. `T : class, IIndexable, new()`. |
| `GetDocumentFrom<T>(object id, string indexName)` / `GetDocumentFromAsync<T>(object id, string indexName)` | Gets document `id` from `indexName`. `T : class, new()`. |
| `SetDocument<T>(T document)` / `SetDocumentAsync<T>(T document)` | Indexes `document` in the index the client infers for `T`. `T : class, new()`. |
| `SetDocumentWithResolver<T>(T document)` / `SetDocumentWithResolverAsync<T>(T document)` | Indexes `document` in the index named by `GetIndexName<T>()`. `T : class, IIndexable, new()`. |
| `SetDocumentTo<T>(T document, string indexName)` / `SetDocumentToAsync<T>(T document, string indexName)` | Indexes `document` in `indexName`. `T : class, new()`. |

### `Extensions`

`public static class Extensions` (namespace `CrispyWaffle.ElasticSearch.Helpers`) has index-management helpers.

The extension methods use the `ElasticConnector` resolved from `ServiceLocator` the first time the class is used. Register the connector before that.

| Member | Description |
|---|---|
| `GetIndexName<T>()` | Returns the `[IndexName]` value of `T`, or the lowercase type name. `T : class, IIndexable, new()`. |
| `AliasAsync<T>(this T index, string alias, string indexName = null)` | Adds `alias` to `indexName`, or to `GetIndexName<T>()` when `indexName` is blank. Returns `index`. |
| `DeleteAsync<T>(this T index)` | Deletes the whole index named by `GetIndexName<T>()` if it exists, not just the document. Returns `index`. |
| `DeleteByQueryAsync<T>(this T index, Expression<Func<T, object>> field, string indexPattern, int daysBefore)` | Deletes documents in `indexPattern` whose date `field` is older than `daysBefore` days. Returns the number of deleted documents. `T : class, new()`. |

### `IIndexable`

`public interface IIndexable` is a marker interface with no members. Implement it on document types that you use with `GetIndexName<T>()`, the `*WithResolver` methods and the `Extensions` methods.

### `IndexNameAttribute`

`public sealed class IndexNameAttribute : Attribute` sets the index name for a class. Apply it to classes only.

| Member | Description |
|---|---|
| `IndexNameAttribute(string indexName)` | Constructor. |
| `IndexName` | The index name. |

## Examples

### Read and write a typed document

```csharp
using CrispyWaffle.ElasticSearch.Helpers;
using CrispyWaffle.ElasticSearch.Utils.Communications;
using Elastic.Clients.Elasticsearch;

[IndexName("products")]
public class Product : IIndexable
{
    public string Id { get; set; }

    public string Name { get; set; }
}

var wrapper = new ElasticWrapper(connector);

await wrapper.SetDocumentWithResolverAsync(new Product { Id = "42", Name = "Waffle iron" });

var product = await wrapper.GetDocumentWithResolverAsync<Product>(new Id("42"));
var exists = await wrapper.IndexExistsAsync<Product>();
```

### Remove old documents and add an alias

```csharp
using CrispyWaffle.ElasticSearch.Helpers;

public class AuditEntry : IIndexable
{
    public DateTime Date { get; set; }

    public string Action { get; set; }
}

var deleted = await new AuditEntry().DeleteByQueryAsync(e => e.Date, "auditentry*", 90);

await new AuditEntry().AliasAsync("audit-current");
```

## See also

- [Logging](../core/logging.md)
- [CrispyWaffle.Configuration](configuration.md)
- [Service locator](../core/service-locator.md)
