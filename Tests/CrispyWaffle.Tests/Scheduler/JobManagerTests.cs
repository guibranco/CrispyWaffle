using CrispyWaffle.Scheduler;
using System.Threading;
using Xunit;

namespace CrispyWaffle.Tests.Scheduler
{
    public class JobManagerTests
    {
        [Fact]
        public void ValidateJobManager()
        {
            var manager = new JobManager();

            var sampler = new TestSample();

            var syncRoot = new object();

            manager.AddJob("*", () => { lock (syncRoot) { sampler.Counter++; } });
            manager.AddJob("*", () => { lock (syncRoot) { sampler.Counter++; } });
            manager.AddJob("*", () => { lock (syncRoot) { sampler.Counter++; } });
            manager.AddJob("*", () => { lock (syncRoot) { sampler.Counter++; } });
            manager.AddJob("*", () => { lock (syncRoot) { sampler.Counter++; } });

            manager.Start();

            Thread.Sleep(40 * 1000);

            manager.Stop();

            Assert.Equal(5, sampler.Counter);

        }
    }
}
