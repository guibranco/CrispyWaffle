namespace CrispyWaffle.BackgroundJobs.Abstractions;

/// <summary>
/// Adds jobs for the background worker to run, either immediately or after a delay.
/// </summary>
public interface IJobScheduler
{
    /// <summary>
    /// Adds a job that becomes due after a delay.
    /// </summary>
    /// <param name="handlerName">The name the handler was registered under.</param>
    /// <param name="payload">The payload of the job. It is serialized to JSON.</param>
    /// <param name="delay">The time to wait before the job becomes due.</param>
    /// <param name="maxAttempts">The maximum number of executions when the handler keeps asking for a retry.</param>
    /// <param name="priority">The priority of the job.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ScheduleAsync(
        string handlerName,
        object payload,
        TimeSpan delay,
        int maxAttempts = 3,
        JobPriority priority = JobPriority.Normal
    );

    /// <summary>
    /// Adds a job that is due immediately.
    /// </summary>
    /// <param name="handlerName">The name the handler was registered under.</param>
    /// <param name="payload">The payload of the job. It is serialized to JSON.</param>
    /// <param name="maxAttempts">The maximum number of executions when the handler keeps asking for a retry.</param>
    /// <param name="priority">The priority of the job.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task EnqueueAsync(
        string handlerName,
        object payload,
        int maxAttempts = 3,
        JobPriority priority = JobPriority.Normal
    );
}
