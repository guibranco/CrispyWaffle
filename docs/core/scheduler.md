# Scheduler (cron jobs)

The scheduler module runs in-process actions on a recurring schedule written as a CRON-like expression. You register one or more jobs with a `JobManager`, start it, and each job's delegate runs on its own thread whenever the current minute matches its expression. Use it for lightweight periodic work inside a long-running process, such as cleanup, polling or cache refresh.

This module is a minimal minute-resolution timer that lives in the core package. For queued, retried or persisted background work, use the separate [BackgroundJobs package](../packages/background-jobs.md) instead.

| | |
|---|---|
| **Package** | `CrispyWaffle` |
| **Namespace** | `CrispyWaffle.Scheduler` |

## How it works

**Timer.** `JobManager` owns a `System.Timers.Timer` with a fixed 30-second interval. On each tick it compares the current minute (`DateTime.Now.Minute`) with the minute of the last run. If the minute has changed, it records the new run time and calls `Execute(DateTime.Now)` on every registered `JobRunner`, in the order they were added. The last-run time is initialized when the `JobManager` is constructed, so the minute in which you create the manager never triggers jobs. All times are local (`DateTime.Now`).

**Matching.** Each `JobRunner` holds a `CronScheduler` built from its expression. `Execute` checks `CronScheduler.IsTime(dateTime)`; if the time does not match, nothing happens.

**Threading.** When the time matches, the runner starts a new `Thread` for the delegate, unless the previous run of the same job is still in the `Running` or `WaitSleepJoin` state, in which case the run is skipped. Runs of the same job never overlap; different jobs run concurrently. Calls to `Execute` are serialized with a lock per runner. The job threads are ordinary foreground threads and nothing catches exceptions thrown by the delegate: an unhandled exception in a job terminates the process. Wrap the body of each job in `try`/`catch`.

**Stopping.** `Stop()` stops the timer, so no new runs start. Jobs already running are not interrupted. `JobManager` does not implement `IDisposable`.

## CRON syntax

`CronScheduler` supports five fields, in this order. There is no seconds field and no year field.

| Position | Field | Values | `*` expands to |
|---|---|---|---|
| 1 | Minute | 0-59 | 0-59 |
| 2 | Hour | 0-23 | 0-23 |
| 3 | Day of month | 1-31 | 1-31 |
| 4 | Month | 1-12 | 1-12 |
| 5 | Day of week | 0-6, 0 = Sunday | 0-6 |

**Fields are found by pattern matching, not by splitting on spaces.** The expression is scanned with `CronSchedulerValidations.ValidationRegex`, and each successive match becomes the next field. Any character that is not part of a match acts as a separator. Fields that are missing at the end default to `*`, so `10` is the same as `10 * * * *` and `* */3` is the same as `* */3 * * *`. Matches after the fifth are ignored.

Each field can use exactly one of these forms:

| Form | Example | Meaning |
|---|---|---|
| `*` | `*` | Every value in the field's range. |
| `*/n` | `*/15` | Every value in the field's range that is divisible by `n` (`value % n == 0`). |
| `a-b` | `10-15` | Every value from `a` to `b`, inclusive. |
| `a-b/n` | `10-30/2` | Every value from `a` to `b`, inclusive, that is divisible by `n`. |
| `a` | `5` | A single value. |
| `a,b,c` | `0,6` | A list of single values. |

All conditions are combined with AND: a time matches only if the minute, hour, day of month, month and day of week all match. This differs from standard cron, where a restricted day of month and a restricted day of week are combined with OR.

Behaviors to be aware of:

