using System.Text.Json;
using CrispyWaffle.BackgroundJobs.Abstractions;

namespace CrispyWaffle.BackgroundJobs.Core;

/// <summary>
/// Creates jobs and hands them to the job store, or to the in-memory queue when there is no store.
/// </summary>
public class JobDispatcher
{
    private readonly IServiceProvider _provider;
    private readonly BackgroundJobQueue? _queue;
    private readonly IJobStore? _store;
    private readonly JobOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobDispatcher"/> class.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    /// <param name="queue">The in-memory queue, used when there is no store.</param>
    /// <param name="store">The job store, when one is registered.</param>
    /// <param name="options">The worker settings.</param>
    public JobDispatcher(
        IServiceProvider provider,
        BackgroundJobQueue? queue,
        IJobStore? store,
        JobOptions options
    )
    {
        _provider = provider;
        _queue = queue;
        _store = store;
        _options = options;
    }

    /// <summary>
    /// Creates a job that is due immediately.
    /// </summary>
    /// <param name="handlerName">The name the handler was registered under.</param>
    /// <param name="payload">The payload of the job. It is serialized to JSON.</param>
    /// <param name="maxAttempts">
    /// The maximum number of executions when the handler keeps asking for a retry,
    /// or <c>null</c> to use <see cref="JobOptions.DefaultMaxAttempt"/>.
    /// </param>
    /// <param name="priority">The priority of the job.</param>
    /// <returns>A task whose result is the id of the new job.</returns>
    public async Task<Guid> EnqueueAsync(
        string handlerName,
        object payload,
        int? maxAttempts = null,
        JobPriority priority = JobPriority.Normal
    )
    {
        var job = new JobEntity
        {
            HandlerName = handlerName,
            Payload = JsonSerializer.Serialize(payload),
            Priority = priority,
            MaxAttempt = maxAttempts ?? _options.DefaultMaxAttempt,
            Status = JobStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        if (_store != null)
        {
            await _store.SaveAsync(job);
        }
        else
        {
            _queue?.Enqueue(job);
        }

        return job.Id;
    }

    /// <summary>
    /// Creates a job that becomes due after a delay.
    /// </summary>
    /// <param name="handlerName">The name the handler was registered under.</param>
    /// <param name="payload">The payload of the job. It is serialized to JSON.</param>
    /// <param name="delay">The time to wait before the job becomes due.</param>
    /// <param name="maxAttempts">
    /// The maximum number of executions when the handler keeps asking for a retry,
    /// or <c>null</c> to use <see cref="JobOptions.DefaultMaxAttempt"/>.
    /// </param>
    /// <param name="priority">The priority of the job.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Without a store, the delay is waited in a background task before the job is queued,
    /// so the job is lost if the process stops in the meantime.
    /// </remarks>
    public async Task ScheduleAsync(
        string handlerName,
        object payload,
        TimeSpan delay,
        int? maxAttempts = null,
        JobPriority priority = JobPriority.Normal
    )
    {
        var job = new JobEntity
        {
            HandlerName = handlerName,
            Payload = JsonSerializer.Serialize(payload),
            Priority = priority,
            MaxAttempt = maxAttempts ?? _options.DefaultMaxAttempt,
            Status = JobStatus.Pending,
            ScheduledAt = DateTimeOffset.UtcNow.Add(delay),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        if (_store != null)
        {
            await _store.SaveAsync(job);
        }
        else
        {
            // schedule in-memory using Task.Delay
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay);
                    _queue?.Enqueue(job);
                }
                catch
                { /* suppressed */
                }
            });
        }
    }
}
