using CrispyWaffle.BackgroundJobs.Core;

namespace CrispyWaffle.BackgroundJobs.Abstractions
{
    /// <summary>
    /// Low-level job representation (mostly used for in-memory workflows).
    /// For persisted jobs, prefer <see cref="IBackgroundJobHandler{TData}"/> with registry-based activation.
    /// </summary>
    public interface IBackgroundJob
    {
        /// <summary>
        /// Executes the job.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the execution.</param>
        /// <returns>A task whose result is the outcome of the job.</returns>
        Task<JobResult> ExecuteAsync(CancellationToken cancellationToken);
    }
}
