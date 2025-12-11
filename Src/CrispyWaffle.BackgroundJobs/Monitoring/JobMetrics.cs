using System.Collections.Concurrent;

namespace CrispyWaffle.BackgroundJobs.Monitoring
{
    public class JobMetrics
    {
        private readonly ConcurrentDictionary<string, long> _counters = new();

        public void Increment(string key)
        {
            _counters.AddOrUpdate(key, 1, (_, v) => v + 1);
        }

        public long Get(string key) => _counters.TryGetValue(key, out var v) ? v : 0;

        public System.Collections.Generic.IDictionary<string, long> Snapshot() => new System.Collections.Generic.Dictionary<string, long>(_counters);
    }
}

