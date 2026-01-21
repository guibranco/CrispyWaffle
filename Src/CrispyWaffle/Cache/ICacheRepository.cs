using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrispyWaffle.Cache;

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
    /// <param name="cancellationToken">Cancel operation.</param>
    /// <returns>returns TaskValue. </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Naming",
        "CA1716:Identifiers should not match keywords",
        Justification = "Design choice."
    )]
    ValueTask SetAsync<T>(T value, string key, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the specified value.
    /// </summary>
    /// <typeparam name="T">The type of object (the object will be cast to this type).</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="key">The key.</param>
    /// <param name="subKey">The sub key.</param>
    /// <param name="cancellationToken">To cancel the operation.</param>
    /// <returns>returns TaskValue. </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Naming",
        "CA1716:Identifiers should not match keywords",
        Justification = "Design choice."
    )]
    ValueTask SetAsync<T>(T value, string key, string subKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the object with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of object to return.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="cancellationToken">Token to cancel request.</param>
    /// <returns>The object as <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">Throws when the object with the specified key doesn't exists.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Naming",
        "CA1716:Identifiers should not match keywords",
        Justification = "Design choice."
    )]
    ValueTask<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the specified key.
    /// </summary>
    /// <typeparam name="T">The type of object to return.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="subKey">The sub key.</param>
    /// <param name="cancellationToken">Token to cancel request.</param>
    /// <returns>The object as <typeparamref name="T"/>.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Naming",
        "CA1716:Identifiers should not match keywords",
        Justification = "Design choice."
    )]
    ValueTask<T> GetAsync<T>(string key, string subKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to get a value based on its key, if exists return true, else false.
    /// The out parameter value is the object requested.
    /// </summary>
    /// <typeparam name="T">The type of object to return if found.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="cancellationToken">Cancel token.</param>
    /// <returns>Returns <b>True</b> if the object with the key exists, false otherwise.</returns>
    ValueTask<(bool Success, T Value)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries the get.
    /// </summary>
    /// <typeparam name="T">The type of object to return if found.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="subKey">The sub key.</param>
    /// <param name="cancellationToken">Cancel token.</param>
    /// <returns>Success info of the get, as a bool.</returns>
    ValueTask<(bool Success, T Value)> TryGetAsync<T>(string key, string subKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the specified key from the cache.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="cancellationToken">Cancel token.</param>
    /// <returns>Completed Task.</returns>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="subKey">The sub key.</param>
    /// <param name="cancellationToken">Cancel token.</param>
    /// <returns>Completed Task.</returns>
    Task RemoveAsync(string key, string subKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the time to live of the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The timespan until this key is expired from the cache or 0 if it's already expired or doesn't exists.</returns>
    Task<TimeSpan> TTLAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears this instance.
    /// </summary>
    /// <returns>Completed Task.</returns>
    Task Clear();
}
