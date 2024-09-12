using System;
using System.Threading.Tasks;
using CrispyWaffle.Scheduler;
using Xunit;

namespace CrispyWaffle.Tests.Scheduler;

/// <summary>
/// Class JobRunnerTests.
/// </summary>
[Collection("JobRunner")]
public class JobRunnerTests
{
    /// <summary>
    /// Defines the test method ValidateJobRunnerEmptyScheduler.
    /// </summary>
    [Fact]
    public void ValidateJobRunnerEmptyScheduler()
    {
        Assert.Throws<ArgumentNullException>(() => new JobRunner(string.Empty, null));
    }

    /// <summary>
    /// Defines the test task ValidateJobRunner.
    /// </summary>
    [Fact]
    public async Task ValidateJobRunner()
    {
        var sampler = new TestObjects();

        var runner = new JobRunner("*", () => sampler.Counter++);

        for (var i = 0; i < 10; i++)
        {
            runner.Execute(DateTime.Now);
            await Task.Delay(500);
        }

        await Task.Delay(1000);

        Assert.Equal(10, sampler.Counter);
    }

    /// <summary>
    /// Defines the test task ValidateOutOfScheduler.
    /// </summary>
    [Fact]
    public async Task ValidateOutOfScheduler()
    {
        var sampler = new TestObjects();

        var runner = new JobRunner("*/5", () => sampler.Counter++);

        var date = DateTime.Parse("00:00:00");

        for (var i = 0; i <= 10; i++)
        {
            runner.Execute(date);
            await Task.Delay(500);
            date = date.AddMinutes(1);
        }

        await Task.Delay(1000);

        Assert.Equal(3, sampler.Counter);
    }

    /// <summary>
    /// Defines the test task ValidateConcurrency.
    /// </summary>
    [Fact]
    public async Task ValidateConcurrency()
    {
        const int sleepMilliseconds = 2000;

        var sampler = new TestObjects();

        var runner = new JobRunner(
            "*",
            async () =>
            {
                sampler.Counter++;
                await Task.Delay(sleepMilliseconds);
            }
        );

        runner.Execute(DateTime.Now);
        runner.Execute(DateTime.Now);

        await Task.Delay(sleepMilliseconds * 2);

        runner.Execute(DateTime.Now);
        runner.Execute(DateTime.Now);

        await Task.Delay(sleepMilliseconds);

        Assert.Equal(2, sampler.Counter);
    }
}
