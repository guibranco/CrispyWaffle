using CrispyWaffle.BackgroundJobs.Core;

namespace CrispyWaffle.BackgroundJobs.Abstractions
{
    /// <summary>
    /// Low-level job representation (mostly used for in-memory workflows).
    /// For persisted jobs, prefer IBackgroundJobHandler<TData> with registry-based activation.
    /// </summary>
    public interface IBackgroundJob
    {
        Task<JobResult> ExecuteAsync(CancellationToken cancellationToken);
    }
}