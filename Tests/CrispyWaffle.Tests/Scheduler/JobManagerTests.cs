using System.Threading.Tasks;
using CrispyWaffle.Scheduler;
using Xunit;

namespace CrispyWaffle.Tests.Scheduler;

/// <summary>
/// Class JobManagerTests.
/// </summary>
[Collection("JobManager")]
public class JobManagerTests
    /// <summary>
    /// Validates the functionality of the JobManager by adding multiple jobs and ensuring they execute correctly.
    /// </summary>
    /// <remarks>
    /// This test method creates an instance of the JobManager and a TestObjects instance to track job execution.
    /// It adds five jobs to the JobManager, each of which increments a counter in a thread-safe manner using a lock on a sync root object.
    /// The JobManager is then started, allowing the jobs to run concurrently for a specified duration (70 seconds).
    /// After the delay, the JobManager is stopped, and an assertion is made to verify that the counter in the TestObjects instance has been incremented exactly five times,
    /// indicating that all jobs were executed as expected.
    /// This test ensures that the JobManager can handle multiple jobs and that they execute in a thread-safe manner.
    /// </remarks>
{
    [Fact]
    public async Task ValidateJobManager()
    {
        var manager = new JobManager();

        var sampler = new TestObjects();

        var syncRoot = new object();

        manager.AddJob(
            "*",
            () =>
            {
                lock (syncRoot)
                {
                    sampler.Counter++;
                }
            }
        );
        manager.AddJob(
            "*",
            () =>
            {
                lock (syncRoot)
                {
                    sampler.Counter++;
                }
            }
        );
        manager.AddJob(
            "*",
            () =>
            {
                lock (syncRoot)
                {
                    sampler.Counter++;
                }
            }
        );
        manager.AddJob(
            "*",
            () =>
            {
                lock (syncRoot)
                {
                    sampler.Counter++;
                }
            }
        );
        manager.AddJob(
            "*",
            () =>
            {
                lock (syncRoot)
                {
                    sampler.Counter++;
                }
            }
        );

        manager.Start();

        await Task.Delay(70 * 1000);

        manager.Stop();

        Assert.Equal(5, sampler.Counter);
    }
}
