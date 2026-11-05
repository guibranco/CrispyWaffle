namespace CrispyWaffle.BackgroundJobs.Abstractions;

using System;

/// <summary>
/// Persistent representation of a job.
/// HandlerName identifies a registered handler; Payload is JSON.
/// </summary>
public class JobEntity
{
    /// <summary>
    /// Gets or sets the unique identifier of the job.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the logical handler name registered in IJobHandlerRegistry.
    /// </summary>
    public string HandlerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON payload (serialized) for the handler.
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the priority of the job.
    /// </summary>
    public JobPriority Priority { get; set; } = JobPriority.Normal;

    /// <summary>
    /// Gets or sets the lifecycle status of the job.
    /// </summary>
    public JobStatus Status { get; set; } = JobStatus.Pending;

    /// <summary>
    /// Gets or sets when the job becomes due for execution. Null means immediately.
    /// </summary>
    public DateTimeOffset? ScheduledAt { get; set; }

    /// <summary>
    /// Gets or sets the number of failed executions so far.
    /// </summary>
    public int Attempt { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum number of executions when the handler keeps asking for a retry.
    /// </summary>
    public int MaxAttempt { get; set; } = 3;

    /// <summary>
    /// Gets or sets the error recorded when the job was marked as failed.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Gets or sets when the job was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets when the job was last changed.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the optional delay in seconds between retries (used only as hint).
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 0;
}
