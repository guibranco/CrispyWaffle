using CrispyWaffle.BackgroundJobs.Core;

namespace CrispyWaffle.BackgroundJobs.Abstractions
{
    /// <summary>
    /// Typed handler used by persisted jobs. Implement this to allow DI-resolved handlers.
    /// TData is the payload type stored as JSON in the job store.
    /// </summary>
    public interface IBackgroundJobHandler<TData>
    {
        Task<JobResult> HandleAsync(TData data, CancellationToken cancellationToken);
    }
}
