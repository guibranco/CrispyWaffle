# Service Locator

`ServiceLocator` is the small inversion-of-control container built into the core package. You register contracts (interfaces or classes) with an implementation, an instance, or a factory, and resolve them anywhere through static methods. Constructor dependencies are resolved recursively, so most classes need no registration at all.

The rest of the library relies on it: `EventsConsumer` and `CommandsConsumer` find their handlers through it, and other modules resolve their collaborators from it.

| | |
|---|---|
| **Package** | `CrispyWaffle` |
| **Namespace** | `CrispyWaffle.Composition` |

## How it works

[![Events and commands dispatch through the service locator](../images/diagrams/events-commands.svg)](../images/diagrams/events-commands.svg)

### Startup

The first time any `ServiceLocator` member is touched, its static constructor runs:

1. It loads every assembly referenced by the assemblies already in the `AppDomain` (one level deep). Assemblies that fail to load are skipped.
2. It builds `TypesCache`, a list of every type in every loaded assembly. This list is never refreshed, so types in assemblies loaded later are not visible to auto-registration or `ResolveAll`.
3. It registers `CancellationToken`, backed by an internal `CancellationTokenSource` (see `RequestCancellation`).

### Registration

Every `Register` overload stores a factory delegate keyed by the contract type. Each contract can be registered only once: a second registration for the same contract throws `ArgumentException` ("An item with the same key has already been added"). The same applies to `RegisterDependencyResolver`.

| Lifetime | Behavior |
|---|---|
| `Lifetime.Transient` (default) | The factory runs on every resolve. |
| `Lifetime.Singleton` | The instance is created lazily on the first resolve and reused afterwards. Creation is guarded by a lock per implementation type. |
| `Register<TContract>(TContract instance)` | The supplied instance is always returned. |

### Resolution

`Resolve<T>()` and `GetInstance(type)` follow these steps:

1. If the type is registered, the registered factory is invoked.
2. Otherwise, if the type is concrete (not abstract and not an interface), a new instance is created every time. Unregistered concrete types therefore behave as transient.
3. Otherwise (interface or abstract class), the locator tries auto-registration: it looks in `TypesCache` for non-abstract types that implement the contract and have a public parameterless constructor.
   - No candidates: nothing is resolved.
   - More than one candidate: `TooManyImplementationsException` is thrown.
   - Exactly one candidate: it is instantiated and stored as a singleton for that contract.
4. If nothing was resolved, `Resolve` and `GetInstance` throw `InvalidOperationException` ("No registrations for ..."). `TryResolve` and `TryGetInstance` return `null` instead.

When an instance is created, the locator picks a public constructor:

- With one public constructor, that constructor is used.
- With several, constructors that take a simple type (value types, `string`, `decimal`, `DateTime`, `DateTimeOffset`, `TimeSpan`, `Guid`, ...) or the type itself are discarded. Of the remaining ones, the constructor with the most parameters whose dependencies can all be resolved wins. Probing resolves the dependencies, so their constructors run during selection.

Each constructor parameter is resolved recursively. If a dependency resolver is registered for the parameter type, it is asked for the value instead (see `IDependencyResolver`). A parameter whose type cannot be resolved (an interface with no implementation, for example) is passed as `null` instead of causing an error. The parameter list chosen for each type is cached. Any exception during creation is wrapped in `InvalidOperationException` ("Unable to create instance of type ...").

### Resolving all implementations

`ResolveAll<T>()` does not use registrations to find implementations. It scans `TypesCache` for every non-abstract type assignable to `T` (the list is cached per contract) and resolves each concrete type individually with `GetInstance`. A concrete type that is itself registered (for example as a singleton) is resolved through that registration. Results come back in `TypesCache` order, not registration order.

### Disposal and cancellation

`DisposeAllRegistrations()` sends a dependency-usage count for every registered contract to `TelemetryAnalytics.TrackDependency`, then disposes registrations whose contract type implements `IDisposable` and that were resolved at least once. It is also called by a finalizer when the static state is collected. Only the contract type is checked: a singleton registered as `Register<IFoo, Foo>()` is not disposed when `IFoo` does not extend `IDisposable`, even if `Foo` does. For a transient registration, disposal invokes the factory, so a new instance is created and immediately disposed.

