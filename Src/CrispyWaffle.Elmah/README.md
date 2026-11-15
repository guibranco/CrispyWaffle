# CrispyWaffle.Elmah

[![NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.Elmah.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Elmah/) [![NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.Elmah.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.Elmah/) [![GitHub license](https://img.shields.io/github/license/guibranco/CrispyWaffle)](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE)

Part of [Crispy Waffle](https://github.com/guibranco/CrispyWaffle), a toolkit for .NET projects.

CrispyWaffle.Elmah provides an exception handler that sends exceptions handled by CrispyWaffle to ELMAH, through the ElmahCore library for ASP.NET Core. Use it when your web application already uses ElmahCore and you want exceptions passed to `LogConsumer.Handle` to show up in the ELMAH error log.

| | |
|---|---|
| **Package** | `CrispyWaffle.Elmah` |
| **Namespace** | `CrispyWaffle.Elmah` |
| **Install** | `dotnet add package CrispyWaffle.Elmah` |
| **Depends on** | `CrispyWaffle`, `ElmahCore` |

## Supported frameworks

This package targets **.NET Standard 2.0**, **.NET Standard 2.1**, **.NET 8.0**, **.NET 9.0** and **.NET 10.0**.

## Getting started

```csharp
using CrispyWaffle.Elmah;
using CrispyWaffle.Log;
using ElmahCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddElmah();

var app = builder.Build();
app.UseElmah();

LogConsumer.SetHandler(new ElmahExceptionHandler());

app.MapGet("/fail", () =>
{
    try
    {
        throw new InvalidOperationException("Something failed");
    }
    catch (Exception e)
    {
        LogConsumer.Handle(e);
        return Results.Problem();
    }
});

app.Run();
```

## Documentation

The full reference and more examples for this package are on the [CrispyWaffle.Elmah documentation page](https://guibranco.github.io/CrispyWaffle/packages/elmah/).

- [Crispy Waffle documentation](https://guibranco.github.io/CrispyWaffle/)
- [Changelog](https://guibranco.github.io/CrispyWaffle/changelog/)
- [Source code](https://github.com/guibranco/CrispyWaffle)
- [Report an issue](https://github.com/guibranco/CrispyWaffle/issues/new)

## License

Released under the [MIT License](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE).
