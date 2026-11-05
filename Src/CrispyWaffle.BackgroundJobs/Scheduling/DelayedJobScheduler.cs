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

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayedJobScheduler"/> class.
        /// </summary>
        /// <param name="provider">The service provider used to resolve the <see cref="JobDispatcher"/>.</param>
        public DelayedJobScheduler(IServiceProvider provider)
        {
            _provider = provider;
        }

        /// <inheritdoc />
        public Task EnqueueAsync(
            string handlerName,
            object payload,
            int maxAttempts = 3,
            JobPriority priority = JobPriority.Normal
        )
        {
            var dispatcher = _provider.GetRequiredService<JobDispatcher>();
            return dispatcher.EnqueueAsync(handlerName, payload, maxAttempts, priority);
        }

        /// <inheritdoc />
        public Task ScheduleAsync(
            string handlerName,
            object payload,
            TimeSpan delay,
            int maxAttempts = 3,
            JobPriority priority = JobPriority.Normal
        )
        {
            var dispatcher = _provider.GetRequiredService<JobDispatcher>();
            return dispatcher.ScheduleAsync(handlerName, payload, delay, maxAttempts, priority);
        }
    }
}