### Thread safety and limitations

- Registration and resolution use concurrent dictionaries and per-type locks for singleton creation, but the per-contract usage counters are a plain `Dictionary`. Register everything at startup, before resolving from several threads.
- `Register<T>()` and `Register<T>(Lifetime.Transient)` on a single concrete type cause infinite recursion (a `StackOverflowException`) when the type is resolved, because the transient factory resolves the same type again. To get a new instance on every resolve, do not register the class (unregistered concrete types are already transient) or register a factory with `Register<T>(() => new T())`. `Register<T>(Lifetime.Singleton)` works as expected.
- `ResolveWith<T>` and `GetInstanceWith` currently fail for every type: the argument list is passed to `Activator.CreateInstance` as a single object instead of an array, so creation throws and is reported as `InvalidOperationException` ("Unable to create instance of type ... using parameters"). Use a factory registration instead.

## Getting started

```csharp
using System;
using CrispyWaffle.Composition;

public interface IClock
{
    DateTime Now { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime Now => DateTime.UtcNow;
}

public sealed class ReportService
{
    private readonly IClock _clock;

    public ReportService(IClock clock) => _clock = clock;

    public string Header() => $"Report generated at {_clock.Now:O}";
}

public static class Program
{
    public static void Main()
    {
        // Register the contract once, at startup.
        ServiceLocator.Register<IClock, SystemClock>(Lifetime.Singleton);

        // ReportService is not registered: it is created on demand and
        // its IClock dependency is resolved from the registration above.
        var service = ServiceLocator.Resolve<ReportService>();
        Console.WriteLine(service.Header());
    }
}
```

## Reference

### `ServiceLocator`

`public static class ServiceLocator` — registers contracts and resolves instances.

| Member | Description |
|---|---|
| `TypesCache` | `List<Type>` of every type in the assemblies loaded when the locator was initialized. Used by auto-registration and `ResolveAll`. |
| `RegisterBootstrapper<TBootstrapper>()` | Creates `TBootstrapper` (constraint: `class, IBootstrapper, new()`) and calls its `RegisterServices()` method. |
| `Register<TContract>(TContract instance)` | Registers an existing instance; every resolve of `TContract` returns it. |
| `Register<TImplementation>()` / `Register<TImplementation>(Lifetime lifetime)` | Registers a class as its own contract. The default lifetime is `Transient`. Only the `Singleton` form is usable; see the limitations above. |
| `Register<TContract, TImplementation>()` / `Register<TContract, TImplementation>(Lifetime lifetime)` | Maps `TContract` to `TImplementation` (constraint: `TImplementation : TContract`). The default lifetime is `Transient`. `TImplementation` is created with constructor injection. |
| `Register<TContract>(Func<TContract> instanceCreator)` / `Register<TContract>(Func<TContract> instanceCreator, Lifetime lifetime)` | Registers a factory. The default lifetime is `Transient`; with `Singleton`, the factory runs once on the first resolve. |
| `RegisterDependencyResolver<TContract, TImplementation>()` | Registers an `IDependencyResolver` (constraint: `TImplementation : IDependencyResolver`) used whenever a constructor parameter of type `TContract` is resolved. The resolver is created immediately. |
| `RegisterDependencyResolver<TContract>(IDependencyResolver resolver)` | Same as above, with an existing resolver instance. |
| `Resolve<T>()` | Returns an instance of `T`. Throws `InvalidOperationException` when nothing can be resolved, and `TooManyImplementationsException` when auto-registration finds several candidates. |
| `TryResolve<T>()` | Like `Resolve<T>()` but returns `null` (`default`) when nothing can be resolved. Creation errors and `TooManyImplementationsException` still propagate. |
| `ResolveAll<T>()` | Returns `IEnumerable<T>` with one instance of every concrete type assignable to `T` in `TypesCache`. Instances are created lazily as the sequence is enumerated. |
| `ResolveWith<T>(Dictionary<int, object> parameters)` | Intended to create `T` using the given values for the constructor parameters at the given zero-based positions, resolving the rest. Currently always throws `InvalidOperationException`; see the limitations above. |
| `GetInstance(Type contract)` | Non-generic form of `Resolve<T>()`. Returns `object`. |
| `TryGetInstance(Type contract)` | Non-generic form of `TryResolve<T>()`. Returns `object` or `null`. |
| `GetInstanceWith(Type contract, Dictionary<int, object> parameters)` | Non-generic form of `ResolveWith<T>()`, with the same limitation. |
| `RequestCancellation()` | Cancels the `CancellationToken` that the locator injects into constructors and returns from `Resolve<CancellationToken>()`. Returns `true` when cancellation was requested. |
| `DisposeAllRegistrations()` | Reports usage counts to telemetry and disposes resolved registrations whose contract implements `IDisposable`. |

