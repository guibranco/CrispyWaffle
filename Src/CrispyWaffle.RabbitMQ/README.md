# CrispyWaffle.RabbitMQ

[![NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.RabbitMQ.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.RabbitMQ/) [![NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.RabbitMQ.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.RabbitMQ/) [![GitHub license](https://img.shields.io/github/license/guibranco/CrispyWaffle)](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE)

Part of [Crispy Waffle](https://github.com/guibranco/CrispyWaffle), a toolkit for .NET projects.

CrispyWaffle.RabbitMQ sends and receives messages through a RabbitMQ broker and provides a log provider that publishes CrispyWaffle log messages to a RabbitMQ exchange.

Use it when you want to publish typed messages to a queue or an exchange with a few lines of code, consume them through an event, or ship your application logs to RabbitMQ.

| | |
|---|---|
| **Package** | `CrispyWaffle.RabbitMQ` |
| **Namespace** | `CrispyWaffle.RabbitMQ.Utils.Communications`, `CrispyWaffle.RabbitMQ.Helpers`, `CrispyWaffle.RabbitMQ.Log` |
| **Install** | `dotnet add package CrispyWaffle.RabbitMQ` |
| **Depends on** | `CrispyWaffle`, `CrispyWaffle.Configuration`, `RabbitMQ.Client` |

## Supported frameworks

This package targets **.NET Standard 2.0**, **.NET Standard 2.1**, **.NET 8.0**, **.NET 9.0** and **.NET 10.0**.

## Getting started

```csharp
using System;
using System.Threading.Tasks;
using CrispyWaffle.Composition;
using CrispyWaffle.Configuration;
using CrispyWaffle.RabbitMQ.Helpers;
using CrispyWaffle.RabbitMQ.Utils.Communications;
using CrispyWaffle.Serialization;

[QueueName("orders")]
[Serializer(SerializerFormat.Json)]
public class OrderCreated : IQueuing
{
    public Guid Id { get; set; }

    public string Customer { get; set; }
}

public static class Program
{
    public static async Task Main()
    {
        var connection = new Connection
        {
            Host = "localhost",
            Port = 5672,
            Credentials = { Username = "guest", Password = "guest" },
        };

        ServiceLocator.Register(new RabbitMQConnector(connection));

        var wrapper = ServiceLocator.Resolve<RabbitMQWrapper>();

        await wrapper.SendToQueueAsync(new OrderCreated { Id = Guid.NewGuid(), Customer = "ACME" });
    }
}
```

## Documentation

The full reference and more examples for this package are on the [CrispyWaffle.RabbitMQ documentation page](https://guibranco.github.io/CrispyWaffle/packages/rabbitmq/).

- [Crispy Waffle documentation](https://guibranco.github.io/CrispyWaffle/)
- [Changelog](https://guibranco.github.io/CrispyWaffle/changelog/)
- [Source code](https://github.com/guibranco/CrispyWaffle)
- [Report an issue](https://github.com/guibranco/CrispyWaffle/issues/new)

## License

Released under the [MIT License](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE).
