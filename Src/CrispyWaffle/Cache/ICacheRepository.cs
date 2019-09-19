namespace CrispyWaffle.Cache
{
    using System;

    /// <summary>
    /// The cache repository interface
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
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        /// <param name="ttl">(Optional)The time to live for this key.</param>
        void Set<T>(T value, string key, TimeSpan? ttl = null);

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        void Set<T>(T value, string key, string subKey);

        /// <summary>
        /// Gets the object with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of object (the object will be cast to this type)</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The object as <typeparamref name="T"/></returns>
        /// <exception cref="InvalidOperationException">Throws when the object with the specified key doesn't exists</exception>
        T Get<T>(string key);

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        /// <returns></returns>
        T Get<T>(string key, string subKey);

        /// <summary>
        /// Tries to get a value based on its key, if exists return true, else false. 
        /// The out parameter value is the object requested.
        /// </summary>
        /// <typeparam name="T">The type of object (the object will be cast to this type)</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>Returns <b>True</b> if the object with the key exists, false otherwise</returns>
        bool TryGet<T>(string key, out T value);

        /// <summary>
        /// Tries the get.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        bool TryGet<T>(string key, string subKey, out T value);

        /// <summary>
        /// Removes the specified key from the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        void Remove(string key);

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        void Remove(string key, string subKey);

        /// <summary>
        /// Returns the time to live of the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The timespan until this key is expired from the cache or 0 if it's already expired or doesn't exists.</returns>
        TimeSpan TTL(string key);

        /// <summary>
        /// Clears this instance.
        /// </summary>
        void Clear();
    }
}
