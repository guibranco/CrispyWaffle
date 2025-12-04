using CrispyWaffle.BackgroundJobs.Abstractions;
using CrispyWaffle.BackgroundJobs.Core;
using Microsoft.Extensions.DependencyInjection;

namespace CrispyWaffle.BackgroundJobs.Scheduling
{
    /// <summary>
    /// Schedules jobs by either creating a persisted scheduled job (if store available) or using in-memory Task.Delay.
    /// </summary>
    public class DelayedJobScheduler : IJobScheduler
    {
        private readonly IServiceProvider _provider;

        public DelayedJobScheduler(IServiceProvider provider)
        {
            _provider = provider;
        }

        public Task EnqueueAsync(string handlerName, object payload, int maxAttempts = 3, JobPriority priority = JobPriority.Normal)
        {
            var dispatcher = _provider.GetRequiredService<JobDispatcher>();
            return dispatcher.EnqueueAsync(handlerName, payload, maxAttempts, priority);
        }

        public Task ScheduleAsync(string handlerName, object payload, TimeSpan delay, int maxAttempts = 3, JobPriority priority = JobPriority.Normal)
        {
            var dispatcher = _provider.GetRequiredService<JobDispatcher>();
            return dispatcher.ScheduleAsync(handlerName, payload, delay, maxAttempts, priority);
        }
    }
}