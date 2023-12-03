//Registering the standard console log adapter to be used by the console log provider. 
using CrispyWaffle.Composition;
using CrispyWaffle.Events;
using CrispyWaffle.Log;
using CrispyWaffle.Log.Adapters;
using CrispyWaffle.Log.Handlers;
using CrispyWaffle.Log.Providers;
using SampleApp;

//Adding console provider to LogConsumer, the log provider will use the registered IConsoleLogAdapter.
LogConsumer.AddProvider<ConsoleLogProvider>();
var @event = new TestObjects.TestDoneEvent(Guid.NewGuid(), @"Sample test");
//EventsConsumer.Raise(@event);
Task t = EventsConsumer.RaiseAsync(@event);

LogConsumer.Info("Hello world Crispy Waffle");

LogConsumer.Debug("Current time: {0:hh:mm:ss}", DateTime.Now);

LogConsumer.Warning("Press any key to close the program!");

Console.ReadKey();
t.Wait();
