# Service Locator

## Definition 

The service locator class is a helper class that acts like a IoC container.
You can register instances as singleton/transient scope. Register dependencies and request a instance of a interface/class.

## Examples

The following example register a singleton class and then request it multiple times.
In this example, the class is instantiante in the first `Resolve` call

```cs
public class SingletonTest 
{
    public DateTime Date { get; set; }

    //when creating a instance, set the Date property to DateTime.Now value.
    public SingletonTest() => Date = DateTime.Now;
}

static void Main(string[] args)
{
    //Registering
    ServiceLocator.Register<SingletonTest>(LifeStyle.SINGLETON);

    var instanceA = ServiceLocator.Resolve<SingletonTest>();

    Thread.Sleep(1000);
    
    var instanceB = ServiceLocator.Resolve<SingletonTest>();

    Thread.Sleep(1000);
    
    var instanceC = ServiceLocator.Resolve<SingletonTest>();

    //check that all 3 instances has the same Date.
}
``` 

The following create a new instance for every `Resolve` call

```cs

public class TransientTest 
{
    public DateTime Date { get; set; }

    //when creating a instance, set the Date property to DateTime.Now value.
    public TransientTest() => Date = DateTime.Now;
}

static void Main(string[] args)
{
    //Registering
    ServiceLocator.Register<TransientTest>();

    var instanceA = ServiceLocator.Resolve<TransientTest>();

    Thread.Sleep(1000);
    
    var instanceB = ServiceLocator.Resolve<TransientTest>();

    Thread.Sleep(1000);
    
    var instanceC = ServiceLocator.Resolve<TransientTest>();

    //check that all 3 instances has different Date properties value.
}
``` 
