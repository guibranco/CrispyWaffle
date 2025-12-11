namespace CrispyWaffle.BackgroundJobs.Abstractions
{
    public interface IJobScheduler
    {
        Task ScheduleAsync(string handlerName, object payload, TimeSpan delay, int maxAttempts = 3, JobPriority priority = JobPriority.Normal);

        Task EnqueueAsync(string handlerName, object payload, int maxAttempts = 3, JobPriority priority = JobPriority.Normal);
    }
}