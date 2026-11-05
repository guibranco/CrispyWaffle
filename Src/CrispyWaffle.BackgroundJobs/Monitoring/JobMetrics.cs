using System.Collections.Concurrent;

namespace CrispyWaffle.BackgroundJobs.Monitoring
{
    /// <summary>
    /// Thread-safe named counters for background jobs.
    /// </summary>
    public class JobMetrics
    {
        private readonly ConcurrentDictionary<string, long> _counters = new();

        /// <summary>
        /// Adds one to a counter, creating it when it does not exist yet.
        /// </summary>
        /// <param name="key">The name of the counter.</param>
        public void Increment(string key)
        {
            _counters.AddOrUpdate(key, 1, (_, v) => v + 1);
        }

        /// <summary>
        /// Gets the value of a counter.
        /// </summary>
        /// <param name="key">The name of the counter.</param>
        /// <returns>The value of the counter, or zero when it does not exist.</returns>
        public long Get(string key) => _counters.TryGetValue(key, out var v) ? v : 0;

        /// <summary>
        /// Takes a copy of all the counters.
        /// </summary>
        /// <returns>A dictionary with the value of each counter, by name.</returns>
        public System.Collections.Generic.IDictionary<string, long> Snapshot() =>
            new System.Collections.Generic.Dictionary<string, long>(_counters);
    }
}
