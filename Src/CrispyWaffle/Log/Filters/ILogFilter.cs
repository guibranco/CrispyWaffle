namespace CrispyWaffle.Log.Filters;

/// <summary>
/// Defines the contract for a log filter that can be applied to log messages.
/// </summary>
/// <remarks>
/// Implementations of this interface are responsible for filtering log messages
/// based on the provider type, log level, category, and message content.
/// </remarks>
public interface ILogFilter
{
    /// <summary>
    /// Filters a log message based on the provided criteria.
    /// </summary>
    /// <param name="providerType">The type of the provider generating the log message (e.g., application, service).</param>
    /// <param name="level">The log level of the message (e.g., Information, Warning, Error).</param>
    /// <param name="category">The category or context of the log message (e.g., component name, module).</param>
    /// <param name="message">The log message content that needs to be filtered.</param>
    /// <returns>
    /// <c>true</c> if the log message should be included; otherwise, <c>false</c> to exclude the message.
    /// </returns>
    /// <remarks>
    /// This method allows the implementation to decide whether to include or exclude a specific log message
    /// based on the provided parameters. This could be useful for filtering logs based on severity, provider, or context.
    /// </remarks>
    bool Filter(string providerType, LogLevel level, string category, string message);
}
