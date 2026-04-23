using System.Collections.Concurrent;
using CrispyWaffle.BackgroundJobs.Abstractions;

namespace CrispyWaffle.BackgroundJobs.Persistence
{
    public class InMemoryJobStore : IJobStore
    {
        private readonly ConcurrentDictionary<Guid, JobEntity> _storage = new();

        public Task SaveAsync(JobEntity job, CancellationToken cancellationToken = default)
        {
            job.UpdatedAt = DateTimeOffset.UtcNow;
            _storage[job.Id] = job;
            return Task.CompletedTask;
        }

        public Task<JobEntity?> FetchNextAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var next = _storage.Values
                .Where(j => j.Status == JobStatus.Pending && (!j.ScheduledAt.HasValue || j.ScheduledAt.Value <= now))
                .OrderBy(j => (int)j.Priority)
                .ThenBy(j => j.CreatedAt)
                .FirstOrDefault();

            if (next != null)
            {
                // try to transition to Processing; using atomic update
                next.Status = JobStatus.Processing;
                next.UpdatedAt = DateTimeOffset.UtcNow;
                _storage[next.Id] = next;
                return Task.FromResult<JobEntity?>(next);
            }

            return Task.FromResult<JobEntity?>(null);
        }

        public Task MarkCompletedAsync(Guid jobId, CancellationToken cancellationToken = default)
        {
            if (_storage.TryGetValue(jobId, out var job))
            {
                job.Status = JobStatus.Completed;
                job.UpdatedAt = DateTimeOffset.UtcNow;
                _storage[jobId] = job;
            }
            return Task.CompletedTask;
        }

        public Task MarkFailedAsync(Guid jobId, string error, CancellationToken cancellationToken = default)
        {
            if (_storage.TryGetValue(jobId, out var job))
            {
                job.Status = JobStatus.Failed;
                job.LastError = error;
                job.UpdatedAt = DateTimeOffset.UtcNow;
                _storage[jobId] = job;
            }
            return Task.CompletedTask;
        }

        public Task MarkRetryAsync(Guid jobId, DateTimeOffset? nextAttemptAt, int attemptCount, CancellationToken cancellationToken = default)
        {
            if (_storage.TryGetValue(jobId, out var job))
            {
                job.Attempt = attemptCount;
                job.Status = JobStatus.Pending;
                job.ScheduledAt = nextAttemptAt;
                job.UpdatedAt = DateTimeOffset.UtcNow;
                _storage[jobId] = job;
            }
            return Task.CompletedTask;
        }
    }
}
