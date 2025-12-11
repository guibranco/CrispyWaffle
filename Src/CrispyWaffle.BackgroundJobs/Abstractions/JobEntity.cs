namespace CrispyWaffle.BackgroundJobs.Abstractions
{
    using System;

    /// <summary>
    /// Persistent representation of a job.
    /// HandlerName identifies a registered handler; Payload is JSON.
    /// </summary>
    public class JobEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Logical handler name registered in IJobHandlerRegistry.
        /// </summary>
        public string HandlerName { get; set; } = string.Empty;

        /// <summary>
        /// JSON payload (serialized) for the handler.
        /// </summary>
        public string Payload { get; set; } = string.Empty;

        public JobPriority Priority { get; set; } = JobPriority.Normal;

        public JobStatus Status { get; set; } = JobStatus.Pending;

        /// <summary>
        /// When the job becomes due for execution. Null means immediately.
        /// </summary>
        public DateTimeOffset? ScheduledAt { get; set; }

        public int Attempt { get; set; } = 0;

        public int MaxAttempt { get; set; } = 3;

        public string? LastError { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Optional delay in seconds between retries (used only as hint).
        /// </summary>
        public int RetryDelaySeconds { get; set; } = 0;
    }
}