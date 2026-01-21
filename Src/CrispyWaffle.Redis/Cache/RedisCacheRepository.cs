using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CrispyWaffle.Cache;
using CrispyWaffle.Log;
using CrispyWaffle.Redis.Utils.Communications;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace CrispyWaffle.Redis.Cache;

/// <summary>
/// Class RedisCacheRepository.
/// Implements the <see cref="ICacheRepository" />.
/// Implements the <see cref="IDisposable" />.
/// </summary>
/// <seealso cref="ICacheRepository" />
/// <seealso cref="IDisposable" />
public class RedisCacheRepository : ICacheRepository, IDisposable
{
    /// <summary>
    /// The connector.
    /// </summary>
    private readonly RedisConnector _connector;

    /// <summary>
    /// The cache client.
    /// </summary>
    private readonly IRedisClient _cacheClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisCacheRepository" /> class.
    /// </summary>
    /// <param name="connector">The connector.</param>
    public RedisCacheRepository(RedisConnector connector)
    {
        _connector = connector;
        _cacheClient = connector.Cache;
    }

    /// <summary>
    /// Handles the exception.
    /// </summary>
    /// <param name="e">The e.</param>
    private static void HandleException(Exception e)
    {
        if (e.Message.IndexOf("timeout", StringComparison.InvariantCultureIgnoreCase) != -1)
        {
            LogConsumer.Trace(e);
        }
        else
        {
            LogConsumer.Handle(e);
        }
    }

    /// <summary>
    /// Sets to database.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="key">The key.</param>
    /// <param name="databaseNumber">The database number.</param>
    /// <param name="ttl">The TTL.</param>
    /// <param name="fireAndForget">if set to <c>true</c> [fire and forget].</param>
    public void SetToDatabase<T>(
        T value,
        string key,
        int databaseNumber,
        TimeSpan? ttl = null,
        bool fireAndForget = false
    )
    {
        var flags = CommandFlags.None;
        if (fireAndForget)
        {
            flags = CommandFlags.FireAndForget;
        }

        var inputBytes = _connector.Serializer.Serialize(value);
        _connector.GetDatabase(databaseNumber).StringSet(key, inputBytes, ttl, When.Always, flags);
    }

