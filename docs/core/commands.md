# Commands

Commands ask the application to do something and return a result. A command is a plain class that implements `ICommand`. Exactly one handler, implementing `ICommandHandler<TCommand, TResult>` or `ICommandHandlerAsync<TCommand, TResult>`, processes it, and `CommandsConsumer` returns the handler's result to the caller.

Use commands for one-to-one requests that produce a value. To notify any number of listeners without a result, use [events](events.md).

| | |
|---|---|
| **Package** | `CrispyWaffle` |
| **Namespace** | `CrispyWaffle.Commands` |

## How it works

[![Events and commands dispatch](../images/diagrams/events-commands.svg)](../images/diagrams/events-commands.svg)

`CommandsConsumer.Raise<TCommand, TResult>(command)` calls `ServiceLocator.Resolve<ICommandHandler<TCommand, TResult>>()`, writes a trace message through `LogConsumer.Trace`, and returns the value of `Handle(command)`. `RaiseAsync` does the same with `ICommandHandlerAsync<TCommand, TResult>` and returns the handler's `Task<TResult>` directly. Both type arguments must be given explicitly, because `TResult` cannot be inferred.

The handler is found by the normal [service locator](service-locator.md) rules:

- If the handler interface is registered (for example `ServiceLocator.Register<ICommandHandler<MyCommand, MyResult>, MyCommandHandler>()`), that registration is used. Register the handler when its constructor takes dependencies.
- Otherwise the locator auto-registers the only non-abstract class that implements the interface and has a public parameterless constructor. The handler instance is then cached and reused as a singleton.
- If no handler is found, `Raise` throws `InvalidOperationException` ("No registrations for ..."). If more than one candidate is found, it throws `TooManyImplementationsException`.

The handler runs on the calling thread, and exceptions it throws propagate to the caller.

## Getting started

```csharp
using System;
using CrispyWaffle.Commands;

public sealed class HelloWorldCommand : ICommand { }

public sealed class HelloWorldCommandHandler : ICommandHandler<HelloWorldCommand, string>
{
    public string Handle(HelloWorldCommand command) =>
        "Hello World, this is the result of the triggered command!";
}

public static class Program
{
    public static void Main()
    {
        var result = CommandsConsumer.Raise<HelloWorldCommand, string>(new HelloWorldCommand());
        Console.WriteLine(result);
    }
}
```

## Reference

### `CommandsConsumer`

`public static class CommandsConsumer` — sends a command to its handler and returns the result.

| Member | Description |
|---|---|
| `TResult Raise<TCommand, TResult>(TCommand command)` | Constraint: `TCommand : ICommand`. Resolves `ICommandHandler<TCommand, TResult>` and returns `Handle(command)`. Throws `InvalidOperationException` when no handler can be resolved and `TooManyImplementationsException` when several unregistered candidates exist. |
| `Task<TResult> RaiseAsync<TCommand, TResult>(TCommand command)` | Constraint: `TCommand : ICommand`. Resolves `ICommandHandlerAsync<TCommand, TResult>` and returns `HandleAsync(command)`. Resolution errors are thrown synchronously, before a task is returned. |

### `ICommand`

`public interface ICommand` — marker interface for command classes.

It has no members. Put the command's input in properties of the class.

### `ICommandHandler<TCommand, TResult>`

`public interface ICommandHandler<in TCommand, out TResult> where TCommand : ICommand` — handles a command synchronously.

| Member | Description |
|---|---|
| `TResult Handle(TCommand command)` | Processes the command and returns its result. |

### `ICommandHandlerAsync<TCommand, TResult>`

`public interface ICommandHandlerAsync<in TCommand, TResult> where TCommand : ICommand` — handles a command asynchronously.

| Member | Description |
|---|---|
| `Task<TResult> HandleAsync(TCommand command)` | Processes the command and returns a task with its result. |

## Examples

### Returning a class as a result

