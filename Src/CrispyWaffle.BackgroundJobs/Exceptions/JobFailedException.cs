namespace CrispyWaffle.BackgroundJobs.Exceptions
{
    public class JobFailedException : Exception
    {
        public JobFailedException(string message) : base(message) { }
    }
}
