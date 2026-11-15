# CrispyWaffle.ElasticSearch

[![NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.ElasticSearch.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.ElasticSearch/) [![NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.ElasticSearch.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.ElasticSearch/) [![GitHub license](https://img.shields.io/github/license/guibranco/CrispyWaffle)](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE)

Part of [Crispy Waffle](https://github.com/guibranco/CrispyWaffle), a toolkit for .NET projects.

CrispyWaffle.ElasticSearch connects CrispyWaffle to [Elasticsearch](https://www.elastic.co/elasticsearch) through the official `Elastic.Clients.Elasticsearch` client. It provides:

- A log provider that indexes every log message as a document and deletes old log documents once, shortly after startup.
- A small wrapper for reading and writing documents.
- Index helpers that derive index names from your types.

Use it when you want CrispyWaffle logs searchable in Elasticsearch or Kibana, or when you need simple document storage without writing client code yourself.

| | |
|---|---|
| **Package** | `CrispyWaffle.ElasticSearch` |
| **Namespace** | `CrispyWaffle.ElasticSearch.Log`, `CrispyWaffle.ElasticSearch.Helpers`, `CrispyWaffle.ElasticSearch.Utils.Communications` |
| **Install** | `dotnet add package CrispyWaffle.ElasticSearch` |
| **Depends on** | `CrispyWaffle`, `CrispyWaffle.Configuration`, `Elastic.Clients.Elasticsearch` |

## Supported frameworks

This package targets **.NET Standard 2.0**, **.NET Standard 2.1**, **.NET 8.0**, **.NET 9.0** and **.NET 10.0**.

## Getting started

Start Elasticsearch locally. The repository's `docker-compose.yml` runs `elasticsearch:8.8.0` as a single node on port 9200 with security disabled.

```csharp
using CrispyWaffle.Composition;
using CrispyWaffle.Configuration;
using CrispyWaffle.ElasticSearch.Log;
using CrispyWaffle.ElasticSearch.Utils.Communications;
using CrispyWaffle.Log;

var connection = new Connection { Host = "localhost", Port = 9200 };

var connector = new ElasticConnector(connection, "logs-my-app");
ServiceLocator.Register(connector);

// Index log messages and delete log documents older than 30 days.
LogConsumer
    .AddProvider(new ElasticSearchLogProvider(connector, 30))
    .SetLevel(LogLevel.Production);

LogConsumer.Info("Application started");
```

## Documentation

The full reference and more examples for this package are on the [CrispyWaffle.ElasticSearch documentation page](https://guibranco.github.io/CrispyWaffle/packages/elasticsearch/).

- [Crispy Waffle documentation](https://guibranco.github.io/CrispyWaffle/)
- [Changelog](https://guibranco.github.io/CrispyWaffle/changelog/)
- [Source code](https://github.com/guibranco/CrispyWaffle)
- [Report an issue](https://github.com/guibranco/CrispyWaffle/issues/new)

## License

Released under the [MIT License](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE).
