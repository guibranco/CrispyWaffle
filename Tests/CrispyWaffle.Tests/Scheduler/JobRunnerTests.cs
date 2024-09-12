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
    /// Validates the execution of the JobRunner by simulating multiple executions.
    /// </summary>
    /// <remarks>
    /// This test method creates an instance of the <see cref="JobRunner"/> class, which is designed to execute a specified action at defined intervals.
    /// In this case, the action increments a counter in the <see cref="TestObjects"/> class. 
    /// The method runs the JobRunner's execute method 10 times, with a delay of 500 milliseconds between each execution.
    /// After all executions, it waits an additional second to ensure all actions are completed before asserting that the counter has been incremented to 10.
    /// This ensures that the JobRunner behaves as expected when executing scheduled tasks.
    /// </remarks>
    /// <exception cref="System.Exception">Thrown if the assertion fails, indicating that the counter did not reach the expected value.</exception>
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
    /// Validates the execution of a job scheduler by simulating time progression.
    /// </summary>
    /// <remarks>
    /// This asynchronous test method creates an instance of a job runner that is scheduled to execute every 5 minutes.
    /// It simulates the passage of time by incrementing the date in one-minute intervals and executes the job runner accordingly.
    /// The test checks that the job is executed the expected number of times (3) within the simulated timeframe.
    /// The method uses a counter from a test object to track how many times the job has been executed.
    /// After running the job for a total of 11 minutes, it asserts that the counter equals 3, indicating that the job was triggered correctly.
    /// </remarks>
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
    /// Validates the concurrency of job execution in the JobRunner class.
    /// </summary>
    /// <remarks>
    /// This asynchronous test method checks if the JobRunner can handle concurrent executions correctly.
    /// It initializes a sampler object to track the number of times a job is executed. 
    /// The job increments the counter and simulates a delay to mimic processing time. 
    /// The method executes the job multiple times in quick succession and then waits for a specified duration 
    /// to ensure that all jobs have completed before asserting the final count of executions. 
    /// The expected outcome is that the counter should equal 2, indicating that only two jobs were processed 
    /// concurrently despite multiple execution calls.
    /// </remarks>
    /// <exception cref="System.Exception">Thrown when the assertion fails.</exception>
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
