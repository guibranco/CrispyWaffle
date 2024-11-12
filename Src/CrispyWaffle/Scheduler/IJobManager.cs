using System.Threading;

namespace CrispyWaffle.Scheduler
{
    /// <summary>
    /// Defines the contract for managing scheduled jobs.
    /// Implementations of this interface should handle the scheduling, starting, and stopping of jobs based on cron expressions.
    /// </summary>
    public interface IJobManager
    {
        /// <summary>
        /// Adds a job to the scheduler with a specified cron expression and action.
        /// The cron expression defines the schedule for the job, and the action is the delegate to be executed when the job runs.
        /// </summary>
        /// <param name="cronExpression">A string representing the cron expression that defines the schedule for the job.
        /// It follows the standard cron syntax for minute, hour, day, month, and weekday.</param>
        /// <param name="action">A <see cref="ThreadStart"/> delegate representing the action to be executed when the job is triggered.
        /// This delegate contains the code that should be run on the scheduled interval.</param>
        void AddJob(string cronExpression, ThreadStart action);

        /// <summary>
        /// Starts the job manager, enabling the execution of scheduled jobs based on their cron expressions.
        /// This method initiates the job scheduling process and begins executing any scheduled jobs according to their respective cron schedules.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the job manager, halting the execution of scheduled jobs.
        /// This method stops the scheduling process and ensures that no further jobs are triggered until <see cref="Start"/> is called again.
        /// </summary>
        void Stop();
    }
}
