using CrispyWaffle.Scheduler;
using System;
using System.Threading;
using Xunit;

namespace CrispyWaffle.Tests.Scheduler
{
    public class JobRunnerTests
    {
        [Fact]
        public void ValidateJobRunnerEmptyScheduler()
        {
            Assert.Throws<ArgumentNullException>(() => new JobRunner(string.Empty, null));
        }

        [Fact]
        public void ValidateJobRunner()
        {
            var sampler = new TestSample();

            var runner = new JobRunner("*", () => { sampler.Counter++; });

            for (var i = 0; i < 10; i++)
            {
                runner.Execute(DateTime.Now);
                Thread.Sleep(500);
            }

            Thread.Sleep(1000);

            Assert.Equal(10, sampler.Counter);
        }

        [Fact]
        public void ValidateOutOfScheduler()
        {
            var sampler = new TestSample();

            var runner = new JobRunner("*/5", () => { sampler.Counter++; });

            var date = DateTime.Parse("00:00:00");

            for (var i = 0; i <= 10; i++)
            {
                runner.Execute(date);
                Thread.Sleep(500);
                date = date.AddMinutes(1);
            }

            Thread.Sleep(1000);

            Assert.Equal(3, sampler.Counter);
        }

        [Fact]
        public void ValidateConcurrency()
        {
            const int sleepMilliseconds = 5000;
            var sampler = new TestSample();

            var runner = new JobRunner("*", () => { sampler.Counter++; Thread.Sleep(sleepMilliseconds); });

            runner.Execute(DateTime.Now);
            runner.Execute(DateTime.Now);

            Thread.Sleep(sleepMilliseconds);

            runner.Execute(DateTime.Now);
            runner.Execute(DateTime.Now);

            Thread.Sleep(sleepMilliseconds);

            Assert.Equal(2, sampler.Counter);


        }
    }
}
