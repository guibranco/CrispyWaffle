# Getting started

This page wires up the most common CrispyWaffle features in a console application: dependency registration, logging, caching, events and commands. Each step links to the module page with the full reference.

## 1. Install the core package

```bash
dotnet add package CrispyWaffle
```

Integration packages (Redis, RabbitMQ, ElasticSearch and others) are installed the same way. See [Installation](installation.md) for the full list.

## 2. Register services

CrispyWaffle features are exposed as static facades (`LogConsumer`, `CacheManager`, `EventsConsumer`, `CommandsConsumer`). They find their collaborators through the [`ServiceLocator`](core/service-locator.md), so register those first.

```csharp
using CrispyWaffle.Composition;
using CrispyWaffle.Log.Adapters;
using CrispyWaffle.Log.Handlers;

ServiceLocator.Register<IConsoleLogAdapter, StandardConsoleLogAdapter>(Lifetime.Singleton);
ServiceLocator.Register<IExceptionHandler, NullExceptionHandler>(Lifetime.Singleton);
```

For larger applications, group registrations in a class that implements `IBootstrapper` and call `ServiceLocator.RegisterBootstrapper<TBootstrapper>()`.

## 3. Log

Add one or more providers to [`LogConsumer`](core/logging.md). Every message goes to every provider. Providers start at `LogLevel.Production` (fatal, error, warning and info), so raise the level to see trace and debug output.

```csharp
using CrispyWaffle.Log;
using CrispyWaffle.Log.Providers;

LogConsumer.AddProvider<ConsoleLogProvider>().SetLevel(LogLevel.All);

LogConsumer.Info("Application started");
LogConsumer.Debug("Current time: {0:HH:mm:ss}", DateTime.Now);
```

## 4. Cache

Add one or more repositories to [`CacheManager`](core/caching.md). Writes go to every repository; reads return the first hit in priority order.

```csharp
using CrispyWaffle.Cache;

CacheManager.AddRepository<MemoryCacheRepository>();

await CacheManager.SetAsync("Crispy Waffle", "app-name");
await CacheManager.SetAsync("cached for 10 minutes", "short-lived", TimeSpan.FromMinutes(10));

var name = await CacheManager.GetAsync<string>("app-name");
```

## 5. Raise events

An event is a class implementing `IEvent`; every `IEventHandler<TEvent>` implementation handles it. The `ServiceLocator` discovers handlers by scanning the loaded assemblies, so they need no explicit registration. See [Events](core/events.md).

```csharp
using CrispyWaffle.Events;

public sealed class UserCreatedEvent : IEvent
{
    public UserCreatedEvent(string email) => Email = email;

    public string Email { get; }
}

public sealed class SendWelcomeEmail : IEventHandler<UserCreatedEvent>
{
    public void Handle(UserCreatedEvent args) =>
        LogConsumer.Info("Sending welcome e-mail to {0}", args.Email);
}

EventsConsumer.Raise(new UserCreatedEvent("someone@example.com"));
```

## 6. Send commands

A command has exactly one handler, which returns a result. If a handler with a parameterless constructor is the only implementation, it is resolved automatically; otherwise register it. See [Commands](core/commands.md).

```csharp
using CrispyWaffle.Commands;

public sealed class SumCommand : ICommand
{
    public int A { get; init; }
    public int B { get; init; }
}

public sealed class SumCommandHandler : ICommandHandler<SumCommand, int>
{
    public int Handle(SumCommand command) => command.A + command.B;
}

var total = CommandsConsumer.Raise<SumCommand, int>(new SumCommand { A = 2, B = 3 });
```

## Next steps

- Browse the [core modules](core/service-locator.md) and [integration packages](packages/configuration.md) in the navigation.
- See how the pieces fit together on the [Diagrams](diagrams.md) page.
- Look at the [test projects](https://github.com/guibranco/CrispyWaffle/tree/main/Tests) for more working examples.
