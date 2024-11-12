using System;
using System.Collections.Concurrent;

namespace CrispyWaffle.Cache;

/// <summary>
/// The memory cache helper class.
/// </summary>
public class MemoryCacheRepository : ICacheRepository
{
    /// <summary>
    /// The data.
    /// </summary>
    private static readonly ConcurrentDictionary<string, object> _data =
        new ConcurrentDictionary<string, object>();

    /// <summary>
    /// The hash.
    /// </summary>
    private static readonly ConcurrentDictionary<string, object> _hash =
        new ConcurrentDictionary<string, object>();

    /// <summary>
    /// Gets or sets a value indicating whether [should propagate exceptions].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [should propagate exceptions]; otherwise, <c>false</c>.
    /// </value>
    public bool ShouldPropagateExceptions { get; set; }

    /// <summary>
    /// Stores the specified key.
    /// </summary>
    /// <typeparam name="T">The type parameter.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="key">The key.</param>
    /// <param name="ttl">This would be the TTL parameter, but it's not implemented in this type of cache (memory). Maybe in further version...</param>
    /// <exception cref="OverflowException">The dictionary already contains the maximum number of elements.</exception>
    public void Set<T>(T value, string key, TimeSpan? ttl = null) =>
        _data.AddOrUpdate(key, value, (_, _) => value);

    /// <summary>
    /// Sets the specified value.
    /// </summary>
    /// <typeparam name="T">The type parameter.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="key">The key.</param>
    /// <param name="subKey">The sub key.</param>
    /// <exception cref="OverflowException">The dictionary already contains the maximum number of elements.</exception>
    public void Set<T>(T value, string key, string subKey)
    {
        var finalKey = $"{key}-{subKey}";
        _hash.AddOrUpdate(finalKey, value, (_, _) => value);
    }

    /// <summary>
    /// Gets the object with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of object (the object will be cast to this type).</typeparam>
    /// <param name="key">The key.</param>
    /// <returns>The object as <typeparamref name="T"/>The type parameter.</returns>
    /// <exception cref="InvalidOperationException">Throws when the object with the specified key doesn't exist.</exception>
    public T Get<T>(string key)
    {
        if (!_data.TryGetValue(key, out var value))
        {
            throw new InvalidOperationException($"Unable to get the item with key {key}");
        }

        return (T)value;
    }

    /// <summary>
    /// Gets the specified key.
    /// </summary>
    /// <typeparam name="T">The type parameter.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="subKey">The sub key.</param>
    /// <returns>T.</returns>
    /// <exception cref="InvalidOperationException">Unable to get the item with key {key} and sub key {subKey}.</exception>
    public T Get<T>(string key, string subKey)
    {
        var finalKey = $"{key}-{subKey}";
        if (!_hash.TryGetValue(finalKey, out var value))
        {
            throw new InvalidOperationException(
                $"Unable to get the item with key {key} and sub key {subKey}"
            );
        }

        return (T)value;
    }

    /// <summary>
    /// Tries to get a value based on its key, if exists return true, else false.
    /// The out parameter value is the object requested.
    /// </summary>
    /// <typeparam name="T">The type of object (the object will be cast to this type).</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>Returns <b>True</b> if the object with the key exists, false otherwise.</returns>
    public bool TryGet<T>(string key, out T value)
    {
        value = default;
        if (!_data.TryGetValue(key, out var temp))
        {
            return false;
        }

        value = (T)temp;
        return true;
    }

    /// <summary>
    /// Tries the get.
    /// </summary>
    /// <typeparam name="T">The type parameter.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="subKey">The sub key.</param>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> if able to get the key and sub key, <c>false</c> otherwise.</returns>
    public bool TryGet<T>(string key, string subKey, out T value)
    {
        value = default;
        var finalKey = $"{key}-{subKey}";
        if (!_hash.TryGetValue(finalKey, out var temp))
        {
            return false;
        }

        value = (T)temp;
        return true;
    }

    /// <summary>
    /// Removes the specified key from the cache.
    /// </summary>
    /// <param name="key">The key.</param>
    public void Remove(string key)
    {
        if (_data.ContainsKey(key))
        {
            _data.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Removes the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="subKey">The sub key.</param>
    public void Remove(string key, string subKey)
    {
        var finalKey = $"{key}-{subKey}";
        if (_data.ContainsKey(finalKey))
        {
            _hash.TryRemove(finalKey, out _);
        }
    }

    /// <summary>
    /// Returns the time to live of the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// The timespan until this key is expired from the cache or 0 if it's already expired or doesn't exist.
    /// As Memory Cache does not implement TTL or expire mechanism, this will always return 0, even if the key exists.
    /// </returns>
    public TimeSpan TTL(string key) => new TimeSpan(0);

    /// <summary>
    /// Clears this instance.
    /// </summary>
    public void Clear()
    {
        _data.Clear();
        _hash.Clear();
    }
}
