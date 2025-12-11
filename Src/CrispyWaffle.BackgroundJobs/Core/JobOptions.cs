namespace CrispyWaffle.BackgroundJobs.Core
{
    public class JobOptions
    {
        /// <summary>
        /// Number of concurrent worker loops.
        /// </summary>
        public int WorkerCount { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// Default maximum retry attempts for jobs enqueued without explicit MaxAttempt.
        /// </summary>
        public int DefaultMaxAttempt { get; set; } = 3;

        /// <summary>
        /// Base polling delay (when using persistent store and no job found).
        /// </summary>
        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Maximum exponential backoff seconds for retry.
        /// </summary>
        public int MaxBackoffSeconds { get; set; } = 300; // 5 minutes
    }
}
