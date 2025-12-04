namespace CrispyWaffle.BackgroundJobs.Abstractions
{
    public enum JobStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3,
        Dead = 4
    }
}