using System.Text.Json;
using CrispyWaffle.BackgroundJobs.Abstractions;

namespace CrispyWaffle.BackgroundJobs.Core
{
    public class JobDispatcher
    {
        private readonly IServiceProvider _provider;
        private readonly BackgroundJobQueue? _queue;
        private readonly IJobStore? _store;
        private readonly JobOptions _options;

        public JobDispatcher(IServiceProvider provider, BackgroundJobQueue? queue, IJobStore? store, JobOptions options)
        {
            _provider = provider;
            _queue = queue;
            _store = store;
            _options = options;
        }

        public async Task<Guid> EnqueueAsync(string handlerName, object payload, int? maxAttempts = null, JobPriority priority = JobPriority.Normal)
        {
            var job = new JobEntity
            {
                HandlerName = handlerName,
                Payload = JsonSerializer.Serialize(payload),
                Priority = priority,
                MaxAttempt = maxAttempts ?? _options.DefaultMaxAttempt,
                Status = JobStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow
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

        public async Task ScheduleAsync(string handlerName, object payload, TimeSpan delay, int? maxAttempts = null, JobPriority priority = JobPriority.Normal)
        {
            var job = new JobEntity
            {
                HandlerName = handlerName,
                Payload = JsonSerializer.Serialize(payload),
                Priority = priority,
                MaxAttempt = maxAttempts ?? _options.DefaultMaxAttempt,
                Status = JobStatus.Pending,
                ScheduledAt = DateTimeOffset.UtcNow.Add(delay),
                CreatedAt = DateTimeOffset.UtcNow
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
                    catch { /* suppressed */ }
                });
            }
        }
    }
}