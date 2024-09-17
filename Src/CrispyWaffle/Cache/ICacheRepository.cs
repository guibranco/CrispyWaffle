using System;
using System.Threading.Tasks;

namespace CrispyWaffle.Cache
{
    /// <summary>
    /// The cache repository interface.
    /// </summary>
    public interface ICacheRepository
    {
        /// <summary>
        /// Gets or sets a value indicating whether [should propagate exceptions].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [should propagate exceptions]; otherwise, <c>false</c>.
        /// </value>
        bool ShouldPropagateExceptions { get; set; }

        /// <summary>
        /// Stores the specified value with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of object (the object will be cast to this type).</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        /// <param name="ttl">(Optional)The time to live for this key.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Naming",
            "CA1716:Identifiers should not match keywords",
            Justification = "Design choice."
        )]
        Task SetAsync<T>(T value, string key, TimeSpan? ttl = null);

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <typeparam name="T">The type of object (the object will be cast to this type).</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Naming",
            "CA1716:Identifiers should not match keywords",
            Justification = "Design choice."
        )]
        Task SetAsync<T>(T value, string key, string subKey);

        /// <summary>
        /// Gets the object with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The object as <typeparamref name="T"/>.<br/> A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Throws when the object with the specified key doesn't exists.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Naming",
            "CA1716:Identifiers should not match keywords",
            Justification = "Design choice."
        )]
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        /// <returns>The object as <typeparamref name="T"/>.<br/> A <see cref="Task"/> representing the asynchronous operation.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Naming",
            "CA1716:Identifiers should not match keywords",
            Justification = "Design choice."
        )]
        Task<T> GetAsync<T>(string key, string subKey);

        /// <summary>
        /// Tries to get a value based on its key, if exists return true, else false.
        /// The out parameter value is the object requested.
        /// </summary>
        /// <typeparam name="T">The type of object to return if found.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="Tuple"/> contating <see cref="bool"/> Exists that contains the success info of the get, and <typeparamref name="T"/> value which is the value.
        /// <br/>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<(bool Exists, T value)> TryGetAsync<T>(string key);

        /// <summary>
        /// Tries the get.
        /// </summary>
        /// <typeparam name="T">The type of object to return if found.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="Tuple"/> contating <see cref="bool"/> Exists that contains the success info of the get, and <typeparamref name="T"/> value which is the value.
        /// <br/>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<(bool Exists, T value)> TryGetAsync<T>(string key, string subKey);

        /// <summary>
        /// Removes the specified key from the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RemoveAsync(string key);

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RemoveAsync(string key, string subKey);

        /// <summary>
        /// Returns the time to live of the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The timespan until this key is expired from the cache or 0 if it's already expired or doesn't exists.<br/> A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<TimeSpan> TTLAsync(string key);

        /// <summary>
        /// Clears this instance.
        /// </summary>
        Task ClearAsync();
    }
}
