namespace CrispyWaffle.Log.Filters;

/// <summary>
/// The log filter interface
/// </summary>
public interface ILogFilter
{
    /// <summary>
    /// Filters the specified provider type.
    /// </summary>
    /// <param name="providerType">Type of the provider.</param>
    /// <param name="level">The level.</param>
    /// <param name="category">The category.</param>
    /// <param name="message">The message.</param>
    /// <returns></returns>
    bool Filter(string providerType, LogLevel level, string category, string message);
}
