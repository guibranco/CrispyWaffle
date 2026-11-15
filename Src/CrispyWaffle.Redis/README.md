# CrispyWaffle.Redis

[![NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Redis.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Redis/) [![NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Redis.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Redis/) [![GitHub license](https://img.shields.io/github/license/guibranco/CrispyWaffle)](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE)

Part of [Crispy Waffle](https://github.com/guibranco/CrispyWaffle), a toolkit for .NET projects.

CrispyWaffle.Redis connects CrispyWaffle to [Redis](https://redis.io). It has four parts:

- A cache repository for `CacheManager`.
- A log provider that publishes log messages to Redis Pub/Sub channels.
- A telemetry client that stores hits, events and metrics in Redis.
- Helpers for master/replica setups and quick counter operations.

Use it when several processes or machines need a shared cache, when you want log messages streamed to listeners in real time, or when you want lightweight counters stored in Redis.

| | |
|---|---|
| **Package** | `CrispyWaffle.Redis` |
| **Namespace** | `CrispyWaffle.Redis.Cache`, `CrispyWaffle.Redis.Log`, `CrispyWaffle.Redis.Log.PropagationStrategy`, `CrispyWaffle.Redis.Telemetry`, `CrispyWaffle.Redis.Utils`, `CrispyWaffle.Redis.Utils.Communications` |
| **Install** | `dotnet add package CrispyWaffle.Redis` |
| **Depends on** | `CrispyWaffle`, `CrispyWaffle.Configuration`, `StackExchange.Redis`, `StackExchange.Redis.Extensions.Core` |

## Supported frameworks

This package targets **.NET Standard 2.0**, **.NET Standard 2.1**, **.NET 8.0**, **.NET 9.0** and **.NET 10.0**.

## Getting started

Start Redis locally. The repository's `docker-compose.yml` runs `redis:6.2-alpine` on port 6379 with the password `myP@ssw0rd`.

Then create a serializer, a connection and a connector, and register them:

```csharp
using System.Text.Json;
using System.Threading;
using CrispyWaffle.Cache;
using CrispyWaffle.Composition;
using CrispyWaffle.Configuration;
using CrispyWaffle.Log;
using CrispyWaffle.Redis.Cache;
using CrispyWaffle.Redis.Log;
using CrispyWaffle.Redis.Log.PropagationStrategy;
using CrispyWaffle.Redis.Telemetry;
using CrispyWaffle.Redis.Utils.Communications;
using CrispyWaffle.Telemetry;
using StackExchange.Redis.Extensions.Core;

public sealed class SystemTextJsonRedisSerializer : ISerializer
{
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    public byte[] Serialize<T>(T item) => JsonSerializer.SerializeToUtf8Bytes(item, _options);

    public T Deserialize<T>(byte[] serializedObject) =>
        JsonSerializer.Deserialize<T>(serializedObject, _options);
}

public static class RedisBootstrap
{
    public static void Configure(CancellationToken shutdownToken)
    {
        var connection = new Connection
        {
            Host = "localhost",
            Port = 6379,
            Credentials = { Password = "myP@ssw0rd" },
        };

        var connector = new RedisConnector(
            connection,
            new SystemTextJsonRedisSerializer(),
            "my-app"
        );

        // One connector per process: it owns the connection pool.
        ServiceLocator.Register(connector);

        // Cache
        CacheManager.AddRepository(new RedisCacheRepository(connector));

        // Logging
        LogConsumer
            .AddProvider(
                new PubSubRedisLogProvider(connector, new GeneralPropagation(), shutdownToken)
            )
            .SetLevel(LogLevel.Production);

        // Telemetry
        TelemetryAnalytics.AddClient(new RedisTelemetryClient(connector, "my-app"));
    }
}
```

After that, `await CacheManager.SetAsync("value", "my-key")`, `LogConsumer.Info("...")` and `TelemetryAnalytics.TrackHit("home")` all reach Redis.

## Documentation

The full reference and more examples for this package are on the [CrispyWaffle.Redis documentation page](https://guibranco.github.io/CrispyWaffle/packages/redis/).

- [Crispy Waffle documentation](https://guibranco.github.io/CrispyWaffle/)
- [Changelog](https://guibranco.github.io/CrispyWaffle/changelog/)
- [Source code](https://github.com/guibranco/CrispyWaffle)
- [Report an issue](https://github.com/guibranco/CrispyWaffle/issues/new)

## License

Released under the [MIT License](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE).
