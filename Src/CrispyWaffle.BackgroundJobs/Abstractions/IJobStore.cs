namespace CrispyWaffle.BackgroundJobs.Abstractions;

/// <summary>
/// Persistence for jobs. Implement it to keep jobs in another kind of storage.
/// </summary>
public interface IJobStore
{
    /// <summary>
    /// Stores a new job.
    /// </summary>
    /// <param name="job">The job to store.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SaveAsync(JobEntity job, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetch the next available job that is due (ScheduledAt &lt;= UtcNow) and Pending.
    /// Implementation should atomically mark it as Processing to avoid double pick-up.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task whose result is the next due job, or <c>null</c> when there is none.</returns>
    Task<JobEntity?> FetchNextAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a job as completed.
    /// </summary>
    /// <param name="jobId">The id of the job.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task MarkCompletedAsync(Guid jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a job as failed and records its error.
    /// </summary>
    /// <param name="jobId">The id of the job.</param>
    /// <param name="error">The error to record for the job.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task MarkFailedAsync(Guid jobId, string error, CancellationToken cancellationToken = default);

    /// <summary>
    /// Puts a job back to pending so that it is executed again.
    /// </summary>
    /// <param name="jobId">The id of the job.</param>
    /// <param name="nextAttemptAt">When the job becomes due again, or <c>null</c> for immediately.</param>
    /// <param name="attemptCount">The number of attempts made so far.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task MarkRetryAsync(
        Guid jobId,
        DateTimeOffset? nextAttemptAt,
        int attemptCount,
        CancellationToken cancellationToken = default
    );
}
