# Telemetry

The telemetry module is a small static facade, `TelemetryAnalytics`, that forwards counters (hits and metrics), stored events, exception types and dependency statistics to every registered telemetry client. The core package defines the contracts only. The storage is supplied by a client such as `RedisTelemetryClient` from the [Redis package](../packages/redis.md), or by your own `ITelemetryClient`.

Use it to count things (page hits, jobs run, metric variations) or to keep short-lived snapshots of domain objects, without tying your code to a particular store.

| | |
|---|---|
| **Package** | `CrispyWaffle` |
| **Namespace** | `CrispyWaffle.Telemetry` |

## How it works

`TelemetryAnalytics` keeps a static list of `ITelemetryClient` instances, filled with `AddClient`. With no clients registered, every call does nothing and the getters return `0` or `null`.

- **Write calls** (`TrackHit`, `TrackEvent`, `TrackMetric`, `TrackException`, `TrackDependency`, `RemoveHit`, `RemoveMetric`) are forwarded to every client, synchronously and in registration order. Return values from the clients' `Remove*` methods are discarded.
- **Read calls** ask each client in order and return the first useful answer: `GetHit` and `GetMetric` return the first non-zero value (or `0`), and `GetEvent` returns the first non-null object (or `null`).
- Every call writes a Trace-level message through `LogConsumer` (for example `Tracking hit of <name>`), so with Trace enabled on a provider you can see telemetry activity in the [logs](logging.md).

The library feeds telemetry itself in two places:

- `DefaultExceptionHandler` calls `TrackException` for exception types in the chain of every exception it handles (see [Logging](logging.md)).
- `ServiceLocator.DisposeAllRegistrations()` calls `TrackDependency` for each registered contract with the number of times it was resolved (see [Service locator](service-locator.md)).

The client list is not synchronized. Add clients at startup, before telemetry is used from multiple threads.

### Redis client

The [Redis package](../packages/redis.md) provides `RedisTelemetryClient`, which stores hits and metrics as Redis counters and events as serialized values with a TTL. It follows the convention described on `ITelemetryEvent<TEvent>`: `Name` is the Redis key and `Category` is the Redis database number.

## Getting started

Register a client at startup, then track and read a hit. This example uses the Redis client; see the [Redis package](../packages/redis.md) for how to build a `RedisConnector`.

```csharp
using CrispyWaffle.Composition;
using CrispyWaffle.Redis.Telemetry;
using CrispyWaffle.Redis.Utils.Communications;
using CrispyWaffle.Telemetry;

public static class TelemetrySetup
{
    public static void Configure(RedisConnector connector)
    {
        // The parameterless RedisTelemetryClient constructor resolves RedisConnector
        // from the service locator and uses a 30-day TTL for events.
        ServiceLocator.Register(connector);
        TelemetryAnalytics.AddClient(new RedisTelemetryClient());
    }

    public static int CountHomePageVisit()
    {
        TelemetryAnalytics.TrackHit("home-page");
        return TelemetryAnalytics.GetHit("home-page");
    }
}
```

## Reference

### `TelemetryAnalytics`

`public static class TelemetryAnalytics` — static facade that forwards telemetry calls to all registered clients.

| Member | Description |
|---|---|
| `AddClient<TTelemetryClient>()` | Resolves `TTelemetryClient` from `ServiceLocator`, adds it and returns it as `ITelemetryClient`. Constraint: `TTelemetryClient : ITelemetryClient`. |
| `AddClient(ITelemetryClient client)` | Adds an existing client instance and returns it. |
| `TrackHit(string hitName)` | Increments the hit counter on every client. |
| `GetHit(string hitName)` | Returns the first non-zero hit count from the clients, or `0`. |
| `RemoveHit(string hitName)` | Removes the hit counter from every client. Returns `void`. |
| `TrackEvent<TEvent>(ITelemetryEvent<TEvent> @event)` | Stores the event on every client, using each client's default TTL. Constraint: `TEvent : class, new()`. |
| `TrackEvent<TEvent>(ITelemetryEvent<TEvent> @event, TimeSpan ttl)` | Stores the event on every client with the given TTL. |
| `GetEvent<TEvent>(ITelemetryEvent<TEvent> @event)` | Returns the first non-null stored event data from the clients, or `null`. Only `@event.Name` and `@event.Category` are needed for the lookup. |
| `TrackMetric(string metricName, string variation)` | Increments the counter for a metric variation on every client. |
| `GetMetric(string metricName, string variation)` | Returns the first non-zero value for a metric variation, or `0`. |
| `RemoveMetric(string metricName, string variation)` | Removes a metric variation from every client. Returns `void`. |
| `TrackException(Type exceptionType)` | Records an occurrence of an exception type on every client. |
| `TrackDependency(Type interfaceType, int resolvedTimes)` | Records how many times a contract was resolved on every client. |

### `ITelemetryClient`