The command carries input data and the handler returns a result object.

```csharp
using System;
using CrispyWaffle.Commands;
using CrispyWaffle.Log;

var result = CommandsConsumer.Raise<SomeCommand, SomeCommandResult>(new SomeCommand(10));
Console.WriteLine($"New quantity: {result.Quantity}");

public sealed class SomeCommand(int quantity) : ICommand
{
    public Guid Id { get; } = Guid.NewGuid();

    public DateTime Date { get; } = DateTime.Now;

    public int Quantity { get; } = quantity;
}

public sealed class SomeCommandResult(int newQuantity)
{
    public int Quantity { get; } = newQuantity;
}

public sealed class SomeCommandHandler : ICommandHandler<SomeCommand, SomeCommandResult>
{
    public SomeCommandResult Handle(SomeCommand command)
    {
        LogConsumer.Warning(
            "Command 'SomeCommand' handled by 'SomeCommandHandler'. Command Id: {0}",
            command.Id
        );

        return new SomeCommandResult(command.Quantity * 2);
    }
}
```

### Synchronous and asynchronous handling in one class

This pattern comes from the test suite. The same class implements both handler interfaces.

```csharp
using System;
using System.Threading.Tasks;
using CrispyWaffle.Commands;

var command = new TestDoneCommand(Guid.NewGuid(), "Sample test");

var result = CommandsConsumer.Raise<TestDoneCommand, TestDoneResultCommand>(command);
var resultAsync = await CommandsConsumer.RaiseAsync<TestDoneCommand, TestDoneResultCommand>(
    command
);

public sealed class TestDoneCommand(Guid identifier, string text) : ICommand
{
    public Guid Identifier { get; } = identifier;

    public string Text { get; } = text;

    public DateTime CreatedDateTime { get; } = DateTime.UtcNow;
}

public sealed class TestDoneResultCommand(string text)
{
    public string Text { get; } = text;
}

public sealed class TestDoneCommandHandler
    : ICommandHandler<TestDoneCommand, TestDoneResultCommand>,
        ICommandHandlerAsync<TestDoneCommand, TestDoneResultCommand>
{
    public TestDoneResultCommand Handle(TestDoneCommand command) =>
        new($"Sample done action handled: {command.Identifier} - {command.Text}");

    public Task<TestDoneResultCommand> HandleAsync(TestDoneCommand command) =>
        Task.Run(() =>
            new TestDoneResultCommand(
                $"Sample done action handled: {command.Identifier} - {command.Text}"
            )
        );
}
```

### A handler with dependencies

Handlers without a parameterless constructor are not auto-registered. Register them explicitly; their dependencies are injected by the service locator.

```csharp
using CrispyWaffle.Commands;
using CrispyWaffle.Composition;

ServiceLocator.Register<IPriceCatalog, FixedPriceCatalog>(Lifetime.Singleton);
ServiceLocator.Register<ICommandHandler<GetPriceCommand, decimal>, GetPriceCommandHandler>();

var price = CommandsConsumer.Raise<GetPriceCommand, decimal>(new GetPriceCommand("SKU-1"));

public interface IPriceCatalog
{
    decimal PriceOf(string sku);
}

public sealed class FixedPriceCatalog : IPriceCatalog
{
    public decimal PriceOf(string sku) => 9.99m;
}

public sealed class GetPriceCommand(string sku) : ICommand
{
    public string Sku { get; } = sku;
}

public sealed class GetPriceCommandHandler : ICommandHandler<GetPriceCommand, decimal>
{
    private readonly IPriceCatalog _catalog;

    public GetPriceCommandHandler(IPriceCatalog catalog) => _catalog = catalog;

    public decimal Handle(GetPriceCommand command) => _catalog.PriceOf(command.Sku);
}
```

## See also

- [Events](events.md)
- [Service Locator](service-locator.md)
- [Logging](logging.md)
- [Diagrams](../diagrams.md)
