namespace CrispyWaffle.Utilities
{
    public class ResetLazyStats
    {
        public ResetLazyStats(Type type)
        {
            Type = type;
        }

        public Type Type;
        public int Loads;
        public int Resets;
        public int Hits;
        public TimeSpan SumLoadTime;
    }
}
