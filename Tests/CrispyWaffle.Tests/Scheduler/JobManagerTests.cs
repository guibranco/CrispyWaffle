using System.Threading.Tasks;
using CrispyWaffle.Scheduler;
using Xunit;

namespace CrispyWaffle.Tests.Scheduler;

/// <summary>
/// Class JobManagerTests.
/// </summary>
[Collection("JobManager")]
public class JobManagerTests
{
    /// <summary>
    /// Defines the test task ValidateJobManager.
    /// </summary>
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
