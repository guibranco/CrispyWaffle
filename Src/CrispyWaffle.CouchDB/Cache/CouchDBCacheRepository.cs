using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CouchDB.Driver;
using CouchDB.Driver.Extensions;
using CrispyWaffle.Cache;
using CrispyWaffle.CouchDB.Utils.Communications;
using CrispyWaffle.Log;

namespace CrispyWaffle.CouchDB.Cache;

/// <summary>
/// Class CouchDBCacheRepository.
/// </summary>
public class CouchDBCacheRepository : ICacheRepository, IDisposable
{
    /// <summary>
    /// The connector.
    /// </summary>
    private readonly CouchDBConnector _connector;

    /// <summary>
    /// Gets or sets a value indicating whether [should propagate exceptions].
    /// </summary>
    /// <value><c>true</c> if [should propagate exceptions]; otherwise, <c>false</c>.</value>
    public bool ShouldPropagateExceptions { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CouchDBCacheRepository"/> class.
    /// </summary>
    /// <param name="connector">The connector.</param>
    public CouchDBCacheRepository(CouchDBConnector connector) => _connector = connector;

    /// <summary>
    /// Retrieves the count of documents of type <typeparamref name="T"/> in the CouchDB database.
    /// </summary>
    /// <typeparam name="T">The type of the CouchDB document for which the count is to be retrieved. It must inherit from <see cref="CouchDBCacheDocument"/>.</typeparam>
    /// <returns>The number of documents of type <typeparamref name="T"/> that have a non-null Id in the database.</returns>
    /// <remarks>
    /// This method first resolves the database for the specified document type <typeparamref name="T"/>.
    /// It then filters the documents to include only those with a non-null Id, ensuring that only valid documents are counted.
    /// Finally, it converts the filtered results to a list and returns the count of those documents.
    /// This is useful for determining how many valid instances of a specific document type exist in the database.
    /// </remarks>
    public async Task<int> GetDocCount<T>()
        where T : CouchDBCacheDocument
    {
        var db = await ResolveDatabase<T>().ConfigureAwait(false);
        return db.Where(x => x.Id != null).ToList().Count;
    }

    /// <summary>
    /// Clears all documents from the cache database.
    /// </summary>
    /// <returns>A task representing the asynchronous clear operation.</returns>
    /// <exception cref="Exception">Thrown if ShouldPropagateExceptions is true and an error occurs during deletion.</exception>
    public async Task Clear() => await Clear<CouchDBCacheDocument>();

    /// <summary>
    /// Clears all documents of the specified type from the cache database.
    /// </summary>
    /// <typeparam name="T">The document type to clear, must inherit from CouchDBCacheDocument.</typeparam>
    /// <returns>A task representing the asynchronous clear operation.</returns>
    /// <exception cref="Exception">Thrown if ShouldPropagateExceptions is true and an error occurs during deletion.</exception>
    public async Task Clear<T>()
        where T : CouchDBCacheDocument
    {
        try
        {
            var db = await ResolveDatabase<T>().ConfigureAwait(false);
            var docs = db.Where(x => x.Id != null).ToList();
            var tasks = new List<Task>(docs.Count);

            foreach (var doc in docs)
            {
                tasks.Add(db.DeleteAsync(doc));
            }

            Task.WaitAll(tasks.ToArray());
        }
        catch (Exception e)
        {
            if (ShouldPropagateExceptions)
            {
                throw;
            }

            LogConsumer.Handle(e);
        }
    }

    /// <inheritdoc />
    public async ValueTask<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!typeof(CouchDBCacheDocument).IsAssignableFrom(typeof(T)))
        {
            return default;
        }

        var result = await GetSpecificAsync<CouchDBCacheDocument>(key);