    /// <summary>
    /// Gets from database.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="databaseNumber">The database number.</param>
    /// <returns>T.</returns>
    /// <exception cref="System.InvalidOperationException">Invalid operation.</exception>
    public T GetFromDatabase<T>(string key, int databaseNumber)
    {
        var valueBytes = _connector
            .GetDatabase(databaseNumber)
            .StringGet(key, CommandFlags.PreferReplica);
        if (!valueBytes.HasValue)
        {
            throw new InvalidOperationException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    "Unable to get the item with key {0}",
                    key
                )
            );
        }

        return _connector.Serializer.Deserialize<T>(valueBytes);
    }

    /// <summary>
    /// Tries the get from database.
    /// </summary>
    /// <typeparam name="T">The type of the document.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="databaseNumber">The database number.</param>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> if get from database, <c>false</c> otherwise.</returns>
    public bool TryGetFromDatabase<T>(string key, int databaseNumber, out T value)
    {
        value = default;
        var valueBytes = _connector
            .GetDatabase(databaseNumber)
            .StringGet(key, CommandFlags.PreferReplica);
        if (!valueBytes.HasValue)
        {
            return false;
        }

        value = _connector.Serializer.Deserialize<T>(valueBytes);
        return true;
    }

    /// <summary>
    /// Removes from database.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="databaseNumber">The database number.</param>
    public void RemoveFromDatabase(string key, int databaseNumber) =>
        _connector.GetDatabase(databaseNumber).KeyDelete(key);

    /// <summary>
    /// Gets or sets a value indicating whether [should propagate exceptions].
    /// </summary>
    /// <value><c>true</c> if [should propagate exceptions]; otherwise, <c>false</c>.</value>
    public bool ShouldPropagateExceptions { get; set; }


    /// <summary>
    /// Sets the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="key">The key.</param>
    /// <param name="ttl">The TTL.</param>
    /// <param name="cancellationToken">To cancel operation.</param>
    /// <returns>A valuetask representing the asynchronous operation.</returns>
    public async ValueTask SetAsync<T>(T value, string key, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Throw if cancellation is requested before starting
            cancellationToken.ThrowIfCancellationRequested();

            Task cacheOperation;

            if (ttl.HasValue)
            {
                cacheOperation = _cacheClient.Db0.AddAsync(key, value, ttl.Value);
            }
            else
            {
                cacheOperation = _cacheClient.Db0.AddAsync(key, value);
            }

            // If your cache client doesn't support cancellation tokens directly,
            // you can use Task.WhenAny with a cancellation task
            if (!cancellationToken.CanBeCanceled)
            {
                await cacheOperation.ConfigureAwait(false);
            }
            else
            {
                var cancellationTask = Task.Delay(Timeout.Infinite, cancellationToken);
                var completedTask = await Task.WhenAny(cacheOperation, cancellationTask).ConfigureAwait(false);

                if (completedTask == cancellationTask)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                await cacheOperation.ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Re-throw cancellation exceptions
            throw;
        }
        catch (Exception e)
        {
            if (ShouldPropagateExceptions)
            {
                throw;
            }

            HandleException(e);
        }
    }

    /// <summary>
    /// Sets the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the document.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="key">The key.</param>
    /// <param name="subKey">The sub key.</param>
    /// <param name="cancellationToken">To cancel operation.</param>
    /// /// <returns>
    /// A ValueTask that represents the asynchronous hash field set operation. The task completes when:
    /// - The hash field has been successfully updated in Redis, OR
    /// - An exception occurs during the cache operation, OR
    /// - The operation is cancelled via the cancellation token
    /// The returned ValueTask has no result value (void equivalent for async operations).
    ///
    /// Completion scenarios:
    /// • Success: Task completes successfully when hash field is updated
    /// • Cancellation: Task throws OperationCanceledException if cancelled before or during execution
    /// • Cache errors: Task may complete silently or throw exception based on ShouldPropagateExceptions setting
    /// • Network errors: Task may complete silently or throw exception based on ShouldPropagateExceptions setting
    ///
    /// The ValueTask should always be awaited or its result checked to ensure proper exception handling.
    /// </returns>
    public async ValueTask SetAsync<T>(T value, string key, string subKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        }

        if (string.IsNullOrEmpty(subKey))
        {
            throw new ArgumentException("SubKey cannot be null or empty", nameof(subKey));
        }

        if (EqualityComparer<T>.Default.Equals(value, default(T)) && !typeof(T).IsValueType)
        {
            throw new ArgumentNullException(nameof(value));
        }

        try
        {
            // Early cancellation check
            cancellationToken.ThrowIfCancellationRequested();

            // Check if the hash key exists
            var keyExists = await _cacheClient.Db0.ExistsAsync(key).ConfigureAwait(false);

            // Get existing hash values if key exists, otherwise create new dictionary
            IDictionary<string, T> allValues;
            if (keyExists)
            {
                // Check for cancellation before expensive operation
                cancellationToken.ThrowIfCancellationRequested();

                allValues = await _cacheClient.Db0.HashGetAllAsync<T>(key, CommandFlags.PreferReplica).ConfigureAwait(false);
            }
            else
            {
                allValues = new Dictionary<string, T>();
            }

            // Update the specific sub-key with the new value
            allValues[subKey] = value;

            // Check for cancellation before final write operation
            cancellationToken.ThrowIfCancellationRequested();

            // Save the updated hash back to Redis
            await _cacheClient.Db0.HashSetAsync(key, allValues, CommandFlags.FireAndForget).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Re-throw cancellation exceptions to preserve cancellation semantics
            throw;
        }
        catch (ArgumentException)
        {
            // Re-throw argument validation exceptions
            throw;
        }
        catch (Exception e)
        {
            // Handle cache/network exceptions according to configuration
            if (ShouldPropagateExceptions)
            {
                throw;
            }

            HandleException(e);
        }
    }

    /// <summary>
    /// Gets the specified key.
    /// </summary>
    /// <typeparam name="T">The type of object (the object will be cast to this type).</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests. <c>default</c> if no cancellation is required;
    /// otherwise, a <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <exception cref="ArgumentException">Thrown when key is null or empty.</exception>
    /// <exception cref="InvalidOperationException">"Unable to get the item with key.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    /// <returns>
    /// A ValueTask that represents the asynchronous cache get operation. The task result contains:
    /// - The cached value of type T if the key exists in cache
    ///
    /// The returned ValueTask will complete with the cached value when:
    /// • Success: Key exists and value is successfully retrieved from cache
    /// • Exception: Key doesn't exist in cache (throws InvalidOperationException)
    /// • Exception: Cache operation fails and ShouldPropagateExceptions is true
    /// • Exception: Cache operation fails and ShouldPropagateExceptions is false (throws InvalidOperationException after logging)
    /// • Cancellation: Operation is cancelled via the cancellation token (throws OperationCanceledException)
    ///
    /// The ValueTask should always be awaited to ensure proper exception handling.
    /// </returns>
    public async ValueTask<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        }

        try
        {
            // Early cancellation check
            cancellationToken.ThrowIfCancellationRequested();

            // Check if the key exists in cache
            var keyExists = await _cacheClient.Db0.ExistsAsync(key).ConfigureAwait(false);

            if (keyExists)
            {
                // Check for cancellation before expensive get operation
                cancellationToken.ThrowIfCancellationRequested();

                // Retrieve the value from cache
                var value = await _cacheClient.Db0.GetAsync<T>(key, CommandFlags.PreferReplica).ConfigureAwait(false);
                return value;
            }
        }
        catch (OperationCanceledException)
        {
            // Re-throw cancellation exceptions to preserve cancellation semantics
            throw;
        }
        catch (ArgumentException)
        {
            // Re-throw argument validation exceptions
            throw;
        }
        catch (Exception e)
        {
            // Handle cache/network exceptions according to configuration
            if (ShouldPropagateExceptions)
            {
                throw;
            }

            HandleException(e);
        }

        // Key doesn't exist or operation failed with exception suppression
        throw new InvalidOperationException(
            string.Format(CultureInfo.CurrentCulture, "Unable to get the item with key {0}", key)
        );
    }

    /// <summary>
    /// Gets the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the document.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="subKey">The sub key.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests. <c>default</c> if no cancellation is required;
    /// otherwise, a <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>
    /// A ValueTask that represents the asynchronous hash field get operation. The task result contains:
    /// - The cached value of type T if the hash field exists in cache
    ///
    /// The returned ValueTask will complete with the cached value when:
    /// • Success: Hash field exists and value is successfully retrieved from cache
    /// • Exception: Hash field doesn't exist in cache (throws InvalidOperationException)
    /// • Exception: Cache operation fails and ShouldPropagateExceptions is true
    /// • Exception: Cache operation fails and ShouldPropagateExceptions is false (throws InvalidOperationException after logging)
    /// • Cancellation: Operation is cancelled via the cancellation token (throws OperationCanceledException)
    ///
    /// The ValueTask should always be awaited to ensure proper exception handling.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when key or subKey is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Unable to get the item with key and sub key.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    ///
    public async ValueTask<T> GetAsync<T>(string key, string subKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        }

        try
        {
            // Early cancellation check
            cancellationToken.ThrowIfCancellationRequested();

            // Check if the key exists in cache
            var keyExists = await _cacheClient.Db0.ExistsAsync(key).ConfigureAwait(false);

            if (keyExists)
            {
                // Check for cancellation before expensive get operation
                cancellationToken.ThrowIfCancellationRequested();

                // Retrieve the value from cache
                return await _cacheClient.Db0.GetAsync<T>(key, CommandFlags.PreferReplica).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Re-throw cancellation exceptions to preserve cancellation semantics
            throw;
        }
        catch (ArgumentException)
        {
            // Re-throw argument validation exceptions
            throw;
        }
        catch (Exception e)
        {
            // Handle cache/network exceptions according to configuration
            if (ShouldPropagateExceptions)
            {
                throw;
            }

            HandleException(e);
        }

        // Key doesn't exist or operation failed with exception suppression
        throw new InvalidOperationException(
            string.Format(CultureInfo.CurrentCulture, "Unable to get the item with key {0}", key)
        );
    }

    /// <summary>
    /// Tries to get a value based on its key, if exists return true, else false.
    /// The out parameter value is the object requested.
    /// </summary>
    /// <typeparam name="T">The type of object (the object will be cast to this type).</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests. <c>default</c> if no cancellation is required; 
    /// otherwise, a <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>
    /// A ValueTask that represents the asynchronous try-get operation. The task result contains:
    /// - A touple with Success=true and the cached value if the key exists
    /// - A touple with Success=false and default(T) if the key doesn't exist or operation fails
    ///
    /// The returned ValueTask will complete when:
    /// • Success: Key exists and value is successfully retrieved from cache
    /// • Success: Key doesn't exist (returns false with default value)
    /// • Exception: Cache operation fails and ShouldPropagateExceptions is true
    /// • Exception: Operation is cancelled via the cancellation token (throws OperationCanceledException)
    ///
    /// When ShouldPropagateExceptions is false, cache errors are logged and the method returns false.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when key is null or empty.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    public async ValueTask<(bool Success, T Value)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        }

        try
        {
            // Early cancellation check
            cancellationToken.ThrowIfCancellationRequested();

            // More efficient: Check existence first, then get value if it exists
            var keyExists = await _cacheClient.Db0.ExistsAsync(key).ConfigureAwait(false);

            if (keyExists)
            {
                // Check for cancellation before expensive get operation
                cancellationToken.ThrowIfCancellationRequested();

                // Retrieve the value from cache
                var value = await _cacheClient.Db0.GetAsync<T>(key, CommandFlags.PreferReplica).ConfigureAwait(false);
                return (Success: true, Value: value);
            }

            // Key doesn't exist
            return (Success: false, Value: default(T));
        }
        catch (OperationCanceledException)
        {
            // Re-throw cancellation exceptions to preserve cancellation semantics
            throw;
        }
        catch (ArgumentException)
        {
            // Re-throw argument validation exceptions
            throw;
        }
        catch (Exception e)
        {
            // Handle cache/network exceptions according to configuration
            if (ShouldPropagateExceptions)
            {
                throw;
            }

            HandleException(e);

            // Return failed result when exceptions are suppressed
            return (Success: false, Value: default(T));
        }
    }

    /// <summary>
    /// Asynchronously attempts to get a value from the cache using the specified key.
    /// This method returns both the success status and the value in a single result.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve from cache</typeparam>
    /// <param name="key">The cache key used to identify the cached value. Cannot be null or empty.</param>
    /// <param name="subKey">The subkey.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests. <c>default</c> if no cancellation is required;
    /// otherwise, a <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>
    /// A ValueTask that represents the asynchronous try-get operation. The task result contains:
    /// - A touple with Success=true and the cached value if the key exists
    /// - A touple with Success=false and default(T) if the key doesn't exist or operation fails
    /// 
    /// The returned ValueTask will complete when:
    /// • Success: Key exists and value is successfully retrieved from cache
    /// • Success: Key doesn't exist (returns false with default value)
    /// • Exception: Cache operation fails and ShouldPropagateExceptions is true
    /// • Exception: Operation is cancelled via the cancellation token (throws OperationCanceledException)
    /// 
    /// When ShouldPropagateExceptions is false, cache errors are logged and the method returns false.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when key is null or empty.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    public async ValueTask<(bool Success, T Value)> TryGetAsync<T>(string key, string subKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        }

        try
        {
            // Early cancellation check
            cancellationToken.ThrowIfCancellationRequested();

            // More efficient: Check existence first, then get value if it exists
            var hashFieldExists = await _cacheClient.Db0.HashExistsAsync(key, subKey).ConfigureAwait(false);

            if (hashFieldExists)
            {
                // Check for cancellation before expensive get operation
                cancellationToken.ThrowIfCancellationRequested();

                // Retrieve the value from cache
                var value = await _cacheClient.Db0.HashGetAsync<T>(key, subKey, CommandFlags.PreferReplica).ConfigureAwait(false);
                return (Success: true, Value: value);
            }

            // Key doesn't exist
            return (Success: false, Value: default(T));
        }
        catch (OperationCanceledException)
        {
            // Re-throw cancellation exceptions to preserve cancellation semantics
            throw;
        }
        catch (ArgumentException)
        {
            // Re-throw argument validation exceptions
            throw;
        }
        catch (Exception e)
        {
            // Handle cache/network exceptions according to configuration
            if (ShouldPropagateExceptions)
            {
                throw;
            }

            HandleException(e);

            // Return failed result when exceptions are suppressed
            return (Success: false, Value: default(T));
        }
    }

    /// <summary>
    /// Removes the specified key from the cache.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests. <c>default</c> if no cancellation is required;
    /// otherwise, a <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A ValueTask that represents the asynchronous call.
    /// </returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _cacheClient.Db0.RemoveAsync(key, CommandFlags.FireAndForget).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Re-throw cancellation exceptions to preserve cancellation semantics
            throw;
        }
        catch (Exception e)
        {
            if (ShouldPropagateExceptions)
            {
                throw;
            }

            HandleException(e);
        }
    }

    /// <summary>
    /// Removes the specified key from the cache.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="subKey">The subkey.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests. <c>default</c> if no cancellation is required;
    /// otherwise, a <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A ValueTask that represents the asynchronous call.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    public async Task RemoveAsync(string key, string subKey, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cacheClient.Db0.HashDeleteAsync(key, subKey, CommandFlags.FireAndForget).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Re-throw cancellation exceptions to preserve cancellation semantics
            throw;
        }
        catch (Exception e)
        {
            if (ShouldPropagateExceptions)
            {
                throw;
            }

            HandleException(e);
        }
    }

    /// <summary>
    /// Returns the time to live of the specified key.
    /// </summary>
    /// <param name="key">The key.</param> 
    /// <param name="cancellationToken">Tp cancel operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <returns>The timespan until this key is expired from the cache or 0 if it's already expired or doesn't exists.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>    
    public async Task<TimeSpan> TTLAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var ttl = await _cacheClient.Db0.Database.KeyTimeToLiveAsync(key, CommandFlags.PreferReplica);
            return ttl ?? TimeSpan.Zero;
        }
        catch (OperationCanceledException)
        {
            // Re-throw cancellation exceptions to preserve cancellation semantics
            throw;
        }
        catch (Exception e)
        {
            if (ShouldPropagateExceptions)
            {
                throw;
            }

            HandleException(e);
        }

        return TimeSpan.Zero;
    }

    /// <summary>
    /// Clears this instance.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Clear()
    {
        try
        {
           await _cacheClient.Db0.FlushDbAsync();
        }
        catch (Exception e)
        {
            if (ShouldPropagateExceptions)
            {
                throw;
            }

            HandleException(e);
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _connector.Dispose();
        GC.SuppressFinalize(this);
    }
}
