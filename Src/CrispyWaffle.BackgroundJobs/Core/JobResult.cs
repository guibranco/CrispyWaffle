namespace CrispyWaffle.BackgroundJobs.Core
{
    /// <summary>
    /// The outcome of a job, as returned by its handler.
    /// </summary>
    /// <param name="Success"><c>true</c> when the job succeeded.</param>
    /// <param name="Retry">When the job did not succeed, whether it should be retried.</param>
    /// <param name="ErrorMessage">The error recorded for the job when it ends as failed.</param>
    public sealed record JobResult(bool Success, bool Retry = false, string? ErrorMessage = null)
    {
        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <returns>A result whose <see cref="Success"/> is <c>true</c>.</returns>
        public static JobResult Ok() => new(true);

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="message">The error message of the failure.</param>
        /// <param name="retry">Whether the job should be retried.</param>
        /// <returns>A result whose <see cref="Success"/> is <c>false</c>.</returns>
        public static JobResult Fail(string message, bool retry = false) =>
            new(false, retry, message);
    }
}
