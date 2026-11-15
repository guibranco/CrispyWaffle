# CrispyWaffle.BackgroundJobs

[![NuGet Version](https://img.shields.io/nuget/v/CrispyWaffle.BackgroundJobs.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.BackgroundJobs/) [![NuGet Downloads](https://img.shields.io/nuget/dt/CrispyWaffle.BackgroundJobs.svg?style=flat)](https://www.nuget.org/packages/CrispyWaffle.BackgroundJobs/) [![GitHub license](https://img.shields.io/github/license/guibranco/CrispyWaffle)](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE)

Part of [Crispy Waffle](https://github.com/guibranco/CrispyWaffle), a toolkit for .NET projects.

`CrispyWaffle.BackgroundJobs` runs fire-and-forget and delayed jobs inside an ASP.NET Core application. You register typed handlers under a name, enqueue a job with that name and a payload, and a hosted worker picks it up, runs the handler in its own DI scope, and retries it with exponential backoff when it fails. Jobs are kept in an in-memory store by default, or in a database through Entity Framework Core.

The package is standalone: it does not reference the core `CrispyWaffle` package. It is built on `Microsoft.Extensions.Hosting` (`BackgroundService`) and `Microsoft.Extensions.DependencyInjection`. It is a different thing from the core package's [cron scheduler](https://guibranco.github.io/CrispyWaffle/core/scheduler/): that one runs in-process actions on cron expressions through `JobManager`, while this package queues persisted, retryable jobs that carry a payload. The `CronJobService` base class in this package does not parse cron expressions (see [Recurring work](https://guibranco.github.io/CrispyWaffle/packages/background-jobs/#recurring-work)).

| | |
|---|---|
| **Package** | `CrispyWaffle.BackgroundJobs` |
| **Namespace** | `CrispyWaffle.BackgroundJobs.Abstractions`, `CrispyWaffle.BackgroundJobs.Core`, `CrispyWaffle.BackgroundJobs.Extensions`, `CrispyWaffle.BackgroundJobs.Persistence`, `CrispyWaffle.BackgroundJobs.Scheduling`, `CrispyWaffle.BackgroundJobs.Monitoring`, `CrispyWaffle.BackgroundJobs.Exceptions` |
| **Install** | Not yet published to NuGet (it is not in the release workflow's package list). Reference the project: `dotnet add reference Src/CrispyWaffle.BackgroundJobs/CrispyWaffle.BackgroundJobs.csproj` |
| **Target framework** | `net10.0` only |
| **Depends on** | No CrispyWaffle packages. `Microsoft.Extensions.Hosting.Abstractions`, `Microsoft.AspNetCore.Hosting.Abstractions` (for `IStartupFilter` and the middleware), `Microsoft.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore.Relational` |

## Supported frameworks

This package targets **.NET 10.0**.

## Getting started

```csharp
using CrispyWaffle.BackgroundJobs.Abstractions;
using CrispyWaffle.BackgroundJobs.Core;
using CrispyWaffle.BackgroundJobs.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCrispyBackgroundJobs(o => o.WorkerCount = 2);
builder.Services.AddJobHandler<SendEmailHandler, SendEmailData>("send-email");

var app = builder.Build();

app.MapPost(
    "/signup",
    async (string email, IJobScheduler jobs) =>
    {
        await jobs.EnqueueAsync("send-email", new SendEmailData(email, "Welcome!"));
        return Results.Accepted();
    }
);

app.Run();

public sealed record SendEmailData(string To, string Subject);

public sealed class SendEmailHandler(ILogger<SendEmailHandler> logger)
    : IBackgroundJobHandler<SendEmailData>
{
    public async Task<JobResult> HandleAsync(SendEmailData data, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Sending '{Subject}' to {To}", data.Subject, data.To);
            await Task.Delay(100, cancellationToken); // send the email here
            return JobResult.Ok();
        }
        catch (HttpRequestException ex)
        {
            return JobResult.Fail(ex.Message, retry: true);
        }
    }
}
```

## Documentation

The full reference and more examples for this package are on the [CrispyWaffle.BackgroundJobs documentation page](https://guibranco.github.io/CrispyWaffle/packages/background-jobs/).

- [Crispy Waffle documentation](https://guibranco.github.io/CrispyWaffle/)
- [Changelog](https://guibranco.github.io/CrispyWaffle/changelog/)
- [Source code](https://github.com/guibranco/CrispyWaffle)
- [Report an issue](https://github.com/guibranco/CrispyWaffle/issues/new)

## License

Released under the [MIT License](https://github.com/guibranco/CrispyWaffle/blob/main/LICENSE).