### `IBootstrapper`

`public interface IBootstrapper` — groups the registrations of an application or module in one class.

| Member | Description |
|---|---|
| `void RegisterServices()` | Performs the registrations. Called by `ServiceLocator.RegisterBootstrapper<TBootstrapper>()`. |

### `IDependencyResolver`

`public interface IDependencyResolver` — supplies a constructor dependency based on the type that requests it.

Register a resolver for a dependency type with `RegisterDependencyResolver`. Whenever the locator builds a type whose constructor has a parameter of that dependency type, it calls the resolver instead of the normal resolution. This lets you hand out different instances (for example, different connection strings) to different consumers. Resolvers are not consulted by a direct `Resolve<TContract>()` call.

| Member | Description |
|---|---|
| `object Resolve(Type type, int order)` | `type` is the class being constructed (the consumer), not the dependency type. `order` is the zero-based position of the parameter in the chosen constructor. Return the instance to inject. An `InvalidOperationException` thrown here is rethrown as `InvalidOperationException` ("No registrations for ...") naming the consumer type. |

### `Lifetime`

`public enum Lifetime` — the lifetime of a registration.

| Value | Description |
|---|---|
| `Transient` | A new instance is created on every resolve. Human-readable value: `Transient`. |
| `Singleton` | One instance is created on the first resolve and reused. When the implementation (or, for factories, the contract) implements `IDisposable`, the instance is held in a `LazyDisposable<T>`. Human-readable value: `Singleton`. |

### `LazyDisposable<T>`

`public class LazyDisposable<T> : Lazy<T>, IDisposable where T : IDisposable` — a `Lazy<T>` that disposes its value when it is disposed.

Disposing an instance whose value was never created does nothing.

| Member | Description |
|---|---|
| `LazyDisposable()` / `LazyDisposable(bool isThreadSafe)` / `LazyDisposable(LazyThreadSafetyMode mode)` | Constructors. The value is created with the default constructor of `T`, using the given thread-safety setting. |
| `LazyDisposable(Func<T> valueFactory)` / `LazyDisposable(Func<T> valueFactory, bool isThreadSafe)` / `LazyDisposable(Func<T> valueFactory, LazyThreadSafetyMode mode)` | Constructors. The value is created by `valueFactory`, using the given thread-safety setting. |
| `Dispose()` | Disposes the value if it has been created and suppresses finalization. |
| `protected virtual Dispose(bool disposing)` | Override to add cleanup. The base implementation disposes `Value` when `disposing` is `true` and `IsValueCreated` is `true`. |

Members inherited from `Lazy<T>` (`Value`, `IsValueCreated`) behave as in the base class.

### `TooManyImplementationsException`

`[Serializable] public class TooManyImplementationsException : Exception` — thrown when auto-registration finds more than one candidate.

Thrown by `Resolve`, `TryResolve`, `GetInstance` and `TryGetInstance` when an unregistered interface or abstract type has more than one non-abstract implementation with a public parameterless constructor. The message is "The type {FullName} has many implementations available, please consider registering one of them". Register the implementation you want to fix it.

