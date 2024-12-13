using System.Threading.Tasks;
using CrispyWaffle.Scheduler;
using Xunit;

namespace CrispyWaffle.Tests.Scheduler;

[Collection("JobManager")]
public class JobManagerTests
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
