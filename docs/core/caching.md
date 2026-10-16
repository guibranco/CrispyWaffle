# Caching

The caching module gives you one static entry point, `CacheManager`, in front of one or more cache repositories. Writes go to every registered repository; reads walk the repositories in priority order and return the first hit. Use it when you want a fast in-process cache, a shared distributed cache, or both layered together.

The core package ships `MemoryCacheRepository`. Distributed repositories live in separate packages: `RedisCacheRepository` in [CrispyWaffle.Redis](../packages/redis.md) and `CouchDBCacheRepository` in [CrispyWaffle.CouchDB](../packages/couchdb.md). You can also write your own by implementing `ICacheRepository`.

| | |
|---|---|
| **Package** | `CrispyWaffle` |
| **Namespace** | `CrispyWaffle.Cache` |

## How it works

[![Caching write and read paths](../images/diagrams/caching.svg)](../images/diagrams/caching.svg)

**Registration and priority.** `CacheManager` keeps its repositories in a list sorted by an integer priority. Lower numbers are queried first. `AddRepository` overloads without a priority use an internal counter that starts at 0 and increments on every such call. When you pass an explicit priority that is already taken, the manager increments it until it finds a free slot, so a call never fails because of a collision. The repository list is static and lives for the life of the process; there is no way to remove a repository once added.

**Writes.** `SetAsync` calls `SetAsync` on every repository in parallel and waits for all of them with `Task.WhenAll`. A repository that throws is logged through `LogConsumer.Error` and skipped; the others still store the value. Cancellation inside a single repository is swallowed (a message is written to the console) and is not rethrown. `SetAsync` therefore never reports a partial failure to the caller. `SetToAsync` writes to exactly one repository, selected by its concrete type.

**Reads.** `GetAsync` and `TryGetAsync` query repositories one at a time, in priority order, through each repository's `TryGetAsync`. The first repository that reports success wins and the rest are not queried. A repository that throws is logged and skipped. When no repository has the key, `GetAsync` throws `InvalidOperationException` and `TryGetAsync` returns a failure result.

**Promotion to memory.** If a `MemoryCacheRepository` is registered and the hit comes from a different repository, the value is copied into the memory repository in a fire-and-forget background task (`SetToAsync<MemoryCacheRepository, T>`). Subsequent reads are served from memory if the memory repository has a lower priority number than the remote one. Promotion failures are logged and never surface to the caller. Promotion ignores the remote TTL, because the memory repository does not support expiration.

**Timeouts and cancellation.** Every read and remove operation that goes through repositories (`GetAsync`, `GetFrom`, `GetFromAsync`, `TryGetAsync`, `Remove`, `RemoveFrom`) runs under a fixed 10-second timeout linked to your `CancellationToken`. If the timeout fires, the manager throws `TimeoutException`. If your token is cancelled, the `OperationCanceledException` is rethrown. The timeout is not configurable. `SetAsync`, `SetToAsync` and `TTLAsync` do not use the timeout.

**TTL.** Expiration is a repository feature. `SetAsync(value, key, ttl)` passes the TTL to each repository; `MemoryCacheRepository` ignores it. `TTLAsync` asks each repository in priority order and returns the first non-zero value, or `TimeSpan.Zero` if every repository returns zero.

**Typed selection.** `SetToAsync`, `GetFrom`, `GetFromAsync` and `RemoveFrom` locate the repository whose runtime type is exactly `TCacheRepository` (not a base type or interface). If none matches they throw `InvalidOperationException`. If more than one registered repository has that type, `SingleOrDefault` throws `InvalidOperationException` as well.

## Getting started

```csharp
using System.Threading.Tasks;
using CrispyWaffle.Cache;

public static class Program
{
    public static async Task Main()
    {
        CacheManager.AddRepository(new MemoryCacheRepository());

        await CacheManager.SetAsync("some string text", "MyKey");

        var value = await CacheManager.GetAsync<string>("MyKey"); // "some string text"

        var (found, cached) = await CacheManager.TryGetAsync<string>("OtherKey");
        // found == false, cached == null

        await CacheManager.Remove("MyKey");
    }
}
```

## Reference

