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

Below are some examples of logging in a simple `console application` with different adapters.

- `StandardConsoleLogAdapter` usage example:
```cs
static void Main(string[] args)
{
    // Registering the standard console log adapter to be used by the console log provider. 
    ServiceLocator.Register<IConsoleLogAdapter, StandardConsoleLogAdapter>(Lifetime.Singleton);

    // Registering the null exception handler for the method LogConsumer.Handle, this means that no action will be executed for exceptions handled by LogConsumer.
    ServiceLocator.Register<IExceptionHandler, NullExceptionHandler>(Lifetime.Singleton);
    
    // Adding console provider to LogConsumer, the log provider will use the registered IConsoleLogAdapter.
    LogConsumer.AddProvider<ConsoleLogProvider>();
        
    LogConsumer.Info("Hello world Crispy Waffle");
        
    LogConsumer.Debug("Current time: {0:hh:mm:ss}", DateTime.Now);
        
    LogConsumer.Warning("Press any key to close the program!");

    Console.ReadKey();
}
```


- `RollingTextFileLogAdapter` usage example:
```cs
static void Main(string[] args)
{
    // Registering the rolling text file log adapter to be used by the text file log provider. 
    // It is recommended to keep the fileNameSeed unique for every object in a multi-threaded environment, otherwise the behaviour might be unexpected.
    ServiceLocator.Register<ITextFileLogAdapter>(
        () => new RollingTextFileLogAdapter(
            // Log directory path.
            AppDomain.CurrentDomain.BaseDirectory,
            // Logs file name seed or unique identifier
            "textFileLogs",
            // Max number of messages allowed per file
            100,
            // Max file size along with unit
            (Unit.KByte, 10),
            // The type of log file to create
            LogFileType.Text
        ),
        Lifetime.Transient
    );

    // Registering the null exception handler for the method LogConsumer.Handle, this means that no action will be executed for exceptions handled by LogConsumer.
    ServiceLocator.Register<IExceptionHandler, NullExceptionHandler>(Lifetime.Singleton);
    
    // Adding rolling text file log provider to LogConsumer, the log provider will use the registered ITextFileLogAdapter.
    LogConsumer.AddProvider<TextFileLogProvider>();
    
    LogConsumer.Info("Hello world Crispy Waffle");
    
    LogConsumer.Debug("Current time: {0:hh:mm:ss}", DateTime.Now);
    
    LogConsumer.Warning("Warning message!");
    
    Console.ReadKey();
}
```