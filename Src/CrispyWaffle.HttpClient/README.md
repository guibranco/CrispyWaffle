# CrispyWaffle.HttpClient

[![NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.HttpClient.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.HttpClient/) [![NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.HttpClient.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.HttpClient/) [![GitHub license](https://img.shields.io/github/license/guibranco/CrispyWaffle)](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE)

Part of [Crispy Waffle](https://github.com/guibranco/CrispyWaffle), a toolkit for .NET projects.

`CrispyWaffle.HttpClient` is a small wrapper around `IHttpClientFactory` that sends JSON requests, retries transient failures with exponential backoff, and returns every result as an `HttpResponse<T>` instead of throwing (unless you ask it to throw). Use it when you want typed JSON calls with retries and a uniform result object, without adding Polly.

The package is standalone: it does not reference the core `CrispyWaffle` package and does not use `ServiceLocator`. It is wired up through `Microsoft.Extensions.DependencyInjection` and consumed by injecting `IHttpClientWrapper`.

| | |
|---|---|
| **Package** | `CrispyWaffle.HttpClient` |
| **Namespace** | `CrispyWaffle.HttpClient`, `CrispyWaffle.HttpClient.Serialization`, `CrispyWaffle.HttpClient.Exceptions` |
| **Install** | Not yet published to NuGet (it is not in the release workflow's package list). Reference the project: `dotnet add reference Src/CrispyWaffle.HttpClient/CrispyWaffle.HttpClient.csproj` |
| **Target framework** | `net10.0` only |
| **Depends on** | No CrispyWaffle packages. `Microsoft.Extensions.Http` (which brings `Microsoft.Extensions.Options`, `Microsoft.Extensions.Logging.Abstractions` and DI abstractions) |

## Supported frameworks

This package targets **.NET 10.0**.

## Getting started

```csharp
using CrispyWaffle.HttpClient;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddLogging();
services.AddCrispyWaffleHttpClient(o =>
{
    o.BaseAddress = "https://api.example.com/";
    o.TimeoutSeconds = 10;
    o.RetryCount = 2;
});

using var provider = services.BuildServiceProvider();
var http = provider.GetRequiredService<IHttpClientWrapper>();

HttpResponse<Todo> response = await http.GetAsync<Todo>("todos/1");
if (response.IsSuccess)
{
    Console.WriteLine($"{response.Data!.Title} ({response.Duration.TotalMilliseconds} ms)");
}
else
{
    Console.WriteLine($"Failed: {response.StatusCode} {string.Join("; ", response.Errors ?? [])}");
}

public sealed record Todo(int Id, string Title, bool Completed);
```

## Documentation

The full reference and more examples for this package are on the [CrispyWaffle.HttpClient documentation page](https://guibranco.github.io/CrispyWaffle/packages/http-client/).

- [Crispy Waffle documentation](https://guibranco.github.io/CrispyWaffle/)
- [Changelog](https://guibranco.github.io/CrispyWaffle/changelog/)
- [Source code](https://github.com/guibranco/CrispyWaffle)
- [Report an issue](https://github.com/guibranco/CrispyWaffle/issues/new)

## License

Released under the [MIT License](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE).
