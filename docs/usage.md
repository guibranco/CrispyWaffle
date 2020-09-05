# Usage

## Logging

A simple `console application` with simple (colored console output) logging example:

```cs
static void Main(string[] args)
{
    ServiceLocator.Register<IConsoleLogAdapter, StandardConsoleLogAdapter>(LifeStyle.SINGLETON);
    ServiceLocator.Register<IExceptionHandler, NullExceptionHandler>(LifeStyle.SINGLETON);
        
    LogConsumer.AddProvider<ConsoleLogProvider>();
        
    LogConsumer.Info("Hello world Crispy Waffle");
        
    LogConsumer.Debug("Current time: {0:hh:mm:ss}", DateTime.Now);
        
    LogConsumer.Warning("Press any key to close the program!");
    Console.ReadKey();
}
```

## Events

An example using `Events` & `EventsHandlers` 

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

    public void Handle(SomeEvent args)
    {
        LogConsumer.Warning("Event 'SomeEvent' handled by 'SomeEventHandler'. Event Id: {0}", args.Id);
        //do any other processing stuff...
    }
}

public class AnotherSomeEventHandler : IEventHandler<SomeEvent>
{
    //constructor of the class, with dependencies...

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

        var evt = new SomeEvent ("README.md test data");
        EventsConsumer.Raise(evt);
    }
}
```

## Scheduled jobs (using CRON expressions)

Using [`cron` expression](https://en.wikipedia.org/wiki/Cron) to schedule tasks/jobs inside a program.

```cs
public static class Program
{
    public static void Main(string[] args)
    {
        var exampleObj = new SomeClass();
        exampleObj.Counter = 10;

        var jobManager = new JobManager();
        jobManager.AddJob(new JobRunner("* * * * *", () => { exampleObj.Counter++; }));
        jobManager.Start();

        Thread.Sleep(120 * 1000); //waits 2 minutes

        jobManager.Stop(); //stops the manager, so no more execution runs.

        if(exampleObj.Counter == 12)
        {
            LogConsumer.Warning("Example job runned for 2 times!");
        }

    }

    internal class SomeClass 
    {
        public int Counter { get; set; } 
    }

}

```

## Other methods

Other methods usage can be infered from usage from [tests project](https://github.com/guibranco/CrispyWaffle).