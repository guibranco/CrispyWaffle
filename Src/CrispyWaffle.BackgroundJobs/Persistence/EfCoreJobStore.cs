using CrispyWaffle.BackgroundJobs.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CrispyWaffle.BackgroundJobs.Persistence
{
    public class EfCoreJobStore : IJobStore
    {
        private readonly JobDbContext _db;

        public EfCoreJobStore(JobDbContext db)
        {
            _db = db;
        }

        public async Task SaveAsync(JobEntity job, CancellationToken cancellationToken = default)
        {
            job.UpdatedAt = DateTimeOffset.UtcNow;
            _db.Jobs.Add(job);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<JobEntity?> FetchNextAsync(CancellationToken cancellationToken = default)
        {
            // We attempt to find a pending job that is due, ordered by priority and created time. We then mark it Processing inside a transaction
            var now = DateTimeOffset.UtcNow;

            // This uses a simple approach: select candidate IDs then try to update one row in a transaction; this reduces race conditions.
            var candidate = await _db.Jobs
                .Where(j => j.Status == JobStatus.Pending && (j.ScheduledAt == null || j.ScheduledAt <= now))
                .OrderBy(j => (int)j.Priority)
                .ThenBy(j => j.CreatedAt)
                .Select(j => j.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (candidate == Guid.Empty) return null;

            // Try to claim the job
            var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == candidate, cancellationToken);
            if (job == null) return null;

            job.Status = JobStatus.Processing;
            job.UpdatedAt = DateTimeOffset.UtcNow;

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                return job;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Someone else claimed it
                return null;
            }
        }

        public async Task MarkCompletedAsync(Guid jobId, CancellationToken cancellationToken = default)
        {
            var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);
            if (job == null) return;
            job.Status = JobStatus.Completed;
            job.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task MarkFailedAsync(Guid jobId, string error, CancellationToken cancellationToken = default)
        {
            var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);
            if (job == null) return;
            job.Status = JobStatus.Failed;
            job.LastError = error;
            job.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task MarkRetryAsync(Guid jobId, DateTimeOffset? nextAttemptAt, int attemptCount, CancellationToken cancellationToken = default)
        {
            var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);
            if (job == null) return;
            job.Attempt = attemptCount;
            job.Status = JobStatus.Pending;
            job.ScheduledAt = nextAttemptAt;
            job.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}