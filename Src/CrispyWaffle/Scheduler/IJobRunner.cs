using System;

namespace CrispyWaffle.Scheduler
{
    /// <summary>
    /// Interface IJobRunner
    /// </summary>
    public interface IJobRunner
    {
        /// <summary>
        /// Executes the specified date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        void Execute(DateTime dateTime);
    }
}
