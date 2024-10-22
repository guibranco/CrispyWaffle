using CrispyWaffle.Log.Adapters;

namespace CrispyWaffle.Seq
{
    public class SeqBatchLogAdapter : ICategorizedLogAdapter
    {
        private readonly SeqConnector _connector;
        private readonly TimeSpan _batchTimeout;
        private readonly int _batchSize;

        public SeqBatchLogAdapter(SeqConnector connector, TimeSpan batchTimeout, int batchSize)
        {
            _connector = connector;
            _batchTimeout = batchTimeout;
            _batchSize = batchSize;
        }

        public void Log(string category, string message, LogLevel level)
        {
            // Implementation to batch and send logs to SEQ server
        }

        public void SetLogLevel(LogLevel level)
        {
            // Implementation to set log level
        }

        // Other ICategorizedLogAdapter methods
    }
}
