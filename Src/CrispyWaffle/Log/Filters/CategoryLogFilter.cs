using System;
using System.Collections.Generic;

namespace CrispyWaffle.Log.Filters;

/// <summary>
/// A log filter based on category and provider type.
/// Filters log messages based on whether the provided category matches
/// the categories added to the filter, and optionally the provider type.
/// </summary>
/// <seealso cref="ILogFilter" />
/// <remarks>
/// Initializes a new instance of the <see cref="CategoryLogFilter"/> class.
/// </remarks>
/// <param name="provider">The provider type to filter logs by. If <c>null</c>, no provider type filter will be applied.</param>
/// <param name="isExclusive">If set to <c>true</c>, the filter will exclude logs from the specified categories and allow all others. If set to <c>false</c>, only logs from the specified categories will be allowed.</param>
public class CategoryLogFilter(Type provider = null, bool isExclusive = true) : ILogFilter
{
    /// <summary>
    /// The provider type used for filtering log messages.
    /// </summary>
    private readonly string _provider = provider?.FullName ?? provider?.Name ?? string.Empty;

    /// <summary>
    /// The list of categories to filter by.
    /// </summary>
    private readonly List<string> _categories = new List<string>();

    /// <summary>
    /// Adds a category to the filter.
    /// </summary>
    /// <param name="category">The category to be added to the filter.</param>
    /// <returns>The current instance of <see cref="CategoryLogFilter"/> to allow method chaining.</returns>
    public CategoryLogFilter AddCategory(string category)
    {
        _categories.Add(category);
        return this;
    }

    /// <summary>
    /// Filters a log message based on its provider type and category.
    /// </summary>
    /// <param name="providerType">The type of the log provider.</param>
    /// <param name="level">The log level of the message.</param>
    /// <param name="category">The category of the log message.</param>
    /// <param name="message">The log message itself.</param>
    /// <returns><c>true</c> if the log message is allowed to pass through, <c>false</c> if it should be dropped.</returns>
    /// <remarks>
    /// If a provider type is specified, log messages for non-matching provider types always pass through (<c>true</c>).
    /// For matching providers (or when no provider type is specified), if the <c>isExclusive</c> flag is set to <c>true</c>,
    /// the log message passes through only if its category is not in the filter (listed categories are excluded).
    /// If the flag is set to <c>false</c>, the message passes through only if its category is in the filter.
    /// </remarks>
    public bool Filter(string providerType, LogLevel level, string category, string message)
    {
        if (
            !string.IsNullOrWhiteSpace(_provider)
            && !_provider.Equals(providerType, StringComparison.OrdinalIgnoreCase)
        )
        {
            return true;
        }

        return isExclusive ? !_categories.Contains(category) : _categories.Contains(category);
    }
}
