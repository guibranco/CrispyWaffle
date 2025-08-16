using CrispyWaffle.BackgroundJobs.Abstractions;
using System.Threading.Channels;

namespace CrispyWaffle.BackgroundJobs.Core
{
    /// <summary>
    /// Lightweight in-memory priority queue using Channel for signaling.
    /// Priorities: lower int = higher priority.
    /// This queue stores JobEntity (for uniformity with persistent store model).
    /// </summary>
    public class BackgroundJobQueue
    {
        private readonly object _lock = new();
        private readonly PriorityQueue<JobEntity, int> _pq = new();
        private readonly Channel<JobEntity> _signal = Channel.CreateUnbounded<JobEntity>(new UnboundedChannelOptions { SingleReader = false, SingleWriter = false });

        public void Enqueue(JobEntity job)
        {
            lock (_lock)
            {
                _pq.Enqueue(job, (int)job.Priority);
            }
            // Always write to signal channel to notify consumers.
            _ = _signal.Writer.WriteAsync(job);
        }

        public async Task<JobEntity?> DequeueAsync(CancellationToken cancellationToken)
        {
            // Wait until a job is signaled and then pop from our priority queue.
            try
            {
                await _signal.Reader.ReadAsync(cancellationToken);
            }
            catch (OperationCanceledException) { return null; }

            lock (_lock)
            {
                if (_pq.TryDequeue(out var job, out _))
                {
                    return job;
                }
            }

            return null;
        }
    }
}