- Steps are divisibility tests, not offsets from the start. `*/5` in the day-of-month field yields 5, 10, 15, 20, 25, 30 (not 1, 6, 11, ...). `1-10/3` in the minute field yields 3, 6, 9 (not 1, 4, 7, 10). `*/2` in the month field yields 2, 4, ..., 12.
- Not supported: names (`MON`, `JAN`), `?`, `L`, `W`, `#`, `7` for Sunday, steps on a single value (`5/10`), lists that contain ranges (`1-5,10`), and macros such as `@daily`.
- Unsupported tokens are not rejected. They are skipped or split into extra matches, which shifts the remaining fields. For example, `0 0 * * MON` has only four matches, so the day of week becomes `*` and the job runs every day at midnight. `1-5,10 * * * *` yields six matches (`1-5`, `10`, `*`, ...), so `10` becomes the hour.
- Values outside a field's range are accepted but never match (for example, minute `70`).
- `IsValid` returns `true` if the expression contains at least one match, so it only rejects strings with no digits and no `*` at all.

| Expression | Runs |
|---|---|
| `*` | Every minute. |
| `10` | At minute 10 of every hour. |
| `*/10` | At minutes 0, 10, 20, 30, 40 and 50 of every hour. |
| `10 1 * * 0` | At 01:10 every Sunday. |
| `0 9-17 * * 1-5` | On the hour from 09:00 to 17:00, Monday to Friday. |
| `*/20 * 10,20,30 * *` | At minutes 0, 20 and 40 of every hour on days 10, 20 and 30 of each month. |
| `0 0 1 * *` | At midnight on the first day of each month. |

## Getting started

```csharp
using System;
using System.Threading;
using CrispyWaffle.Scheduler;

var counter = 0;

var jobManager = new JobManager();
jobManager.AddJob("* * * * *", () => Interlocked.Increment(ref counter));
jobManager.Start();

Thread.Sleep(TimeSpan.FromMinutes(2));

jobManager.Stop();
Console.WriteLine(counter); // typically 1 or 2, depending on when the manager was created
```

## Reference

### `JobManager`

`public class JobManager : IJobManager` — holds a list of jobs and triggers them from a 30-second timer, at most once per minute.

| Member | Description |
|---|---|
| `JobManager()` | Creates the manager and its timer. The timer is not started. |
| `AddJob(string cronExpression, ThreadStart action)` | Creates a `JobRunner` for the expression and action and adds it. Throws `ArgumentNullException` if the expression is null or whitespace or the action is null, `ArgumentOutOfRangeException` if the expression is not valid. The job list is not thread-safe; add jobs before calling `Start`. |
| `Start()` | Starts the timer. |
| `Stop()` | Stops the timer. Running jobs continue to completion. |

### `IJobManager`

`public interface IJobManager` — contract for a component that schedules and runs jobs.

| Member | Description |
|---|---|
| `AddJob(string cronExpression, ThreadStart action)` | Adds a job with a schedule and the delegate to run. |
| `Start()` | Begins running scheduled jobs. |
| `Stop()` | Stops triggering jobs until `Start` is called again. |

### `IJobRunner`

`public interface IJobRunner` — contract for a single scheduled job.

| Member | Description |
|---|---|
| `Execute(DateTime dateTime)` | Runs the job if `dateTime` matches its schedule. |

### `JobRunner`

`public class JobRunner : IJobRunner` — runs one `ThreadStart` delegate on a new thread when a `CronScheduler` matches.

You normally create these through `JobManager.AddJob`. Create one directly to drive a schedule from your own timer.

| Member | Description |
|---|---|
| `JobRunner(string schedule, ThreadStart threadStart)` | Builds a `CronScheduler` from `schedule`. Throws `ArgumentNullException` if `schedule` is null or whitespace or `threadStart` is null, `ArgumentOutOfRangeException` if `schedule` is not a valid expression. |
| `Execute(DateTime dateTime)` | If `dateTime` matches the schedule and the previous run is not still running, starts the delegate on a new thread and returns immediately. Thread-safe. |

### `IScheduler`

`public interface IScheduler` — contract for an object that parses a schedule expression and tests dates against it.