`public interface ITelemetryClient` — a telemetry store. Implement it to send telemetry to your own backend.

| Member | Description |
|---|---|
| `GetHit(string hitName)` | Returns the current count for a hit. |
| `TrackHit(string hitName)` | Increments a hit counter. |
| `RemoveHit(string hitName)` | Removes a hit counter. Returns `true` if it was removed. |
| `GetEvent<TEvent>(ITelemetryEvent<TEvent> @event)` | Returns the stored data for an event, or `null`. Constraint: `TEvent : class, new()`. |
| `TrackEvent<TEvent>(ITelemetryEvent<TEvent> @event)` | Stores an event's data. |
| `TrackEvent<TEvent>(ITelemetryEvent<TEvent> @event, TimeSpan ttl)` | Stores an event's data with a time-to-live. |
| `GetMetric(string metricName, string variation)` | Returns the current value of a metric variation. |
| `TrackMetric(string metricName, string variation)` | Increments a metric variation. |
| `RemoveMetric(string metricName, string variation)` | Removes a metric variation. Returns `true` if it was removed. |
| `TrackException(Type exceptionType)` | Records an exception type. |
| `TrackDependency(Type interfaceType, int resolvedTimes)` | Records a dependency's resolution count. |

### `ITelemetryEvent<TEvent>`

`public interface ITelemetryEvent<out TEvent> where TEvent : class, new()` — identifies a stored event and carries its data. Covariant in `TEvent`.

| Member | Description |
|---|---|
| `Name` | `string`. Key of the event. `RedisTelemetryClient` uses it as the Redis key. |
| `Category` | `int`. Partition of the event. `RedisTelemetryClient` uses it as the Redis database number. |
| `Event` | `TEvent`. The data to store. |

## Examples

### Storing and reading an event

Define an event type and store a snapshot for one hour.

```csharp
using System;
using CrispyWaffle.Telemetry;

var tracked = new OrderEvent(new Order { Id = 42, Status = "Shipped" });
TelemetryAnalytics.TrackEvent(tracked, TimeSpan.FromHours(1));

Order stored = TelemetryAnalytics.GetEvent(tracked);

public class Order
{
    public int Id { get; set; }
    public string Status { get; set; }
}

public sealed class OrderEvent : ITelemetryEvent<Order>
{
    public OrderEvent(Order order) => Event = order;

    public string Name => $"order:{Event.Id}";

    public int Category => 2;

    public Order Event { get; }
}
```

### Counting metric variations

Track which payment method customers choose.

```csharp
using CrispyWaffle.Telemetry;

TelemetryAnalytics.TrackMetric("payment-method", "credit-card");
TelemetryAnalytics.TrackMetric("payment-method", "pix");

var creditCard = TelemetryAnalytics.GetMetric("payment-method", "credit-card");
```

### Writing a custom client

An in-memory client, useful in tests.

```csharp
using System;
using System.Collections.Concurrent;
using CrispyWaffle.Telemetry;

TelemetryAnalytics.AddClient(new InMemoryTelemetryClient());
TelemetryAnalytics.TrackHit("startup");

public sealed class InMemoryTelemetryClient : ITelemetryClient
{
    private readonly ConcurrentDictionary<string, int> _counters = new();
    private readonly ConcurrentDictionary<string, object> _events = new();

    public int GetHit(string hitName) => _counters.TryGetValue(hitName, out var v) ? v : 0;

    public void TrackHit(string hitName) => _counters.AddOrUpdate(hitName, 1, (_, v) => v + 1);

    public bool RemoveHit(string hitName) => _counters.TryRemove(hitName, out _);

    public TEvent GetEvent<TEvent>(ITelemetryEvent<TEvent> @event)
        where TEvent : class, new() =>
        _events.TryGetValue($"{@event.Category}:{@event.Name}", out var v) ? v as TEvent : null;

    public void TrackEvent<TEvent>(ITelemetryEvent<TEvent> @event)
        where TEvent : class, new() =>
        _events[$"{@event.Category}:{@event.Name}"] = @event.Event;

    public void TrackEvent<TEvent>(ITelemetryEvent<TEvent> @event, TimeSpan ttl)
        where TEvent : class, new() => TrackEvent(@event);

    public int GetMetric(string metricName, string variation) => GetHit($"{metricName}:{variation}");

    public void TrackMetric(string metricName, string variation) => TrackHit($"{metricName}:{variation}");

    public bool RemoveMetric(string metricName, string variation) => RemoveHit($"{metricName}:{variation}");

    public void TrackException(Type exceptionType) => TrackHit($"exception:{exceptionType.FullName}");

    public void TrackDependency(Type interfaceType, int resolvedTimes) =>
        _counters[$"dependency:{interfaceType.FullName}"] = resolvedTimes;
}
```

## See also

- [Logging](logging.md)
- [Service locator](service-locator.md)
- [Redis package](../packages/redis.md)
