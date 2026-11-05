namespace CrispyWaffle.BackgroundJobs.Abstractions;

/// <summary>
/// The lifecycle state of a job.
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// The job is waiting to run, or waiting for a retry.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// The job was claimed by a worker.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// The handler of the job reported success.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// The job failed and will not be retried.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// The job was abandoned. The library itself never assigns this value.
    /// </summary>
    Dead = 4,
}
