namespace CrispyWaffle.Log.Adapters
{
    /// <summary>
    /// Defines an interface for logging messages to the console.
    /// Implementers of this interface should handle logging to the console, supporting different log levels and formats.
    /// This allows for easy logging directly to the console for debugging and monitoring purposes.
    /// </summary>
    /// <seealso cref="ILogAdapter" />
    public interface IConsoleLogAdapter : ILogAdapter
    {
        // Inheriting from ILogAdapter means any log levels and methods from ILogAdapter would also apply here
    }
}