| Member | Description |
|---|---|
| `IsValid(string expression)` | Returns `true` if the expression is acceptable to the scheduler. |
| `IsTime(DateTime dateTime)` | Returns `true` if `dateTime` matches the schedule. |
| `Minutes` | `ICollection<int>` of matching minutes. |
| `Hours` | `ICollection<int>` of matching hours. |
| `DaysOfMonth` | `ICollection<int>` of matching days of the month. |
| `Months` | `ICollection<int>` of matching months. |
| `DaysOfWeek` | `ICollection<int>` of matching days of the week. In `CronScheduler` these use `System.DayOfWeek` numbering: 0 = Sunday through 6 = Saturday. |

### `CronScheduler`

`public class CronScheduler : IScheduler` — parses a five-field expression (see [CRON syntax](#cron-syntax)) into sets of matching values.

| Member | Description |
|---|---|
| `CronScheduler()` | Creates an instance with no expression. All collections are empty and `IsTime` always returns `false`. Useful only for calling `IsValid`. |
| `CronScheduler(string expression)` | Parses `expression`. Throws `ArgumentOutOfRangeException` if `IsValid` returns `false`, `ArgumentNullException` if `expression` is null. |
| `IsValid(string expression)` | Returns `true` if `ValidationRegex` finds at least one match in `expression`. This is a weak check; see [CRON syntax](#cron-syntax). |
| `IsTime(DateTime dateTime)` | Returns `true` if the minute, hour, day, month and day of week of `dateTime` are all in their sets. Seconds are ignored. |
| `Minutes`, `Hours`, `DaysOfMonth`, `Months`, `DaysOfWeek` | The parsed sets, as `ICollection<int>`. They are the live internal sets; do not modify them. |

### `CronSchedulerValidations`

`public static class CronSchedulerValidations` — the regular expressions used to tokenize and classify CRON fields.

All are `public static readonly Regex`, compiled, with a 5-second match timeout.

| Member | Pattern | Matches |
|---|---|---|
| `DividedRegex` | `(\*/\d+)` | `*/n` |
| `RangeRegex` | `(\d+\-\d+)\/?(\d+)?` | `a-b` and `a-b/n` |
| `WildRegex` | `(\*)` | `*` |
| `ListRegex` | `(((\d+,)*\d+)+)` | `a` and `a,b,c` |
| `ValidationRegex` | The four above joined with `\|`, in that order | Any single field. Used to split an expression into fields. |

When classifying a field, `CronScheduler` tests `DividedRegex`, then `RangeRegex`, then `WildRegex`, then `ListRegex`, and uses the first that matches.

## Examples

### Several jobs with error handling

```csharp
using System;
using CrispyWaffle.Log;
using CrispyWaffle.Scheduler;

var jobManager = new JobManager();

jobManager.AddJob("*/5", () =>
{
    try
    {
        PurgeExpiredSessions();
    }
    catch (Exception ex)
    {
        LogConsumer.Handle(ex);
    }
});

jobManager.AddJob("0 3 * * 0", () =>
{
    try
    {
        RebuildSearchIndex(); // 03:00 every Sunday
    }
    catch (Exception ex)
    {
        LogConsumer.Handle(ex);
    }
});

jobManager.Start();
```

### Inspecting a schedule

```csharp
using System;
using CrispyWaffle.Scheduler;

var scheduler = new CronScheduler("* 10-15 * * *");

Console.WriteLine(string.Join(",", scheduler.Hours)); // 10,11,12,13,14,15

var weekend = new CronScheduler("* * * * 0,6");
Console.WriteLine(weekend.IsTime(new DateTime(2020, 9, 5, 12, 0, 0))); // True (Saturday)
```

### Driving a runner from your own clock

```csharp
using System;
using CrispyWaffle.Scheduler;

IJobRunner runner = new JobRunner("0 * * * *", () => Console.WriteLine("top of the hour"));

runner.Execute(new DateTime(2024, 1, 1, 10, 0, 0)); // starts the job
runner.Execute(new DateTime(2024, 1, 1, 10, 1, 0)); // does nothing
```

## See also

- [BackgroundJobs](../packages/background-jobs.md)
- [Logging](logging.md)
