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
        /// Gets or sets the default maximum retry attempts for jobs enqueued without explicit MaxAttempt.
        /// </summary>
        public int DefaultMaxAttempt { get; set; } = 3;

        /// <summary>
        /// Gets or sets the base polling delay (when using persistent store and no job found).
        /// </summary>
        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Gets or sets the maximum exponential backoff seconds for retry.
        /// </summary>
        public int MaxBackoffSeconds { get; set; } = 300; // 5 minutes
    }
}
