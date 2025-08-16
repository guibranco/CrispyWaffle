namespace CrispyWaffle.BackgroundJobs.Abstractions
{
    public interface IJobStore
    {
        Task SaveAsync(JobEntity job, CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetch the next available job that is due (ScheduledAt <= UtcNow) and Pending.
        /// Implementation should atomically mark it as Processing to avoid double pick-up.
        /// </summary>
        Task<JobEntity?> FetchNextAsync(CancellationToken cancellationToken = default);

        Task MarkCompletedAsync(Guid jobId, CancellationToken cancellationToken = default);

        Task MarkFailedAsync(Guid jobId, string error, CancellationToken cancellationToken = default);

        Task MarkRetryAsync(Guid jobId, DateTimeOffset? nextAttemptAt, int attemptCount, CancellationToken cancellationToken = default);
    }
}