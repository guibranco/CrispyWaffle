namespace CrispyWaffle.Log.Filters
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The category log filter class.
    /// </summary>
    /// <seealso cref="ILogFilter" />
    public class CategoryLogFilter : ILogFilter
    {
        #region Private fields

        /// <summary>
        /// The type
        /// </summary>
        private readonly string _provider;

        /// <summary>
        /// The categories
        /// </summary>
        private readonly List<string> _categories;

        /// <summary>
        /// The is exclusive
        /// </summary>
        private readonly bool _isExclusive;

        #endregion

        #region ~Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryLogFilter"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="isExclusive">if set to <c>true</c> [is exclusive].</param>
        public CategoryLogFilter(Type provider = null, bool isExclusive = true)
        {
            _categories = new List<string>();
            _isExclusive = isExclusive;
            _provider = provider?.FullName ?? provider?.Name ?? string.Empty;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds the category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        public CategoryLogFilter AddCategory(string category)
        {
            _categories.Add(category);
            return this;
        }

        #endregion

        #region Implementation of ILogFilter

        /// <summary>
        /// Filters the specified provider type.
        /// </summary>
        /// <param name="providerType">Type of the provider.</param>
        /// <param name="level">The level.</param>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public bool Filter(string providerType, LogLevel level, string category, string message)
        {
            if (!string.IsNullOrWhiteSpace(_provider) && !_provider.Equals(providerType))
                return true;
            return _isExclusive ? !_categories.Contains(category) : _categories.Contains(category);
        }

        #endregion
    }
}
