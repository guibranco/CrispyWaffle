# CrispyWaffle.CouchDB

[![NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.CouchDB.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.CouchDB/) [![NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.CouchDB.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.CouchDB/) [![GitHub license](https://img.shields.io/github/license/guibranco/CrispyWaffle)](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE)

Part of [Crispy Waffle](https://github.com/guibranco/CrispyWaffle), a toolkit for .NET projects.

CrispyWaffle.CouchDB provides a cache repository that stores documents in [Apache CouchDB](https://couchdb.apache.org/), using the CouchDB.NET driver. Use it when cached data should survive restarts and be readable from any process that can reach the CouchDB server, and when the cached values are documents rather than plain strings or numbers.

| | |
|---|---|
| **Package** | `CrispyWaffle.CouchDB` |
| **Namespace** | `CrispyWaffle.CouchDB.Cache`, `CrispyWaffle.CouchDB.Utils.Communications` |
| **Install** | Not yet published to NuGet (it is not in the release workflow's package list). Reference the project: `dotnet add reference Src/CrispyWaffle.CouchDB/CrispyWaffle.CouchDB.csproj` |
| **Depends on** | `CrispyWaffle`, `CrispyWaffle.Configuration`, `CouchDB.NET` |

## Supported frameworks

This package targets **.NET Standard 2.0**, **.NET Standard 2.1**, **.NET 8.0**, **.NET 9.0** and **.NET 10.0**.

## Getting started

Start CouchDB locally. The repository's `docker-compose.yml` runs `couchdb:latest` on port 5984 with the user `Admin` and the password `myP@ssw0rd`.

```csharp
using CrispyWaffle.Cache;
using CrispyWaffle.Composition;
using CrispyWaffle.Configuration;
using CrispyWaffle.CouchDB.Cache;
using CrispyWaffle.CouchDB.Utils.Communications;

var connection = new Connection
{
    Host = "http://localhost", // include the scheme
    Port = 5984,
    Credentials = { Username = "Admin", Password = "myP@ssw0rd" },
};

ServiceLocator.Register(() => new CouchDBConnector(connection), Lifetime.Singleton);
ServiceLocator.Register(
    () => new CouchDBCacheRepository(ServiceLocator.Resolve<CouchDBConnector>()),
    Lifetime.Singleton
);

CacheManager.AddRepository<CouchDBCacheRepository>();

// Use the repository directly for typed documents.
var repository = ServiceLocator.Resolve<CouchDBCacheRepository>();

var doc = new CouchDBCacheDocument();
await repository.SetAsync(doc, Guid.NewGuid().ToString());

var stored = await repository.GetAsync<CouchDBCacheDocument>(doc.Key);
```

## Documentation

The full reference and more examples for this package are on the [CrispyWaffle.CouchDB documentation page](https://guibranco.github.io/CrispyWaffle/packages/couchdb/).

- [Crispy Waffle documentation](https://guibranco.github.io/CrispyWaffle/)
- [Changelog](https://guibranco.github.io/CrispyWaffle/changelog/)
- [Source code](https://github.com/guibranco/CrispyWaffle)
- [Report an issue](https://github.com/guibranco/CrispyWaffle/issues/new)

## License

Released under the [MIT License](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE).
