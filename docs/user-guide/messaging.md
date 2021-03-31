# Messaging

## Definition

The Crispy Waffle provides a set of helpers to send/receive messaging over RabbitMq broker.

## Examples

Send message to a RabbitMq exchage:

```cs

[ExchangeName("rabbitmq-exchange-name")]
[Serializer(SerializerFormat.JSON)]
public class SampleItemDto
{
    public Guid Id { get; set; }

    public string Text { get; set; }

    public DateTime Date { get; set; }
}

static void Main(string[] args)
{
    //Registering the RabbitMq connector as singleton lifecycle.
    ServiceLocator.Register<RabbitMQConnector>(LifeStyle.SINGLETON);

    var data = new SampleItemDto 
    {
        Id = Guid.NewGuid(),
        Text = "some random text",
        Date = DateTime.Now
    };

    //Gets the wrapper
    var wrapper = ServiceLocator.Resolve<RabbitMQWrapper>();

    //Send to exchange (the exchange name is set via attributes in the SampleItemDto declaration)
    wrapper.SendToExchange(data);

    Console.ReadKey();
}
```

Send message to a RabbitMq queue:

```cs

[QueueName("rabbitmq-queue-name")]
[Serializer(SerializerFormat.JSON)]
public class SampleItemDto
{
    public Guid Id { get; set; }

    public string Text { get; set; }

    public DateTime Date { get; set; }
}

static void Main(string[] args)
{
    //Registering the RabbitMq connector as singleton lifecycle.
    ServiceLocator.Register<RabbitMQConnector>(LifeStyle.SINGLETON);

    var data = new SampleItemDto 
    {
        Id = Guid.NewGuid(),
        Text = "some random text",
        Date = DateTime.Now
    };

    //Gets the wrapper
    var wrapper = ServiceLocator.Resolve<RabbitMQWrapper>();

    //Send to queue (the queue name is set via attributes in the SampleItemDto declaration)
    wrapper.SendToQueue(data);

    Console.ReadKey();
}
```