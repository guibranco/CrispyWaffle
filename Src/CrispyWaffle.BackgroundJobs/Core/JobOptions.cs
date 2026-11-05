namespace CrispyWaffle.BackgroundJobs.Core
{
    /// <summary>
    /// Settings of the background worker.
    /// </summary>
    public class JobOptions
    {
        /// <summary>
        /// Gets or sets the number of concurrent worker loops.
        /// </summary>
        public int WorkerCount { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// Gets or sets the <see cref="Abstractions.JobEntity.MaxAttempt"/> given to jobs that
        /// <see cref="JobDispatcher"/> enqueues without an explicit value.
        /// </summary>
        /// <remarks>
        /// The value counts executions, not retries: with the default of 3, a handler that keeps asking
        /// for a retry runs three times. It is not checked when the handler throws an exception: such a
        /// job is always scheduled to run again.
        /// </remarks>
        public int DefaultMaxAttempt { get; set; } = 3;

        /// <summary>
        /// Gets or sets the base polling delay (when using persistent store and no job found).
        /// </summary>
        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Gets or sets the maximum exponential backoff seconds for retry.
        /// </summary>
        /// <remarks>
        /// The worker does not read this value: its retry backoff is capped at a fixed 300 seconds.
        /// </remarks>
        public int MaxBackoffSeconds { get; set; } = 300; // 5 minutes
    }
}