### `CacheManager`

`public static class CacheManager` — static facade that fans writes out to all registered `ICacheRepository` instances and reads from the first one that has the key.

All `key` and `subKey` arguments are strings. Every method that takes a `CancellationToken` defaults it to `default`. Methods check the token on entry and throw `OperationCanceledException` if it is already cancelled.

**Registration**

| Member | Description |
|---|---|
| `AddRepository<TCacheRepository>()` | Resolves `TCacheRepository` from [`ServiceLocator`](service-locator.md) and adds it with the next automatic priority. `TCacheRepository` must implement `ICacheRepository`. Returns the resolved repository as `ICacheRepository`. |
| `AddRepository(ICacheRepository repository)` | Adds an existing instance with the next automatic priority. Returns the same instance. |
| `AddRepository<TCacheRepository>(int priority)` | Resolves `TCacheRepository` from `ServiceLocator` and adds it at `priority` (or the next free slot above it). Returns the resolved repository. |
| `AddRepository(ICacheRepository repository, int priority)` | Adds an instance at `priority`, incrementing until a free slot is found. Returns the `int` priority actually used. |

**Writing**

| Member | Description |
|---|---|
| `SetAsync<T>(T value, string key, CancellationToken cancellationToken)` | Stores `value` under `key` in every repository, in parallel. Returns `ValueTask`. Per-repository errors are logged and swallowed. |
| `SetAsync<T>(T value, string key, string subKey, CancellationToken cancellationToken)` | Same as above, using a key plus sub-key (hash field). |
| `SetAsync<T>(T value, string key, TimeSpan ttl, CancellationToken cancellationToken)` | Same as the first overload, passing `ttl` to each repository. Repositories that do not support expiration ignore it. |
| `SetToAsync<TCacheRepository, TValue>(TValue value, string key, CancellationToken cancellationToken)` | Stores the value only in the repository whose type is exactly `TCacheRepository`. Throws `InvalidOperationException` if that repository is not registered. Errors from the repository itself are logged and swallowed. |
| `SetToAsync<TCacheRepository, TValue>(TValue value, string key, string subKey, CancellationToken cancellationToken)` | Same, with a sub-key. |
| `SetToAsync<TCacheRepository, TValue>(TValue value, string key, TimeSpan ttl, CancellationToken cancellationToken)` | Same, passing `ttl` to the repository. |
| `SetToAsync<TCacheRepository, TValue>(TValue value, string key, string subKey, TimeSpan ttl, CancellationToken cancellationToken)` | Stores the value with a sub-key in one repository. Note: `ttl` is currently ignored; the call is forwarded to the sub-key overload without a TTL. |

**Reading**

| Member | Description |
|---|---|
| `GetAsync<T>(string key, CancellationToken cancellationToken)` | Returns `ValueTask<T>` with the first value found, querying repositories in priority order and promoting remote hits to memory. Throws `InvalidOperationException` if no repository has the key, `TimeoutException` after 10 seconds. |
| `GetAsync<T>(string key, string subKey, CancellationToken cancellationToken)` | Same, for a key plus sub-key. |
| `TryGetAsync<T>(string key, CancellationToken cancellationToken)` | Returns `ValueTask<(bool Success, T Value)>`. `Success` is `false` and `Value` is `default(T)` when no repository has the key. Same ordering, promotion and timeout as `GetAsync`. |
| `TryGetAsync<T>(string key, string subKey, CancellationToken cancellationToken)` | Returns `ValueTask<bool>` only: `true` if any repository has the key and sub-key. The value itself is not returned (it is still promoted to memory). Use `GetAsync<T>(key, subKey)` to read it. |
| `GetFrom<TCacheRepository, TValue>(string key, CancellationToken cancellationToken)` | Reads from the single repository whose type is exactly `TCacheRepository`, via that repository's `GetAsync`. Returns `ValueTask<TValue>`. Throws `InvalidOperationException` if the repository is not registered or (for most repositories) if the key is missing, `TimeoutException` after 10 seconds. No promotion. |
| `GetFromAsync<TCacheRepository, TValue>(string key, string subKey, CancellationToken cancellationToken)` | Same as `GetFrom`, for a key plus sub-key. Note the `Async` suffix, which the single-key overload lacks. |
| `TTLAsync(string key)` | Returns `Task<TimeSpan>`: the first non-zero TTL reported by a repository in priority order, or `TimeSpan.Zero`. No cancellation token and no timeout. |

