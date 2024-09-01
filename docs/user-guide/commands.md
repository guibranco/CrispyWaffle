# Commands

## Definition

Commands are a way to execute one or more actions based on a class (the command itself).
Commands are defined as a simple class inherited from the `ICommand` interface.

Each command can be handled by one and only one of the command handlers.

The command handler class inherits from the `ICommandHandler<TCommand, TResult>` interface and must implement the `TResult Handle(TCommand command)`.

For example, the `HelloWorldCommand` class, defined below, is a simple class without methods, properties, or fields and is used to trigger `HelloWorldCommandHandler`:

Command class:

```cs
public class HelloWorldCommand : ICommand {}
```

Command handler class:

```cs
public class HelloWorldCommandHandler : ICommandHandler<HelloWorldCommand, string>
{
    public string Handle(HelloWorldCommand command) =>
        "Hello World, this is the result of the triggered command!";
}
```

To trigger the command handler, just call the `CommandsConsumer.Raise` method from any part of your code:

```cs
CommandsConsumer.Raise<HelloWorldCommand, string>(new HelloWorldCommand());
//do other stuff...
```

## Examples

### Returning a class as a result

In this example, the command has some properties, and the handler will give the result:

```cs
// The command class.
public class SomeCommand(int quantity) : ICommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime Data { get; } = DateTime.Now;
    public int Quantity { get; } = quantity;
}

// The result of the command.
public class ResultSomeCommand(int newQuantity)
{
    public int Quantity { get; } = newQuantity;
}

// The command handler class. Each event can be handled by one and only one handler.
public class SomeCommandHandler : ICommandHandler<SomeCommand, ResultSomeCommand>
{
    //constructor of the class, with dependencies...
    //dependencies are resolved by ServiceLocator.

    public ResultSomeCommand Handle(SomeCommand command)
    {
        LogConsumer.Warning("Command 'SomeCommand' handled by 'SomeCommandHandler'. Command Id: {0}", args.Id);
        //do any other processing stuff...
        int newQuantity = RecoveryQuantity();
        return new(newQuantity);
    }
}

// Program entry point
public static class Program
{
    public static void Main(string[] args)
    {
        //Initialize the dependencies with ServiceLocator.
        //Initialize log providers/adapters
        //...

        var command = new SomeCommand(10);
        var result = CommandsConsumer.Raise<SomeCommand, ResultSomeCommand>(command);
        Console.WriteLine($"New quantity: {quantity.Quantity}");
    }
}

```
