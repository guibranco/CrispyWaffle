using CrispyWaffle.BackgroundJobs.Core;

namespace CrispyWaffle.BackgroundJobs.Abstractions;

/// <summary>
/// Typed handler used by persisted jobs. Implement this to allow DI-resolved handlers.
/// TData is the payload type stored as JSON in the job store.
/// </summary>
/// <typeparam name="TData">The payload type of the jobs this handler runs.</typeparam>
public interface IBackgroundJobHandler<TData>
{
    /// <summary>
    /// Runs a job with the given payload.
    /// </summary>
    /// <param name="data">The payload of the job, deserialized from its JSON representation.</param>
    /// <param name="cancellationToken">A token to cancel the execution.</param>
    /// <returns>A task whose result is the outcome of the job.</returns>
    Task<JobResult> HandleAsync(TData data, CancellationToken cancellationToken);
}