**Removing**

| Member | Description |
|---|---|
| `Remove(string key, CancellationToken cancellationToken)` | Removes `key` from every repository, sequentially in priority order. Returns `ValueTask`. Errors from individual repositories are swallowed without logging. Throws `TimeoutException` after 10 seconds. |
| `Remove(string key, string subKey, CancellationToken cancellationToken)` | Same, for a key plus sub-key. |
| `RemoveFrom<TCacheRepository>(string key, CancellationToken cancellationToken)` | Removes `key` only from the repository whose type is exactly `TCacheRepository`. Throws `InvalidOperationException` if it is not registered; repository errors propagate. |
| `RemoveFrom<TCacheRepository>(string key, string subKey, CancellationToken cancellationToken)` | Same, for a key plus sub-key. |

### `ICacheRepository`

`public interface ICacheRepository` — contract every cache backend implements so it can be registered with `CacheManager`.

Implement this to plug in your own store. `CacheManager` reads through `TryGetAsync`, so make sure it returns `(false, default)` rather than throwing on a miss.

| Member | Description |
|---|---|
| `ShouldPropagateExceptions` | `bool`, get/set. Whether the implementation should rethrow backend errors instead of swallowing them. Honoring it is up to the implementation. |
| `SetAsync<T>(T value, string key, TimeSpan? ttl = null, CancellationToken cancellationToken = default)` | Stores a value. `ttl` is the optional time to live. Returns `ValueTask`. |
| `SetAsync<T>(T value, string key, string subKey, CancellationToken cancellationToken = default)` | Stores a value under a key and sub-key. Returns `ValueTask`. |
| `GetAsync<T>(string key, CancellationToken cancellationToken = default)` / `GetAsync<T>(string key, string subKey, CancellationToken cancellationToken = default)` | Returns `ValueTask<T>`. Expected to throw `InvalidOperationException` when the key does not exist. |
| `TryGetAsync<T>(string key, CancellationToken cancellationToken = default)` / `TryGetAsync<T>(string key, string subKey, CancellationToken cancellationToken = default)` | Returns `ValueTask<(bool Success, T Value)>`. |
| `RemoveAsync(string key, CancellationToken cancellationToken = default)` / `RemoveAsync(string key, string subKey, CancellationToken cancellationToken = default)` | Removes the entry. Returns `Task`. |
| `TTLAsync(string key, CancellationToken cancellationToken = default)` | Returns `Task<TimeSpan>`: the remaining time to live, or `TimeSpan.Zero` if expired, missing or unsupported. |
| `Clear()` | Removes every entry. Returns `Task`. |

### `MemoryCacheRepository`

`public class MemoryCacheRepository : ICacheRepository` — in-process cache backed by `ConcurrentDictionary` instances.

The backing dictionaries are `static`, so every `MemoryCacheRepository` instance in the process shares the same data, and `Clear()` on one instance clears all of them. Values are stored as object references, not serialized copies: mutating a cached reference type mutates the cached value. There is no expiration and no size limit.

| Member | Description |
|---|---|
| `MemoryCacheRepository()` | Default constructor. |
| `ShouldPropagateExceptions` | Settable, but not used by this implementation. |
| `SetAsync<T>(T value, string key, TimeSpan? ttl, CancellationToken cancellationToken)` | Adds or replaces the value. `ttl` is ignored. |
| `SetAsync<T>(T value, string key, string subKey, CancellationToken cancellationToken)` | Adds or replaces the value in a separate sub-key store, under the composite key `"{key}-{subKey}"`. |
| `GetAsync<T>(string key, CancellationToken cancellationToken)` | Returns the value cast to `T`. Throws `InvalidOperationException` if the key is missing, `InvalidCastException` if the value is not a `T`. |
| `GetAsync<T>(string key, string subKey, CancellationToken cancellationToken)` | Same, for the sub-key store. |
| `TryGetAsync<T>(string key, CancellationToken cancellationToken)` / `TryGetAsync<T>(string key, string subKey, CancellationToken cancellationToken)` | Returns `(true, value)` when found and castable to `T`; `(false, default(T))` when missing or of a different type. |
| `RemoveAsync(string key, CancellationToken cancellationToken)` | Removes the key if present. |
| `RemoveAsync(string key, string subKey, CancellationToken cancellationToken)` | Intended to remove the sub-key entry. Note: it checks the main store for the composite key instead of the sub-key store, so in practice sub-key entries are not removed. |
| `TTLAsync(string key, CancellationToken cancellationToken)` | Always returns `TimeSpan.Zero`. |
| `Clear()` | Clears both the key store and the sub-key store. |

