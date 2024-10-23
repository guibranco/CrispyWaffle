using CrispyWaffle.Log;

namespace CrispyWaffle.Seq
{
    public class SeqLogProvider : ILogProvider
    {
        private readonly SeqConnector _connector;

        public SeqLogProvider(SeqConnector connector)
        {
            _connector = connector;
        }

        public void Log(string message, LogLevel level)
        {
            // Implementation to send log to SEQ server
        }

        public void SetLogLevel(LogLevel level)
        {
            // Implementation to set log level
        }

        // Other ILogProvider methods
    }
}
