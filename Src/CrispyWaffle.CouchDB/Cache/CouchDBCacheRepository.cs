using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CouchDB.Driver;
using CrispyWaffle.Cache;
using CrispyWaffle.CouchDB.Utils.Communications;
using CrispyWaffle.Log;

namespace CrispyWaffle.CouchDB.Cache;

/// <summary>
/// Class CouchDBCacheRepository.
/// </summary>
public class CouchDBCacheRepository : ICacheRepository, IDisposable
{
    private readonly CouchDBConnector _connector;

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
    public int GetDocCount<T>()
        where T : CouchDBCacheDocument =>
        ResolveDatabase<T>().Where(x => x.Id != null).ToList().Count;

    /// <inheritdoc />
    public void Clear() => Clear<CouchDBCacheDocument>();

    /// <summary>
    /// Clears all documents of type <typeparamref name="T"/> from the CouchDB database.
    /// </summary>
    /// <typeparam name="T">The type of documents to be cleared, which must inherit from <see cref="CouchDBCacheDocument"/>.</typeparam>
    /// <remarks>
    /// This method retrieves all documents of the specified type from the database that have a non-null Id.
    /// It then creates a list of asynchronous delete tasks for each document and waits for all tasks to complete.
    /// If an exception occurs during the process, it checks whether exceptions should be propagated or handled.
    /// If propagation is not desired, the exception is logged using the <see cref="LogConsumer"/>.
    /// This method does not return any value and modifies the state of the database by removing documents.
    /// </remarks>
    /// <exception cref="Exception">Thrown when an error occurs during the deletion process, unless exceptions are suppressed.</exception>
    public void Clear<T>()
        where T : CouchDBCacheDocument
    {
        try
        {
            var db = ResolveDatabase<T>();
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
    public T Get<T>(string key)
    {
        if (!typeof(CouchDBCacheDocument).IsAssignableFrom(typeof(T)))
        {
            return default;
        }

        return (T)(object)GetSpecific<CouchDBCacheDocument>(key);
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
    /// If it is, it calls the method <see cref="GetSpecific{CouchDBCacheDocument}"/> to retrieve the cached document associated with the provided keys.
    /// If the type is not assignable, it returns the default value for that type, which could be null for reference types or zero for numeric types.
    /// This method is useful for retrieving cached data in a type-safe manner, ensuring that only compatible types are processed.
    /// </remarks>
    public T Get<T>(string key, string subKey)
    {
        if (!typeof(CouchDBCacheDocument).IsAssignableFrom(typeof(T)))
        {
            return default;
        }

        return (T)(object)GetSpecific<CouchDBCacheDocument>(key, subKey);
    }

    /// <summary>
    /// Gets from a class specified database instead of the general <see cref="CouchDBCacheDocument"/> database.
    /// </summary>
    /// <typeparam name="T">Type T with base type <see cref="CouchDBCacheDocument"/>.</typeparam>
    /// <param name="key">A uniquely identifiable key to get document from the specified database.</param>
    /// <returns>The document if found.</returns>
    /// <exception cref="InvalidOperationException">Thrown in case the operation fails.</exception>
    public T GetSpecific<T>(string key)
        where T : CouchDBCacheDocument
    {
        try
        {
            var doc = ResolveDatabase<T>().Where(x => x.Key == key).FirstOrDefault();

            if (doc != default && doc.ExpiresAt != default && doc.ExpiresAt <= DateTime.UtcNow)
            {
                RemoveSpecific<T>(key);
                return default;
            }

            return doc;
        }
        catch (Exception e)
        {
            if (ShouldPropagateExceptions)
            {
                throw;
            }

            LogConsumer.Handle(e);
        }

        throw new InvalidOperationException($"Unable to get the item with key: {key}");
    }

    /// <summary>
    /// Retrieves a specific document from the CouchDB cache based on the provided key and subKey.
    /// </summary>
    /// <typeparam name="T">The type of the document to retrieve, which must inherit from <see cref="CouchDBCacheDocument"/>.</typeparam>
    /// <param name="key">The primary key used to identify the document.</param>
    /// <param name="subKey">The secondary key used to further specify the document.</param>
    /// <returns>The document of type <typeparamref name="T"/> if found and not expired; otherwise, returns <c>default</c> for the type.</returns>
    /// <remarks>
    /// This method queries the database for a document that matches the specified <paramref name="key"/> and <paramref name="subKey"/>.
    /// If a matching document is found, it checks whether the document has expired by comparing its expiration time with the current UTC time.
    /// If the document has expired, it is removed from the cache, and the method returns <c>default</c>.
    /// If an exception occurs during the database operation, it is either logged or propagated based on the <c>ShouldPropagateExceptions</c> flag.
    /// If no document is found and no exceptions are thrown, an <see cref="InvalidOperationException"/> is thrown indicating that the item could not be retrieved.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when unable to retrieve the item with the specified key and sub key.</exception>
    public T GetSpecific<T>(string key, string subKey)
        where T : CouchDBCacheDocument
    {
        try
        {
            var doc = ResolveDatabase<T>()
                .Where(x => x.Key == key && x.SubKey == subKey)
                .FirstOrDefault();

            if (doc != default && doc.ExpiresAt != default && doc.ExpiresAt <= DateTime.UtcNow)
            {
                RemoveSpecific<T>(key, subKey);
                return default;
            }

            return doc;
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
    public void Remove(string key) => RemoveSpecific<CouchDBCacheDocument>(key);

    /// <summary>
    /// Removes a specific entry from the cache based on the provided key and subKey.
    /// </summary>
    /// <param name="key">The primary key associated with the entry to be removed.</param>
    /// <param name="subKey">The secondary key associated with the entry to be removed.</param>
    /// <remarks>
    /// This method calls the generic method <c>RemoveSpecific</c> with the type <c>CouchDBCacheDocument</c> to remove the entry
    /// identified by the combination of <paramref name="key"/> and <paramref name="subKey"/>.
    /// It is important to ensure that both keys are correctly specified to successfully remove the intended entry from the cache.
    /// If the specified entry does not exist, no action will be taken, and no exceptions will be thrown.
    /// </remarks>
    public void Remove(string key, string subKey) =>
        RemoveSpecific<CouchDBCacheDocument>(key, subKey);

    /// <summary>
    /// Removes from a class specified database instead of the general <see cref="CouchDBCacheDocument"/> database.
    /// </summary>
    /// <typeparam name="T">Type T with base type <see cref="CouchDBCacheDocument"/>.</typeparam>
    /// <param name="key">A uniquely identifiable key to remove document from the specified database.</param>
    public void RemoveSpecific<T>(string key)
        where T : CouchDBCacheDocument
    {
        try
        {
            var db = _connector.CouchDBClient.GetDatabase<T>();
            var doc = db.Where(x => x.Key == key).FirstOrDefault();

            if (doc != default)
            {
                db.DeleteAsync(doc).Wait();
            }
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
    /// Removes a specific document from the CouchDB database based on the provided key and subKey.
    /// </summary>
    /// <typeparam name="T">The type of the CouchDB document, which must inherit from <see cref="CouchDBCacheDocument"/>.</typeparam>
    /// <param name="key">The key of the document to be removed.</param>
    /// <param name="subKey">The subKey of the document to be removed.</param>
    /// <remarks>
    /// This method retrieves a document from the CouchDB database that matches the specified <paramref name="key"/>
    /// and <paramref name="subKey"/>. If a matching document is found, it is deleted asynchronously from the database.
    /// If an exception occurs during this process, it is either propagated or handled based on the value of
    /// <see cref="ShouldPropagateExceptions"/>. If exceptions are not propagated, they are logged using the
    /// <see cref="LogConsumer"/>. This method does not return any value.
    /// </remarks>
    public void RemoveSpecific<T>(string key, string subKey)
        where T : CouchDBCacheDocument
    {
        try
        {
            var db = _connector.CouchDBClient.GetDatabase<T>();
            var doc = db.Where(x => x.Key == key && x.SubKey == subKey).FirstOrDefault();

            if (doc != default)
            {
                db.DeleteAsync(doc).Wait();
            }
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
    public void Set<T>(T value, string key, TimeSpan? ttl = null)
    {
        if (!typeof(CouchDBCacheDocument).IsAssignableFrom(typeof(T)))
        {
            return;
        }

        SetSpecific((CouchDBCacheDocument)(object)value, key, ttl);
    }

    /// <summary>
    /// Sets a value in the CouchDB cache using the specified key and sub-key.
    /// </summary>
    /// <typeparam name="T">The type of the value to be set, which must be a subclass of <see cref="CouchDBCacheDocument"/>.</typeparam>
    /// <param name="value">The value to be set in the cache.</param>
    /// <param name="key">The primary key under which the value is stored.</param>
    /// <param name="subKey">The sub-key under which the value is stored.</param>
    /// <remarks>
    /// This method checks if the provided type <typeparamref name="T"/> is assignable from <see cref="CouchDBCacheDocument"/>.
    /// If it is not, the method returns without performing any action.
    /// If the type is valid, it calls the <see cref="SetSpecific"/> method to set the value in the cache.
    /// This allows for type-safe caching of documents that inherit from <see cref="CouchDBCacheDocument"/>.
    /// </remarks>
    public void Set<T>(T value, string key, string subKey)
    {
        if (!typeof(CouchDBCacheDocument).IsAssignableFrom(typeof(T)))
        {
            return;
        }

        SetSpecific((CouchDBCacheDocument)(object)value, key, subKey);
    }

    /// <summary>
    /// Persists to a class specified database instead of the general <see cref="CouchDBCacheDocument"/> database.
    /// </summary>
    /// <typeparam name="T">Type T with base type <see cref="CouchDBCacheDocument"/>.</typeparam>
    /// <param name="value">The value of type T to be persisted.</param>
    /// <param name="key">A uniquely identifiable key to remove document from the specified database.</param>
    /// <param name="ttl">How long the value should be stored.</param>
    public void SetSpecific<T>(T value, string key, TimeSpan? ttl = null)
        where T : CouchDBCacheDocument
    {
        try
        {
            value.Key = key;

            if (ttl != null)
            {
                value.TTL = ttl.Value;
                value.ExpiresAt = DateTime.UtcNow.Add(ttl.Value);
            }

            ResolveDatabase<T>().CreateAsync(value).Wait();
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
    /// <remarks>
    /// This method assigns the provided <paramref name="key"/> and <paramref name="subKey"/> to the specified
    /// <paramref name="value"/> of type <typeparamref name="T"/>. It then attempts to create or update the
    /// document in the database asynchronously using the ResolveDatabase method. If an exception occurs during
    /// this process, it checks whether exceptions should be propagated. If propagation is enabled, the exception
    /// is rethrown; otherwise, it is logged using the LogConsumer.
    /// </remarks>
    /// <exception cref="Exception">Thrown when an error occurs during the database operation, unless exceptions are suppressed.</exception>
    public void SetSpecific<T>(T value, string key, string subKey)
        where T : CouchDBCacheDocument
    {
        try
        {
            value.Key = key;
            value.SubKey = subKey;

            ResolveDatabase<T>().CreateOrUpdateAsync(value).Wait();
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
    public bool TryGet<T>(string key, out T value)
    {
        var get = Get<CouchDBCacheDocument>(key);

        if (get != default)
        {
            value = (T)(object)get;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to retrieve a value associated with the specified keys from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The primary key used to access the cached value.</param>
    /// <param name="subKey">The secondary key used to access the cached value.</param>
    /// <param name="value">When this method returns, contains the retrieved value if successful; otherwise, the default value for type <typeparamref name="T"/>.</param>
    /// <returns>True if the value was found and retrieved successfully; otherwise, false.</returns>
    /// <remarks>
    /// This method attempts to fetch a cached document from a data store using a combination of a primary key and a secondary key.
    /// If the document is found, it is cast to the specified type <typeparamref name="T"/> and returned through the out parameter <paramref name="value"/>.
    /// If no document is found, <paramref name="value"/> is set to its default value, and the method returns false.
    /// This is useful for safely attempting to retrieve values without throwing exceptions if the keys do not exist in the cache.
    /// </remarks>
    public bool TryGet<T>(string key, string subKey, out T value)
    {
        var get = Get<CouchDBCacheDocument>(key, subKey);

        if (get != default)
        {
            value = (T)(object)get;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Retrieves the Time-To-Live (TTL) value for a specified cache key.
    /// </summary>
    /// <param name="key">The key for which the TTL value is to be retrieved.</param>
    /// <returns>The TimeSpan representing the TTL for the specified <paramref name="key"/>.</returns>
    /// <remarks>
    /// This method accesses a CouchDB document associated with the provided <paramref name="key"/>
    /// and retrieves its Time-To-Live (TTL) value. The TTL indicates the duration for which the
    /// cached item is considered valid. If the TTL has expired, the item may be removed from the
    /// cache, and subsequent requests for this key may result in a cache miss.
    /// This method assumes that the key exists in the cache; if it does not, the behavior will depend
    /// on the implementation of the Get method.
    /// </remarks>
    public TimeSpan TTL(string key) => Get<CouchDBCacheDocument>(key).TTL;

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
    private CouchDatabase<T> ResolveDatabase<T>(string dbName = default)
        where T : CouchDBCacheDocument
    {
        if (string.IsNullOrEmpty(dbName))
        {
            dbName = $"{typeof(T).Name.ToLowerInvariant()}s";
        }

        return !_connector.CouchDBClient.GetDatabasesNamesAsync().Result.Contains(dbName)
            ? _connector.CouchDBClient.CreateDatabaseAsync<T>().Result
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