| Member | Description |
|---|---|
| `TooManyImplementationsException(Type type)` | Creates the exception for the contract `type`. |
| `protected TooManyImplementationsException(SerializationInfo info, StreamingContext context)` | Serialization constructor. |

## Examples

### Group registrations in a bootstrapper

Implement `IBootstrapper` and register it once at startup.

```csharp
using CrispyWaffle.Composition;

// At startup:
ServiceLocator.RegisterBootstrapper<AppBootstrapper>();

var greeter = ServiceLocator.Resolve<IGreeter>();

public interface IGreeter
{
    string Greet(string name);
}

public sealed class Greeter : IGreeter
{
    public string Greet(string name) => $"Hello, {name}!";
}

public sealed class AppBootstrapper : IBootstrapper
{
    public void RegisterServices()
    {
        ServiceLocator.Register<IGreeter, Greeter>(Lifetime.Singleton);
    }
}
```

### Register a singleton built by a factory

Use a factory when the instance needs values that cannot be injected, such as configuration strings. This pattern comes from the test fixtures.

```csharp
using CrispyWaffle.Composition;
using CrispyWaffle.Configuration;

ServiceLocator.Register<ISecureCredentialProvider>(
    () =>
        new SecureCredentialProvider
        {
            PasswordHash = "my-password-hash",
            SaltKey = "my-salt-key",
            IVKey = "my-16-char-iv-key",
        },
    Lifetime.Singleton
);
```

### Check singleton and transient behavior

A class registered as a singleton is created once. An unregistered concrete class is created on every resolve.

```csharp
using System;
using CrispyWaffle.Composition;

ServiceLocator.Register<SingletonTest>(Lifetime.Singleton);

var a = ServiceLocator.Resolve<SingletonTest>();
var b = ServiceLocator.Resolve<SingletonTest>();
Console.WriteLine(a.Id == b.Id); // True

// Not registered: a new instance every time.
var c = ServiceLocator.Resolve<TransientTest>();
var d = ServiceLocator.Resolve<TransientTest>();
Console.WriteLine(c.Id == d.Id); // False

public sealed class SingletonTest
{
    public Guid Id { get; } = Guid.NewGuid();
}

public sealed class TransientTest
{
    public Guid Id { get; } = Guid.NewGuid();
}
```

### Inject a value that depends on the consumer

A dependency resolver returns a different `IConnection` for each class that asks for one.

```csharp
using System;
using CrispyWaffle.Composition;

ServiceLocator.RegisterDependencyResolver<IConnection, ConnectionResolver>();

var repository = ServiceLocator.Resolve<OrdersRepository>();
Console.WriteLine(repository.Connection.Name); // orders-db

public interface IConnection
{
    string Name { get; }
}

public sealed class Connection : IConnection
{
    public Connection(string name) => Name = name;

    public string Name { get; }
}

public sealed class ConnectionResolver : IDependencyResolver
{
    public object Resolve(Type type, int order) =>
        new Connection(type == typeof(OrdersRepository) ? "orders-db" : "default-db");
}

public sealed class OrdersRepository
{
    public OrdersRepository(IConnection connection) => Connection = connection;

    public IConnection Connection { get; }
}
```

### Stop background work with the shared cancellation token

Any class that takes a `CancellationToken` in its constructor receives the locator's token. `RequestCancellation` cancels it.

```csharp
using System.Threading;
using CrispyWaffle.Composition;

var worker = ServiceLocator.Resolve<Worker>();

// Later, for example on application shutdown:
ServiceLocator.RequestCancellation();

// worker.CancellationToken.IsCancellationRequested is now true.

public sealed class Worker
{
    public Worker(CancellationToken cancellationToken) => CancellationToken = cancellationToken;

    public CancellationToken CancellationToken { get; }
}
```

## See also

- [Events](events.md)
- [Commands](commands.md)
- [Telemetry](telemetry.md)
- [Diagrams](../diagrams.md)