        return (T)(object)result;
    }

    /// <summary>
    /// Retrieves a cached document of the specified type from the cache using the provided keys.
    /// </summary>
    /// <typeparam name="T">The type of the cached document to retrieve.</typeparam>
    /// <param name="key">The primary key used to identify the cached document.</param>
    /// <param name="subKey">The secondary key used to further specify the cached document.</param>
    /// <returns>
    /// An instance of type <typeparamref name="T"/> if the type is assignable from <see cref="CouchDBCacheDocument"/>; otherwise, returns the default value for type <typeparamref name="T"/>.
    /// </returns>
    /// <remarks>
    /// This method checks if the specified type <typeparamref name="T"/> is assignable from <see cref="CouchDBCacheDocument"/>.
    /// If it is, it calls the method <see cref="GetSpecificAsync{CouchDBCacheDocument}"/> to retrieve the cached document associated with the provided keys.
    /// If the type is not assignable, it returns the default value for that type, which could be null for reference types or zero for numeric types.
    /// This method is useful for retrieving cached data in a type-safe manner, ensuring that only compatible types are processed.
    /// </remarks>
    public async ValueTask<T> GetAsync<T>(string key, string subKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!typeof(CouchDBCacheDocument).IsAssignableFrom(typeof(T)))
        {
            return default;
        }

        var result = await GetSpecificAsync<CouchDBCacheDocument>(key, subKey, cancellationToken);

        return (T)(object)result;
    }

    /// <summary>
    /// Gets from a class specified database instead of the general <see cref="CouchDBCacheDocument"/> database.
    /// </summary>
    /// <typeparam name="T">Type T with base type <see cref="CouchDBCacheDocument"/>.</typeparam>
    /// <param name="key">A uniquely identifiable key to get document from the specified database.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The document if found.</returns>
    /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled.</exception>
    /// <exception cref="InvalidOperationException">Thrown in case the operation fails.</exception>
    public async Task<T> GetSpecificAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : CouchDBCacheDocument
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var clientDb = await ResolveDatabase<T>().ConfigureAwait(false);

            var doc = await clientDb.FindAsync(key);
            if (doc != default && doc.ExpiresAt != default && doc.ExpiresAt <= DateTime.UtcNow)
            {
                await RemoveSpecificAsync<T>(key, cancellationToken).ConfigureAwait(false);
                return default;
            }

            return doc;
        }
        catch (OperationCanceledException)
        {
            LogConsumer.Warning($"Operation cancelled while getting document with key: {key}");
            throw;
        }
        catch (Exception e)
        {
            if (ShouldPropagateExceptions)
            {
                throw;
            }

            LogConsumer.Handle(e);
        }

        throw new InvalidOperationException(
            $"Unable to get the item with key: {key}"
        );
    }

    /// <summary>
    /// Retrieves a specific cached document by key and subkey, removing it if expired.
    /// </summary>
    /// <typeparam name="T">The document type to retrieve, must inherit from CouchDBCacheDocument.</typeparam>
    /// <param name="key">The primary cache key.</param>
    /// <param name="subKey">The secondary cache key.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The cached document if found and not expired; otherwise null.</returns>
    /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled.</exception>
    /// <exception cref="Exception">Thrown if ShouldPropagateExceptions is true and an error occurs.</exception>
    public async Task<T> GetSpecificAsync<T>(string key, string subKey, CancellationToken cancellationToken = default)
        where T : CouchDBCacheDocument
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var client = await ResolveDatabase<T>().ConfigureAwait(false);
            var doc = client
                .Where(x => x.Key == key && x.SubKey == subKey)
                .FirstOrDefault();

            if (doc != default && doc.ExpiresAt != default && doc.ExpiresAt <= DateTime.UtcNow)
            {
                await RemoveSpecificAsync<T>(key, subKey, cancellationToken).ConfigureAwait(false);
                return default;
            }

            return doc;
        }
        catch (OperationCanceledException)
        {
            LogConsumer.Warning($"Operation cancelled while getting document with key and subkey: {key} {subKey}");
            throw;
        }
        catch (Exception e)
        {
            if (ShouldPropagateExceptions)
            {
                throw;
            }

            LogConsumer.Handle(e);
        }

        throw new InvalidOperationException(
            $"Unable to get the item with key: {key} and sub key: {subKey}"
        );
    }

    /// <inheritdoc />
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <param name="key">Key to be removed.</param>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await RemoveSpecificAsync<CouchDBCacheDocument>(key, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes a specific entry from the cache based on the provided key and subKey.
    /// </summary>
    /// <param name="key">The primary key associated with the entry to be removed.</param>
    /// <param name="subKey">The secondary key associated with the entry to be removed.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <remarks>
    /// This method calls the generic method <c>RemoveSpecific</c> with the type <c>CouchDBCacheDocument</c> to remove the entry
    /// identified by the combination of <paramref name="key"/> and <paramref name="subKey"/>.
    /// It is important to ensure that both keys are correctly specified to successfully remove the intended entry from the cache.
    /// If the specified entry does not exist, no action will be taken, and no exceptions will be thrown.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task RemoveAsync(string key, string subKey, CancellationToken cancellationToken = default)
    {
        await RemoveSpecificAsync<CouchDBCacheDocument>(key, subKey, cancellationToken).ConfigureAwait(false);
    }


    /// <summary>
    /// Removes from a class specified database instead of the general <see cref="CouchDBCacheDocument"/> database.
    /// </summary>
    /// <typeparam name="T">Type T with base type <see cref="CouchDBCacheDocument"/>.</typeparam>
    /// <param name="key">A uniquely identifiable key to remove document from the specified database.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RemoveSpecificAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : CouchDBCacheDocument
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be null, empty, or whitespace.", nameof(key));
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Using synchronous database resolution if async version not available
            var db = await ResolveDatabase<T>().ConfigureAwait(false);
            var doc = await db.FindAsync(key).ConfigureAwait(false);

            if (doc != null)
            {
                await db.DeleteAsync(doc).ConfigureAwait(false);
                LogConsumer.Info($"Successfully removed document with key: {key}");
            }
            else
            {
                LogConsumer.Info($"Document with key: {key} not found for removal");
            }
        }
        catch (OperationCanceledException)
        {
            LogConsumer.Warning($"Operation cancelled while removing document with key: {key}");
            throw;
        }
        catch (Exception e)
        {
            LogConsumer.Error($"Failed to remove document with key: {key}", e);

            if (ShouldPropagateExceptions)
            {
                throw;
            }

            LogConsumer.Handle(e);
        }
    }

    /// <summary>
    /// Removes a specific document from the CouchDB database based on the provided key and subKey.
    /// </summary>
    /// <typeparam name="T">The type of the CouchDB document, which must inherit from <see cref="CouchDBCacheDocument"/>.</typeparam>
    /// <param name="key">The key of the document to be removed.</param>
    /// <param name="subKey">The subKey of the document to be removed.</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <remarks>
    /// This method retrieves a document from the CouchDB database that matches the specified <paramref name="key"/>
    /// and <paramref name="subKey"/>. If a matching document is found, it is deleted asynchronously from the database.
    /// If an exception occurs during this process, it is either propagated or handled based on the value of
    /// <see cref="ShouldPropagateExceptions"/>. If exceptions are not propagated, they are logged using the
    /// <see cref="LogConsumer"/>. This method does not return any value.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when key is null, empty, or whitespace.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    public async Task RemoveSpecificAsync<T>(string key, string subKey, CancellationToken cancellationToken = default)
        where T : CouchDBCacheDocument
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be null, empty, or whitespace.", nameof(key));
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
 
            var db = await ResolveDatabase<T>().ConfigureAwait(false);
            var doc = db.Where(x => x.Key == key && x.SubKey == subKey).FirstOrDefault();

            if (doc != null)
            {
                await db.DeleteAsync(doc).ConfigureAwait(false);
                LogConsumer.Info($"Successfully removed document with key: {key}");
            }
            else
            {
                LogConsumer.Info($"Document with key: {key} not found for removal");
            }
        }
        catch (OperationCanceledException)
        {
            LogConsumer.Warning($"Operation cancelled while removing document with key: {key}");
            throw;
        }
        catch (Exception e)
        {
            LogConsumer.Error($"Failed to remove document with key: {key}", e);

            if (ShouldPropagateExceptions)
            {
                throw;
            }

            LogConsumer.Handle(e);
        }
    }

    /// <inheritdoc />
    public async ValueTask SetAsync<T>(T value, string key, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        if (!typeof(CouchDBCacheDocument).IsAssignableFrom(typeof(T)))
        {
            return;
        }

        await SetSpecificAsync((CouchDBCacheDocument)(object)value, key, ttl, cancellationToken);
    }

    /// <summary>
    /// Sets a value in the CouchDB cache using the specified key and sub-key.
    /// </summary>
    /// <typeparam name="T">The type of the value to be set, which must be a subclass of <see cref="CouchDBCacheDocument"/>.</typeparam>
    /// <param name="value">The value to be set in the cache.</param>
    /// <param name="key">The primary key under which the value is stored.</param>
    /// <param name="subKey">The sub-key under which the value is stored.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <remarks>
    /// This method checks if the provided type <typeparamref name="T"/> is assignable from <see cref="CouchDBCacheDocument"/>.
    /// If it is not, the method returns without performing any action.
    /// If the type is valid, it calls the <see cref="SetSpecific"/> method to set the value in the cache.
    /// This allows for type-safe caching of documents that inherit from <see cref="CouchDBCacheDocument"/>.
    /// </remarks>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    public async ValueTask SetAsync<T>(T value, string key, string subKey, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!typeof(CouchDBCacheDocument).IsAssignableFrom(typeof(T)))
            {
                return;
            }

            await SetSpecificAsync((CouchDBCacheDocument)(object)value, key, subKey, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Always propagate cancellation
            LogConsumer.Warning($"Operation cancelled while setting document with key: {key}, subKey: {subKey}");
            throw;
        }
        catch (Exception ex)
        {
            // Log and wrap with context
            LogConsumer.Error($"Failed to set cache item with key: '{key}', subKey: '{subKey}', type: {typeof(T).Name}", ex);

            if (ShouldPropagateExceptions)
            {
                throw;
            }
        }
    }

    /// <summary>
    /// Persists to a class specified database instead of the general <see cref="CouchDBCacheDocument"/> database.
    /// </summary>
    /// <typeparam name="T">Type T with base type <see cref="CouchDBCacheDocument"/>.</typeparam>
    /// <param name="value">The value of type T to be persisted.</param>
    /// <param name="key">A uniquely identifiable key to remove document from the specified database.</param>
    /// <param name="ttl">How long the value should be stored.</param>
    /// <returns>A ValueTask representing the asynchronous operation</returns>
    public async Task SetSpecificAsync<T>(T value, string key, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
        where T : CouchDBCacheDocument
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            value.Key = key;

            if (ttl != null)
            {
                value.TTL = ttl.Value;
                value.ExpiresAt = DateTime.UtcNow.Add(ttl.Value);
            }

            var clientDB = await ResolveDatabase<T>().ConfigureAwait(false);
            await clientDB.CreateAsync(value);
        }
        catch (Exception e)
        {
            if (ShouldPropagateExceptions)
            {
                throw;
            }

            LogConsumer.Handle(e);
        }
    }

    /// <summary>
    /// Sets the specific key and subKey for a CouchDBCacheDocument and updates the database asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the CouchDBCacheDocument.</typeparam>
    /// <param name="value">The CouchDBCacheDocument instance to be updated.</param>
    /// <param name="key">The key to be set for the CouchDBCacheDocument.</param>
    /// <param name="subKey">The subKey to be set for the CouchDBCacheDocument.</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <remarks>
    /// This method assigns the provided <paramref name="key"/> and <paramref name="subKey"/> to the specified
    /// <paramref name="value"/> of type <typeparamref name="T"/>. It then attempts to create or update the
    /// document in the database asynchronously using the ResolveDatabase method. If an exception occurs during
    /// this process, it checks whether exceptions should be propagated. If propagation is enabled, the exception
    /// is rethrown; otherwise, it is logged using the LogConsumer.
    /// </remarks>
    /// <exception cref="Exception">Thrown when an error occurs during the database operation, unless exceptions are suppressed.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled</exception>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task SetSpecificAsync<T>(T value, string key, string subKey, CancellationToken cancellationToken = default)
        where T : CouchDBCacheDocument
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            value.Key = key.Trim();
            value.SubKey = subKey.Trim();

            var clientDB = await ResolveDatabase<T>().ConfigureAwait(false);
            await clientDB.CreateOrUpdateAsync(value).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Always propagate cancellation
            LogConsumer.Warning($"Operation cancelled while setting document with key: {key}, subKey: {subKey}");
            throw;
        }
        catch (Exception e)
        {
            if (ShouldPropagateExceptions)
            {
                throw;
            }

            LogConsumer.Handle(e);
        }
    }

    /// <inheritdoc />
    public async ValueTask<(bool Success, T Value)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var get = await GetAsync<CouchDBCacheDocument>(key, cancellationToken);

            if (get != default)
            {
                var value = (T)(object)get;
                return (true, value);
            }
        }
        catch (OperationCanceledException)
        {
            // Always propagate cancellation
            LogConsumer.Warning($"Operation cancelled while setting document with key: {key}");
            return (false, default(T));
        }
        catch (Exception e)
        {
            if (ShouldPropagateExceptions)
            {
                throw;
            }

            LogConsumer.Handle(e);
        }

        return (false, default(T));
    }

    /// <summary>
    /// Attempts to retrieve a cached document by key and subkey.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The primary key used to access the cached value.</param>
    /// <param name="subKey">The secondary key used to access the cached value.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A tuple containing Success (true if found and retrieved) and Value (the document cast to T or default(T)).
    /// </returns>
    /// <remarks>
    /// This method attempts to fetch a cached document from a data store using a combination of a primary key and a secondary key.
    /// If the document is found, it is cast to the specified type <typeparamref name="T"/> and returned./>.
    /// This is useful for safely attempting to retrieve values without throwing exceptions if the keys do not exist in the cache.
    /// </remarks>
    /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled.</exception>
    /// <exception cref="Exception">Thrown if ShouldPropagateExceptions is true and an error occurs.</exception>
    public async ValueTask<(bool Success, T Value)> TryGetAsync<T>(string key, string subKey, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var get = await GetAsync<CouchDBCacheDocument>(key, subKey, cancellationToken);
            if (get != default)
            {
                var value = (T)(object)get;
                return (true, value);
            }
        }
        catch (OperationCanceledException)
        {
            // Always propagate cancellation
            LogConsumer.Warning($"Operation cancelled while setting document with key and subkey: {key} {subKey}");
            throw;
        }
        catch (Exception e)
        {
            if (ShouldPropagateExceptions)
            {
                throw;
            }

            LogConsumer.Handle(e);
        }

        return (false, default(T));
    }

    /// <summary>
    /// Retrieves the Time-To-Live (TTL) value for a specified cache key.
    /// </summary>
    /// <param name="key">The key for which the TTL value is to be retrieved.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The TTL of the cached document, or TimeSpan.Zero if not found or no TTL set</returns>
    /// <remarks>
    /// This method accesses a CouchDB document associated with the provided <paramref name="key"/>
    /// and retrieves its Time-To-Live (TTL) value. The TTL indicates the duration for which the
    /// cached item is considered valid. If the TTL has expired, the item may be removed from the
    /// cache, and subsequent requests for this key may result in a cache miss.
    /// This method assumes that the key exists in the cache; if it does not, the behavior will depend
    /// on the implementation of the Get method.
    /// </remarks>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled</exception>
    public async Task<TimeSpan> TTLAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var document = await GetAsync<CouchDBCacheDocument>(key, cancellationToken).ConfigureAwait(false);

            return document?.TTL ?? TimeSpan.Zero;
        }
        catch (OperationCanceledException)
        {
            // Always propagate cancellation
            LogConsumer.Warning($"Operation cancelled while setting document with key: {key}");
            throw;
        }
        catch (Exception ex)
        {
            LogConsumer.Error($"Failed to get TTL for key: '{key}'", ex);

            if (ShouldPropagateExceptions)
            {
                throw;
            }

            // Return default value on error
            return TimeSpan.Zero;
        }
    }

    /// <summary>
    /// Resolves a CouchDatabase instance for the specified database name or defaults to the type name if none is provided.
    /// </summary>
    /// <typeparam name="T">The type of the CouchDBCacheDocument that the database will contain.</typeparam>
    /// <param name="dbName">The name of the database to resolve. If not provided, defaults to the lowercase name of the type <typeparamref name="T"/> followed by 's'.</param>
    /// <returns>A CouchDatabase instance of type <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// This method checks if the specified database name exists in the CouchDB instance. If the database does not exist, it creates a new database with the given name.
    /// If the database already exists, it retrieves the existing database. The method uses asynchronous calls to interact with the CouchDB client,
    /// ensuring that it handles database creation and retrieval efficiently. The use of generics allows for flexibility in specifying the type of documents
    /// that will be stored in the database, making this method suitable for various CouchDBCacheDocument types.
    /// </remarks>
    private async Task<CouchDatabase<T>> ResolveDatabase<T>(string dbName = default)
        where T : CouchDBCacheDocument
    {
        if (string.IsNullOrEmpty(dbName))
        {
            dbName = $"{typeof(T).Name.ToLowerInvariant()}s";
        }

        return !(await _connector.CouchDBClient.GetDatabasesNamesAsync()).Contains(dbName)
            ? await _connector.CouchDBClient.CreateDatabaseAsync<T>(dbName)
            : _connector.CouchDBClient.GetDatabase<T>();
    }

    /// <summary>
    /// Releases the resources used by the current instance of the class.
    /// </summary>
    /// <remarks>
    /// This method is part of the IDisposable interface implementation. It is responsible for
    /// cleaning up any resources that the instance may be holding onto, such as unmanaged resources
    /// or other disposable objects. The method calls the Dispose method on the _connector object
    /// to ensure that any resources it holds are also released. After disposing of the resources,
    /// it suppresses the finalization of the current object to optimize garbage collection.
    /// This method should be called when the object is no longer needed to prevent resource leaks.
    /// </remarks>
    public void Dispose()
    {
        _connector.Dispose();
        GC.SuppressFinalize(this);
    }
}
