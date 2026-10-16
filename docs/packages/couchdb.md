# CrispyWaffle.CouchDB

CrispyWaffle.CouchDB provides a cache repository that stores documents in [Apache CouchDB](https://couchdb.apache.org/), using the CouchDB.NET driver. Use it when cached data should survive restarts and be readable from any process that can reach the CouchDB server, and when the cached values are documents rather than plain strings or numbers.

| | |
|---|---|
| **Package** | `CrispyWaffle.CouchDB` |
| **Namespace** | `CrispyWaffle.CouchDB.Cache`, `CrispyWaffle.CouchDB.Utils.Communications` |
| **Install** | Not yet published to NuGet (it is not in the release workflow's package list). Reference the project: `dotnet add reference Src/CrispyWaffle.CouchDB/CrispyWaffle.CouchDB.csproj` |
| **Depends on** | `CrispyWaffle`, `CrispyWaffle.Configuration`, `CouchDB.NET` |

## How it works

[![Caching](../images/diagrams/caching.svg)](../images/diagrams/caching.svg)

### Connecting

`CouchDBConnector` creates a CouchDB.NET `CouchClient` from an `IConnection`. The client URL is `"{Host}:{Port}"`, so `Host` must include the scheme, for example `http://localhost`. The client always uses basic authentication with `Credentials.Username` and `Credentials.Password`.

You can also pass a `CouchClient` that you have configured yourself.

### Documents and databases

Only types derived from `CouchDBCacheDocument` can be stored.

- `CouchDBCacheDocument.Key` is the CouchDB document ID, so one key holds at most one document. `SubKey` is an ordinary document field that the sub-key overloads use as an extra filter.
- The repository picks a database from the document type: the type name in lowercase followed by `s`. For example, `CouchDBCacheDocument` maps to `couchdbcachedocuments` and a `Car` subclass maps to `cars`. If that database isn't in the server's database list, it's created. If it's already there, the repository opens it with `CouchClient.GetDatabase<T>()`.

There are two groups of methods:

- **The `ICacheRepository` methods** (`SetAsync`, `GetAsync`, `TryGetAsync`, `RemoveAsync`, `TTLAsync`, `Clear`) always use the database of the base `CouchDBCacheDocument` type.
  - `SetAsync<T>` returns without storing anything when `T` doesn't derive from `CouchDBCacheDocument`, and `GetAsync<T>` returns `default`.
  - Values are read back as `CouchDBCacheDocument` and then cast to `T`.
- **The `*Specific*` methods** (`SetSpecificAsync<T>`, `GetSpecificAsync<T>`, `RemoveSpecificAsync<T>`, `Clear<T>`, `GetDocCount<T>`) use the database of `T`. Use them for your own document subclasses.

### Expiration

`SetSpecificAsync(value, key, ttl)` stores `ttl` in `TTL` and `DateTime.UtcNow + ttl` in `ExpiresAt`. CouchDB doesn't expire documents itself. Expiration is checked when a document is read:

- If `ExpiresAt` is set and in the past, the read deletes the document and returns `default`.
- The sub-key `SetAsync` and `SetSpecificAsync` overloads don't set a TTL.
- `TTLAsync` returns the stored `TTL` value, which is the original lifetime, not the time remaining.

### Errors

`ShouldPropagateExceptions` defaults to `false`.

When it's `false`:

- Most failures are passed to `LogConsumer.Handle` and the method returns normally.
- The `Remove*` methods and `TTLAsync` also log through `LogConsumer.Error`.
- `GetSpecificAsync` throws `InvalidOperationException` after logging a suppressed error.

In both modes:

- A document that doesn't exist is returned as `null`, not as an error.
- `OperationCanceledException` is logged as a warning and rethrown by most methods. The exception is `TryGetAsync<T>(key)`, which returns `(false, default)` on cancellation.

`Dispose()` disposes the `CouchDBConnector`, which disposes the `CouchClient`.

## Getting started

Start CouchDB locally. The repository's `docker-compose.yml` runs `couchdb:latest` on port 5984 with the user `Admin` and the password `myP@ssw0rd`.

```csharp
using CrispyWaffle.Cache;
using CrispyWaffle.Composition;
using CrispyWaffle.Configuration;
using CrispyWaffle.CouchDB.Cache;
using CrispyWaffle.CouchDB.Utils.Communications;

var connection = new Connection
{
    Host = "http://localhost", // include the scheme
    Port = 5984,
    Credentials = { Username = "Admin", Password = "myP@ssw0rd" },
};

ServiceLocator.Register(() => new CouchDBConnector(connection), Lifetime.Singleton);
ServiceLocator.Register(
    () => new CouchDBCacheRepository(ServiceLocator.Resolve<CouchDBConnector>()),
    Lifetime.Singleton
);

CacheManager.AddRepository<CouchDBCacheRepository>();

// Use the repository directly for typed documents.
var repository = ServiceLocator.Resolve<CouchDBCacheRepository>();

var doc = new CouchDBCacheDocument();
await repository.SetAsync(doc, Guid.NewGuid().ToString());

var stored = await repository.GetAsync<CouchDBCacheDocument>(doc.Key);
```

## Reference

### `CouchDBCacheRepository`

`public class CouchDBCacheRepository : ICacheRepository, IDisposable` is a cache repository that stores `CouchDBCacheDocument` instances in CouchDB.

| Member | Description |
|---|---|
| `CouchDBCacheRepository(CouchDBConnector connector)` | Constructor. |
| `ShouldPropagateExceptions` | When `true`, CouchDB errors are rethrown. When `false` (default), they are logged and suppressed. |
| `SetAsync<T>(T value, string key, TimeSpan? ttl = null, CancellationToken cancellationToken = default)` | When `T` derives from `CouchDBCacheDocument`, calls `SetSpecificAsync` with `value` cast to `CouchDBCacheDocument`, so it's stored in the base document database. Does nothing for other types. Returns `ValueTask`. |
| `SetAsync<T>(T value, string key, string subKey, CancellationToken cancellationToken = default)` | Same as above, but sets `Key` and `SubKey` (trimmed) and calls `CreateOrUpdateAsync`. Suppressed errors are logged with `LogConsumer.Error`. |
| `GetAsync<T>(string key, CancellationToken cancellationToken = default)` | Reads document `key` from the base document database and casts it to `T`. Returns `default` when `T` doesn't derive from `CouchDBCacheDocument`, or when the document is missing or expired. Returns `ValueTask<T>`. |
| `GetAsync<T>(string key, string subKey, CancellationToken cancellationToken = default)` | Same, but matches on both `Key` and `SubKey`. |
| `TryGetAsync<T>(string key, CancellationToken cancellationToken = default)` | Returns `(true, value)` when a document is found, otherwise `(false, default)`. Returns `(false, default)` on cancellation instead of throwing. |
| `TryGetAsync<T>(string key, string subKey, CancellationToken cancellationToken = default)` | Same, matching on `Key` and `SubKey`. Rethrows `OperationCanceledException`. |
| `RemoveAsync(string key, CancellationToken cancellationToken = default)` / `RemoveAsync(string key, string subKey, CancellationToken cancellationToken = default)` | Removes a document from the base document database. See `RemoveSpecificAsync`. |
| `TTLAsync(string key, CancellationToken cancellationToken = default)` | Returns the `TTL` stored on document `key`, or `TimeSpan.Zero` when there is no document or a suppressed error occurred. |
| `Clear()` / `Clear<T>()` | Deletes every document in the base document database, or in the database of `T`. Blocks until all deletes finish. |
| `GetDocCount<T>()` | Returns the number of documents with a non-null ID in the database of `T`. `T : CouchDBCacheDocument`. |
| `SetSpecificAsync<T>(T value, string key, TimeSpan? ttl = null, CancellationToken cancellationToken = default)` | Sets `value.Key`, sets `TTL` and `ExpiresAt` when `ttl` is given, and creates the document in the database of `T` with `CreateAsync`. `T : CouchDBCacheDocument`. Returns `Task`. |
| `SetSpecificAsync<T>(T value, string key, string subKey, CancellationToken cancellationToken = default)` | Sets `Key` and `SubKey` (trimmed) and calls `CreateOrUpdateAsync` in the database of `T`. |
| `GetSpecificAsync<T>(string key, CancellationToken cancellationToken = default)` | Finds document `key` in the database of `T`. Returns `null` when it doesn't exist. Deletes it and returns `null` when it has expired. Throws `InvalidOperationException` after a suppressed error. |
| `GetSpecificAsync<T>(string key, string subKey, CancellationToken cancellationToken = default)` | Same, matching on `Key` and `SubKey`. |
| `RemoveSpecificAsync<T>(string key, CancellationToken cancellationToken = default)` | Deletes document `key` from the database of `T`. Logs at info level whether the document was found. Throws `ArgumentException` when `key` is null, empty or whitespace. |
| `RemoveSpecificAsync<T>(string key, string subKey, CancellationToken cancellationToken = default)` | Same, matching on `Key` and `SubKey`. |
| `Dispose()` | Disposes the connector. |

### `CouchDBConnector`

`public class CouchDBConnector : IDisposable` holds the CouchDB.NET client. It has the `[ConnectionName("CouchDB")]` attribute.

| Member | Description |
|---|---|
| `CouchDBConnector(IConnection connection)` | Creates a `CouchClient` for `"{connection.Host}:{connection.Port}"` with basic authentication from `connection.Credentials`. |
| `CouchDBConnector(CouchClient client)` | Wraps an existing client. |
| `CouchDBClient` | The `CouchClient` instance. Use it for operations the repository doesn't cover. |
| `Dispose()` | Disposes the client. |
| `Dispose(bool disposing)` | Protected virtual. Disposes `CouchDBClient` when `disposing` is `true`. Override it in subclasses to release extra resources. |

### `CouchDBCacheDocument`

`public class CouchDBCacheDocument : CouchDocument` is the base class for everything stored by `CouchDBCacheRepository`. Derive from it to add your own fields. `Id` and `Rev` come from CouchDB.NET's `CouchDocument`.

| Member | Description |
|---|---|
| `Key` | The cache key. Reads and writes the document `Id`. |
| `SubKey` | Optional secondary key, stored as a field. |
| `ExpiresAt` | UTC time after which the document is treated as expired. `default` means it never expires. |
| `TTL` | The lifetime the document was stored with. |

## Examples

### Store a custom document type in its own database

Documents of type `Car` go to the `cars` database.

```csharp
using CrispyWaffle.CouchDB.Cache;

public class Car : CouchDBCacheDocument
{
    public Car(string maker) => Maker = maker;

    public string Maker { get; set; }
}

var car = new Car("MakerOne");
await repository.SetSpecificAsync(car, Guid.NewGuid().ToString());

var fromDb = await repository.GetSpecificAsync<Car>(car.Key);

await repository.RemoveSpecificAsync<Car>(car.Key);
```

### Store a document that expires

```csharp
var key = Guid.NewGuid().ToString();
await repository.SetAsync(new CouchDBCacheDocument(), key, TimeSpan.FromSeconds(5));

await Task.Delay(6000);

var expired = await repository.GetAsync<CouchDBCacheDocument>(key); // null; the document was deleted
```

### Clear a database and count documents

```csharp
await repository.Clear();
var remaining = await repository.GetDocCount<CouchDBCacheDocument>(); // 0
```

## See also

- [Caching](../core/caching.md)
- [CrispyWaffle.Configuration](configuration.md)
- [CrispyWaffle.Redis](redis.md)
