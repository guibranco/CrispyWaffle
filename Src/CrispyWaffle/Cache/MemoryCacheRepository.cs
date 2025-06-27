using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="OverflowException">The dictionary already contains the maximum number of elements.</exception>
    /// <exception cref="OperationCanceledException">If cancelled.</exception>
    /// <returns>The Value.</returns>
    public ValueTask SetAsync<T>(T value, string key, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_data.Count >= int.MaxValue)
        {
            throw new OverflowException("The dictionary already contains the maximum number of elements.");
        }

        _data.AddOrUpdate(key, value, (_, _) => value);
        return default(ValueTask);
    }

    /// <summary>
    /// Sets the specified value.
    /// </summary>
    /// <typeparam name="T">The type parameter.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="key">The key.</param>
    /// <param name="subKey">The sub key.</param>
    /// <param name="cancellationToken">Cancel.</param>
    /// <exception cref="OverflowException">The dictionary already contains the maximum number of elements.</exception>
    /// <exception cref="OperationCanceledException">If cancelled.</exception>
    /// <returns>ValuTask.</returns>
    public ValueTask SetAsync<T>(T value, string key, string subKey, CancellationToken cancellationToken = default)
    {

        cancellationToken.ThrowIfCancellationRequested();

        var finalKey = $"{key}-{subKey}";
        _hash.AddOrUpdate(finalKey, value, (_, _) => value);
        return default(ValueTask);
    }

    /// <summary>
    /// Gets the object with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of object (the object will be cast to this type).</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="cancellationToken">to cancel.</param>
    /// <returns>The object as <typeparamref name="T"/>The type parameter.</returns>
    /// <exception cref="InvalidOperationException">Throws when the object with the specified key doesn't exist.</exception>
    /// <exception cref="OperationCanceledException">If cancelled.</exception>
    public async ValueTask<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {

        cancellationToken.ThrowIfCancellationRequested();

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
    /// <param name="cancellationToken">to cancel.</param>  
    /// <exception cref="InvalidOperationException">Unable to get the item with key {key} and sub key {subKey}.</exception>
    /// <exception cref="OperationCanceledException">If cancelled.</exception>
    /// <exception cref="InvalidCastException">Invalid cast.</exception>
    /// <returns>T.</returns>
    public ValueTask<T> GetAsync<T>(string key, string subKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var finalKey = $"{key}-{subKey}";
        if (!_hash.TryGetValue(finalKey, out var temp))
        {
            throw new InvalidOperationException(
                $"Unable to get the item with key {key} and sub key {subKey}"
            );
        }

        try
        {
            var value = (T)temp;
            return new ValueTask<T>(value);
        }
        catch (InvalidCastException)
        {
            throw new InvalidCastException($"Unable to convert the item with key {key} and sub key {subKey}");
        }
    }

    /// <summary>
    /// Attempts to retrieve a value from the cache by key.
    /// </summary>
    /// <typeparam name="T">The type of object (the object will be cast to this type).</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="OperationCanceledException">If cancelled.</exception>
    /// <exception cref="InvalidCastException">Invalid cast of value.</exception>
    /// <returns>
    /// A tuple containing Success (true if found and castable to T) and Value (the cached item or default(T)).
    /// </returns>
    public ValueTask<(bool Success, T Value)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_data.TryGetValue(key, out var temp))
        {
            return new ValueTask<(bool Success, T Value)>((Success: false, Value: default(T)));
        }

        try
        {
            var value = (T)temp;
            return new ValueTask<(bool Success, T Value)>((Success: true, Value: value));
        }
        catch (InvalidCastException)
        {
            return new ValueTask<(bool Success, T Value)>((Success: false, Value: default(T)));
        }
    }

    /// <summary>
    /// Attempts to retrieve a value from the cache by key.
    /// </summary>
    /// <typeparam name="T">The type parameter.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="subKey">The sub key.</param>   
    /// <param name="cancellationToken">Cancel token.</param>
    /// <exception cref="OperationCanceledException">If cancelled.</exception>
    /// <exception cref="InvalidCastException">Invalid cast of value.</exception>
    /// <returns>
    /// A tuple containing Success (true if found and castable to T) and Value (the cached item or default(T)).
    /// </returns>
    public ValueTask<(bool Success, T Value)> TryGetAsync<T>(string key, string subKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var finalKey = $"{key}-{subKey}";
        if (!_hash.TryGetValue(finalKey, out var temp))
        {
            return new ValueTask<(bool Success, T Value)>((Success: false, Value: default(T))); ;
        }

        try
        {
            var value = (T)temp;
            return new ValueTask<(bool Success, T Value)>((Success: true, Value: value)); ;
        }
        catch (InvalidCastException)
        {
            return new ValueTask<(bool Success, T Value)>((Success: false, Value: default(T))); ;
        }
    }

    /// <summary>
    /// Removes the specified key from the cache.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="cancellationToken">Cancel token.</param>
    /// <exception cref="OperationCanceledException">If cancelled.</exception>
    /// <returns>Completed Task.</returns>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_data.ContainsKey(key))
        {
            _data.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="subKey">The sub key.</param>
    /// <param name="cancellationToken">Cancel token.</param>
    /// <returns>Completed Task.</returns>
    public Task RemoveAsync(string key, string subKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var finalKey = $"{key}-{subKey}";
        if (_data.ContainsKey(finalKey))
        {
            _hash.TryRemove(finalKey, out _);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns the time to live of the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>
    /// The timespan until this key is expired from the cache or 0 if it's already expired or doesn't exist.
    /// As Memory Cache does not implement TTL or expire mechanism, this will always return 0, even if the key exists.
    /// </returns>
    public Task<TimeSpan> TTLAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TimeSpan.Zero);
    }

    /// <summary>
    /// Clears this instance.
    /// </summary>
    /// <returns>Completed Task.</returns>
    public Task Clear()
    {
        _data.Clear();
        _hash.Clear();
        return Task.CompletedTask;
    }
}
