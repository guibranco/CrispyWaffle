# Scheduled jobs

## Definition

Scheduler jobs allow scheduling the execution of some method/action using `CRON expressions`.

## CRON expression

Currently supports the following formats:

- `*`: translates to `* * * * *` (`every minute`, `every day`).
- `10`: translates to `10 * * * *` (every `10th minute` of `every hour` on `every day`).
- `10 1 * * 0`: runs every `1:10 am` of every `Sunday`
- `*/10`: runs at `0, 10, 20, 30, 40 and 50` minutes of `every hour` on `every day`.
- `*/20 * 10,20,30 * *`: runs at `0, 20 and 40` minutes of `every hour` only on days `10, 20 or 30` of each month, independently of the weekday. 


Check the [Wikipedia's `CRON`](https://en.wikipedia.org/wiki/Cron) page for more examples and details.

## Examples

Using [`cron` expression](https://en.wikipedia.org/wiki/Cron) to schedule tasks/jobs inside a program.

```cs
public static class Program
{
    public static void Main(string[] args)
    {
        var exampleObj = new SomeClass();
        exampleObj.Counter = 10;

        var jobManager = new JobManager();
        jobManager.AddJob(new JobRunner("* * * * *", () => { exampleObj.Counter++; }));
        jobManager.Start();

        Thread.Sleep(120 * 1000); //waits 2 minutes

        jobManager.Stop(); //stops the manager, so no more execution runs.

        if(exampleObj.Counter == 12)
        {
            LogConsumer.Warning("Example job runned for 2 times!");
        }

    }

    internal class SomeClass 
    {
        public int Counter { get; set; } 
    }

}

```
