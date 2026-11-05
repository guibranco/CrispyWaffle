namespace CrispyWaffle.BackgroundJobs.Exceptions;

/// <summary>
/// The exception that represents the failure of a job.
/// </summary>
public class JobFailedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JobFailedException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the failure.</param>
    public JobFailedException(string message)
        : base(message) { }
}
