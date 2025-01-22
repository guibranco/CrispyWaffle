using System;

namespace CrispyWaffle.Scheduler;

/// <summary>
/// Defines the contract for a job runner that executes scheduled jobs based on a specified date and time.
/// </summary>
/// <remarks>
/// Implementations of this interface are responsible for executing jobs at a specific time, typically
/// based on a scheduling mechanism. This can be useful for running background tasks, maintenance jobs, or
/// other time-based operations.
/// </remarks>
public interface IJobRunner
{
    /// <summary>
    /// Executes a job at the specified date and time.
    /// </summary>
    /// <param name="dateTime">The date and time when the job should be executed. This is typically
    /// provided by a scheduling system or the job runner itself.</param>
    /// <remarks>
    /// The implementation of this method should define the behavior for executing the job at the given
    /// <paramref name="dateTime"/>. For example, it could trigger specific processes or perform actions
    /// related to the scheduled task.
    /// </remarks>
    void Execute(DateTime dateTime);
}
