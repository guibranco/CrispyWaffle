namespace CrispyWaffle.BackgroundJobs.Abstractions
{
    /// <summary>
    /// The priority of a job. Jobs with a lower value are picked first.
    /// </summary>
    public enum JobPriority
    {
        /// <summary>
        /// Picked before <see cref="Normal"/> and <see cref="Low"/> jobs.
        /// </summary>
        High = 0,

        /// <summary>
        /// The default priority.
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Picked after all the other jobs.
        /// </summary>
        Low = 2,
    }
}
