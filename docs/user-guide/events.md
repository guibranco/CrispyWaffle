# Events

## Definition

Events are a way to execute one or more actions based on a class (the event itself).
Events are defined as a simple class, inherited from `IEvent` interface.

Each event can be handled by one, two or infinite number of event handlers.

Event handler class inherit from `IEventHandler<TEvent>` interface and must implement the `Handle(TEvent evt)` method.
The `TEvent` generic type is the type of `IEvent` that will be handled.

For example, the `HellWorldEvent` class, defined below is a simple class without methods, properties or fields and is used to trigger `HelloWorldEventHandler`: 

Event class:

```cs
public class HelloWorldEvent : IEvent {}
```

Event handler class:

```cs
public class HelloWorldEventHandler : IEventHandler<HelloWorldEvent> 
{
    public Handle(HelloWorldEvent evt)
    {
        Console.WriteLine("Hello World triggered!");
    }
}
```

To trigger the event handler just call the `EventsConsumer.Raise` method from any part of your code:

```cs
EventsConsumer.Raise(new HelloWorldEvent());
//do other stuff...
```

Each event handler can handle many kinds of events, just need to implement each interface and each method `Handle`.

Event handling is currently done synchronously. There are plans to do async, issue [#XX](https://github.com/guibranco/CrispyWaffle/issues/XX).

---

## Examples

### Multiple event handlers for the same event

A more complex example using `Events` & `EventsHandlers`. 
In this example, there are two event handlers and the event class has some properties:

```cs
//The event class.
public class SomeEvent : IEvent 
{
    public SomeEvent(string data)
    {
        Id = Guid.NewGuid();
        Date = DateTime.Now;
        Data = data;
    }

    public Guid Id { get; }

    public string Data { get; }

    public DateTime Date { get; }
}

//The event handler class. Each event can be handled by N event handlers.
public class SomeEventHandler : IEventHandler<SomeEvent>
{
    //constructor of the class, with dependencies...
    //dependencies are resolved by ServiceLocator.

    public void Handle(SomeEvent args)
    {
        LogConsumer.Warning("Event 'SomeEvent' handled by 'SomeEventHandler'. Event Id: {0}", args.Id);
        //do any other processing stuff...
    }
}

public class AnotherSomeEventHandler : IEventHandler<SomeEvent>
{
    //constructor of the class, with dependencies...
    //dependencies are resolved by ServiceLocator.

    public void Handle(SomeEvent args)
    {
        LogConsumer.Warning("Event 'SomeEvent' handled by 'AnotherSomeEventHandler'. Event Id: {0}", args.Id);
            
        //someOtherDependency.DoSomeStuffWithEvent(args.Id, args.Data);
        //do any other processing stuff...
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

        var evt = new SomeEvent ("Some text passed to all event handlers");
        EventsConsumer.Raise(evt);
    }
}
```
