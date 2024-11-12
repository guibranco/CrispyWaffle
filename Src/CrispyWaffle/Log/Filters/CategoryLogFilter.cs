using System;
using System.Collections.Generic;

namespace CrispyWaffle.Log.Filters
{
    /// <summary>
    /// A log filter based on category and provider type.
    /// Filters log messages based on whether the provided category matches
    /// the categories added to the filter, and optionally the provider type.
    /// </summary>
    /// <seealso cref="ILogFilter" />
    public class CategoryLogFilter : ILogFilter
    {
        /// <summary>
        /// The provider type used for filtering log messages.
        /// </summary>
        private readonly string _provider;

        /// <summary>
        /// The list of categories to filter by.
        /// </summary>
        private readonly List<string> _categories;

        /// <summary>
        /// Indicates whether the filter is exclusive. If <c>true</c>, only the categories
        /// in the filter will pass through. If <c>false</c>, all categories except those
        /// in the filter will pass through.
        /// </summary>
        private readonly bool _isExclusive;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryLogFilter"/> class.
        /// </summary>
        /// <param name="provider">The provider type to filter logs by. If <c>null</c>, no provider type filter will be applied.</param>
        /// <param name="isExclusive">If set to <c>true</c>, the filter will only allow logs from the specified categories. If set to <c>false</c>, all categories except those specified will be allowed.</param>
        public CategoryLogFilter(Type provider = null, bool isExclusive = true)
        {
            _categories = new List<string>();
            _isExclusive = isExclusive;
            _provider = provider?.FullName ?? provider?.Name ?? string.Empty;
        }

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
        /// <returns><c>true</c> if the log message should be filtered (i.e., not allowed to pass through), <c>false</c> otherwise.</returns>
        /// <remarks>
        /// If a provider type is specified, the log message will only pass through if it matches
        /// the provided provider type. If the <see cref="_isExclusive"/> flag is set to <c>true</c>,
        /// the log message will only pass through if its category is one of the specified categories.
        /// If the flag is set to <c>false</c>, the message will pass through only if its category
        /// is not in the filter.
        /// </remarks>
        public bool Filter(string providerType, LogLevel level, string category, string message)
        {
            if (!string.IsNullOrWhiteSpace(_provider) && !_provider.Equals(providerType))
            {
                return true;
            }

            return _isExclusive ? !_categories.Contains(category) : _categories.Contains(category);
        }
    }
}