All methods except `TTLAsync` and `Clear` throw `OperationCanceledException` if the token is already cancelled.

## Examples

### Two-tier cache: memory in front of Redis

Register memory first so it gets the lower priority number and is queried first. A Redis hit is promoted into memory in the background. See [Redis](../packages/redis.md) for registering `RedisConnector`.

```csharp
using CrispyWaffle.Cache;
using CrispyWaffle.Composition;
using CrispyWaffle.Redis.Cache;

ServiceLocator.Register<MemoryCacheRepository>(Lifetime.Singleton);
ServiceLocator.Register<RedisCacheRepository>(Lifetime.Singleton);

CacheManager.AddRepository<MemoryCacheRepository>();  // priority 0
CacheManager.AddRepository<RedisCacheRepository>();   // priority 1

await CacheManager.SetAsync("some string text", "MyKey", TimeSpan.FromMinutes(5));

// Looks in memory first, then Redis.
var value = await CacheManager.GetAsync<string>("MyKey");

// Reads only from Redis, bypassing memory.
var fromRedis = await CacheManager.GetFrom<RedisCacheRepository, string>("MyKey");

// Remaining TTL as reported by the first repository that supports it (Redis here).
var ttl = await CacheManager.TTLAsync("MyKey");
```

### Explicit priorities

```csharp
CacheManager.AddRepository(new MemoryCacheRepository(), 0);
CacheManager.AddRepository<RedisCacheRepository>(10);
var used = CacheManager.AddRepository(new MyCacheRepository(), 10); // 10 is taken, so used == 11
```

### Key and sub-key

Sub-keys group related values under one key (a hash in Redis).

```csharp
await CacheManager.SetAsync(42, "user:1", "score");

int score = await CacheManager.GetAsync<int>("user:1", "score");

bool exists = await CacheManager.TryGetAsync<int>("user:1", "score"); // value is not returned

await CacheManager.Remove("user:1", "score");
```

### Handling timeouts and misses

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

try
{
    var order = await CacheManager.GetAsync<Order>("order:123", cts.Token);
}
catch (InvalidOperationException)
{
    // Not found in any repository.
}
catch (TimeoutException)
{
    // The internal 10-second timeout elapsed.
}
catch (OperationCanceledException)
{
    // Your token was cancelled.
}
```

### Custom repository

```csharp
public sealed class MyCacheRepository : ICacheRepository
{
    public bool ShouldPropagateExceptions { get; set; }

    public ValueTask SetAsync<T>(T value, string key, TimeSpan? ttl = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public ValueTask SetAsync<T>(T value, string key, string subKey, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public ValueTask<T> GetAsync<T>(string key, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public ValueTask<T> GetAsync<T>(string key, string subKey, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public ValueTask<(bool Success, T Value)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public ValueTask<(bool Success, T Value)> TryGetAsync<T>(string key, string subKey, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task RemoveAsync(string key, string subKey, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<TimeSpan> TTLAsync(string key, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task Clear() => throw new NotImplementedException();
}

CacheManager.AddRepository(new MyCacheRepository());
```

## See also

- [Redis](../packages/redis.md) — `RedisCacheRepository`
- [CouchDB](../packages/couchdb.md) — `CouchDBCacheRepository`
- [Service locator](service-locator.md)
- [Logging](logging.md) — where `CacheManager` writes its trace and error messages
- [Diagrams](../diagrams.md)
