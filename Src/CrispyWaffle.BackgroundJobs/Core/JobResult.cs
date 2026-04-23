namespace CrispyWaffle.BackgroundJobs.Core
{
    public sealed record JobResult(bool Success, bool Retry = false, string? ErrorMessage = null)
    {
        public static JobResult Ok() => new(true);
        public static JobResult Fail(string message, bool retry = false) => new(false, retry, message);
    }
}
