# Logging

## Definition

The Crispy Waffle provides some log providers/adapters.

Default providers:

- ConsoleLogProvider: Output log to console window.
- EventLogProvider: Output log using `EventLogAdapter`.
- TextFileLogProvider: Output log to a text file in the file system.
- Log4NetLogProvider: Bridge from CrispyWaffle logging to Log4Net logging.
- ...

Default adapters:

- StandardConsoleLogAdapter: Colored console output.
- StandardTextFileLogAdapter: Simple text file output.
- RollingTextFileLogAdapter: Text file output, rolling to another file every time each reaches a defined size.
- ...

## Examples

A simple `console application` with simple (*coloured console output*) logging example:

```cs
static void Main(string[] args)
{
    //Registering the standard console log adapter to be used by the console log provider. 
    ServiceLocator.Register<IConsoleLogAdapter, StandardConsoleLogAdapter>(LifeStyle.SINGLETON);

    //Registering the null exception handler for the method LogConsumer.Handle, this means that no action will be executed for exceptions handled by LogConsumer.
    ServiceLocator.Register<IExceptionHandler, NullExceptionHandler>(LifeStyle.SINGLETON);
    
    //Adding console provider to LogConsumer, the log provider will use the registered IConsoleLogAdapter.
    LogConsumer.AddProvider<ConsoleLogProvider>();
        
    LogConsumer.Info("Hello world Crispy Waffle");
        
    LogConsumer.Debug("Current time: {0:hh:mm:ss}", DateTime.Now);
        
    LogConsumer.Warning("Press any key to close the program!");

    Console.ReadKey();
}
```
