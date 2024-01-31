using System.Threading;

namespace CrispyWaffle.Scheduler
{
    /// <summary>
    /// Interface IJobManager
    /// </summary>
    public interface IJobManager
    {
        /// <summary>
        /// Adds the job.
        /// </summary>
        /// <param name="cronExpression">The cron expression.</param>
        /// <param name="action">The action.</param>
        void AddJob(string cronExpression, ThreadStart action);

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();
    }
}
