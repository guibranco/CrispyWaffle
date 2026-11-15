# CrispyWaffle.Configuration

[![NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Configuration.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Configuration/) [![NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Configuration.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Configuration/) [![GitHub license](https://img.shields.io/github/license/guibranco/CrispyWaffle)](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE)

Part of [Crispy Waffle](https://github.com/guibranco/CrispyWaffle), a toolkit for .NET projects.

CrispyWaffle.Configuration holds the connection settings that the connector packages (Redis, CouchDB, Elasticsearch, RabbitMQ, SMTP) take. A `Connection` stores a host, a port and a set of credentials. When it is serialized, the password is encrypted, so you can keep connections in configuration files without writing passwords in plain text.

Use this package whenever you create a connector from one of the other CrispyWaffle packages. You can also use it for your own connection settings.

| | |
|---|---|
| **Package** | `CrispyWaffle.Configuration` |
| **Namespace** | `CrispyWaffle.Configuration` |
| **Install** | `dotnet add package CrispyWaffle.Configuration` |
| **Depends on** | `CrispyWaffle` |

## Supported frameworks

This package targets **.NET Standard 2.0**, **.NET Standard 2.1**, **.NET 8.0**, **.NET 9.0** and **.NET 10.0**.

## Getting started

```csharp
using CrispyWaffle.Composition;
using CrispyWaffle.Configuration;

// Register the keys used to encrypt and decrypt serialized passwords.
ServiceLocator.Register<ISecureCredentialProvider>(
    () =>
        new SecureCredentialProvider
        {
            PasswordHash = "Cr1$PTVV@FE13",
            SaltKey = "y48H85nH21",
            IVKey = "HZEM7|Ne2YGS/F41",
        },
    Lifetime.Singleton
);

// Describe a connection. Credentials is created by the constructor.
var connection = new Connection
{
    Name = "Redis",
    Host = "localhost",
    Port = 6379,
    Credentials = { Username = "default", Password = "myP@ssw0rd" },
};
```

Pass the connection to a connector constructor, for example `new RedisConnector(connection, serializer)`. See [CrispyWaffle.Redis](https://guibranco.github.io/CrispyWaffle/packages/redis/), [CrispyWaffle.CouchDB](https://guibranco.github.io/CrispyWaffle/packages/couchdb/) and [CrispyWaffle.ElasticSearch](https://guibranco.github.io/CrispyWaffle/packages/elasticsearch/).

## Documentation

The full reference and more examples for this package are on the [CrispyWaffle.Configuration documentation page](https://guibranco.github.io/CrispyWaffle/packages/configuration/).

- [Crispy Waffle documentation](https://guibranco.github.io/CrispyWaffle/)
- [Changelog](https://guibranco.github.io/CrispyWaffle/changelog/)
- [Source code](https://github.com/guibranco/CrispyWaffle)
- [Report an issue](https://github.com/guibranco/CrispyWaffle/issues/new)

## License

Released under the [MIT License](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE).
