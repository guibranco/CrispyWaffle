using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CouchDB.Driver;
using CouchDB.Driver.Settings;
using CrispyWaffle.Cache;
using CrispyWaffle.Configuration;
using CrispyWaffle.CouchDB.DTOs;
using CrispyWaffle.Log;

namespace CrispyWaffle.CouchDB
{
    /// <summary>
    /// Class CouchDBCacheRepository.
    /// </summary>
    public class CouchDBCacheRepository : ICacheRepository, IDisposable
    {
        private readonly CouchClient _couchClient;

        /// <inheritdoc />
        public bool ShouldPropagateExceptions { get; set; }

        /// <summary>
        /// Retrieves the count of documents of a specified type from the database.
        /// </summary>
        /// <typeparam name="T">The type of documents to count, which must inherit from <see cref="CouchDoc"/>.</typeparam>
        /// <returns>The number of documents of type <typeparamref name="T"/> that have a non-null Id.</returns>
        /// <remarks>
        /// This method first resolves the database for the specified document type <typeparamref name="T"/>. 
        /// It then filters the documents to include only those with a non-null Id. 
        /// Finally, it converts the filtered results to a list and returns the count of those documents. 
        /// This is useful for determining how many valid documents of a certain type exist in the database.
        /// </remarks>
        public int GetDocCount<T>()
            where T : CouchDoc
        {
            return ResolveDatabase<T>().Where(x => x.Id != null).ToList().Count;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CouchDBCacheRepository"/> class.
        /// </summary>
        /// <param name="connection">Connection information including username and password.</param>
        /// <param name="authType">The type of authentication to be used.</param>
        /// <param name="cookieDuration">Cookie duration in case cookie auth is used.</param>
        public CouchDBCacheRepository(IConnection connection, AuthType authType, int cookieDuration = 10)
        {
            try
            {
                _couchClient = new CouchClient(
                    $"{connection.Host}:{connection.Port}",
                    GetAuth(authType, connection)
                );
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

        /// <inheritdoc />
        public void Clear()
        {
            Clear<CouchDoc>();
        }

        /// <summary>
        /// Clears all documents of type <typeparamref name="T"/> from the database.
        /// </summary>
        /// <typeparam name="T">The type of documents to be cleared, which must inherit from <see cref="CouchDoc"/>.</typeparam>
        /// <remarks>
        /// This method retrieves all documents of the specified type from the database that have a non-null Id.
        /// It then deletes each document asynchronously, waiting for all delete operations to complete before finishing.
        /// If an exception occurs during the deletion process, it checks the <see cref="ShouldPropagateExceptions"/> flag.
        /// If this flag is set to true, the exception is re-thrown; otherwise, it is handled by the <see cref="HandleException"/> method.
        /// This ensures that the operation can either fail fast or be handled gracefully, depending on the configuration.
        /// </remarks>
        /// <exception cref="Exception">Thrown when an error occurs during the deletion of documents if <see cref="ShouldPropagateExceptions"/> is true.</exception>
        public void Clear<T>()
            where T : CouchDoc
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

                HandleException(e);
            }
        }

        /// <inheritdoc />
        public T Get<T>(string key)
        {
            if (!typeof(CouchDoc).IsAssignableFrom(typeof(T)))
            {
                return default;
            }

            return (T)(object)GetSpecific<CouchDoc>(key);
        }

        /// <summary>
        /// Retrieves a specific document of type <typeparamref name="T"/> from the data store using the provided keys.
        /// </summary>
        /// <typeparam name="T">The type of the document to be retrieved, which must be a subclass of <see cref="CouchDoc"/>.</typeparam>
        /// <param name="key">The primary key used to identify the document.</param>
        /// <param name="subKey">The secondary key used to further specify the document.</param>
        /// <returns>
        /// Returns an instance of type <typeparamref name="T"/> if the type is valid and the document is found; otherwise, returns the default value for the type.
        /// </returns>
        /// <remarks>
        /// This method checks if the specified type <typeparamref name="T"/> is assignable from <see cref="CouchDoc"/>. 
        /// If it is not, the method returns the default value for that type. 
        /// If the type is valid, it calls another method, <see cref="GetSpecific{CouchDoc}"/>, to retrieve the document associated with the provided keys.
        /// This allows for a flexible retrieval mechanism for documents stored in a CouchDB-like structure.
        /// </remarks>
        public T Get<T>(string key, string subKey)
        {
            if (!typeof(CouchDoc).IsAssignableFrom(typeof(T)))
            {
                return default;
            }

            return (T)(object)GetSpecific<CouchDoc>(key, subKey);
        }

        /// <summary>
        /// Gets from a class specified database instead of the general <see cref="CouchDoc"/> database.
        /// </summary>
        /// <typeparam name="T">Type T with base type <see cref="CouchDoc"/>.</typeparam>
        /// <param name="key">A uniquely identifiable key to get document from the specified database.</param>
        /// <returns>The document if found.</returns>
        /// <exception cref="InvalidOperationException">Thrown in case the operation fails.</exception>
        public T GetSpecific<T>(string key)
            where T : CouchDoc
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

                HandleException(e);
            }

            throw new InvalidOperationException($"Unable to get the item with key: {key}");
        }

        /// <summary>
        /// Retrieves a specific document from the database based on the provided key and subKey.
        /// </summary>
        /// <typeparam name="T">The type of the document to retrieve, which must inherit from <see cref="CouchDoc"/>.</typeparam>
        /// <param name="key">The primary key used to identify the document.</param>
        /// <param name="subKey">The secondary key used to further identify the document.</param>
        /// <returns>A document of type <typeparamref name="T"/> if found and not expired; otherwise, returns the default value for type <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// This method queries the database for a document that matches the specified key and subKey. 
        /// If a matching document is found, it checks if the document has expired by comparing its expiration date with the current UTC time.
        /// If the document has expired, it is removed from the database, and the method returns the default value for the type <typeparamref name="T"/>.
        /// If an exception occurs during the operation, it will either be propagated or handled based on the value of <see cref="ShouldPropagateExceptions"/>.
        /// If no document is found or an error occurs, an <see cref="InvalidOperationException"/> is thrown with a message indicating the failure to retrieve the item.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when unable to get the item with the specified key and subKey.</exception>
        public T GetSpecific<T>(string key, string subKey)
            where T : CouchDoc
        {
            try
            {
                var doc = ResolveDatabase<T>().Where(x => x.Key == key && x.SubKey == subKey).FirstOrDefault();

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

                HandleException(e);
            }

            throw new InvalidOperationException($"Unable to get the item with key: {key} and sub key: {subKey}");
        }

        /// <inheritdoc />
        public void Remove(string key)
        {
            RemoveSpecific<CouchDoc>(key);
        }

        /// <summary>
        /// Removes a specific entry identified by the provided key and subKey.
        /// </summary>
        /// <param name="key">The primary key of the entry to be removed.</param>
        /// <param name="subKey">The secondary key that further identifies the entry to be removed.</param>
        /// <remarks>
        /// This method calls the generic method <c>RemoveSpecific</c> with the type parameter <c>CouchDoc</c>,
        /// which is responsible for handling the removal of the specified entry from the underlying data structure.
        /// The method does not return any value and is intended to modify the state of the data store by removing the specified entry.
        /// It is important to ensure that both the key and subKey correspond to an existing entry; otherwise, the removal operation may not have any effect.
        /// </remarks>
        public void Remove(string key, string subKey)
        {
            RemoveSpecific<CouchDoc>(key, subKey);
        }

        /// <summary>
        /// Removes from a class specified database instead of the general <see cref="CouchDoc"/> database.
        /// </summary>
        /// <typeparam name="T">Type T with base type <see cref="CouchDoc"/>.</typeparam>
        /// <param name="key">A uniquely identifiable key to remove document from the specified database.</param>
        public void RemoveSpecific<T>(string key)
            where T : CouchDoc
        {
            try
            {
                var db = _couchClient.GetDatabase<T>();
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

                HandleException(e);
            }
        }

        /// <summary>
        /// Removes a specific document from the database based on the provided key and subKey.
        /// </summary>
        /// <typeparam name="T">The type of the document that extends <see cref="CouchDoc"/>.</typeparam>
        /// <param name="key">The key of the document to be removed.</param>
        /// <param name="subKey">The subKey of the document to be removed.</param>
        /// <remarks>
        /// This method retrieves a document from the database that matches the specified key and subKey.
        /// If a matching document is found, it is deleted asynchronously from the database.
        /// If an exception occurs during the operation, it will either be propagated or handled based on the value of <see cref="ShouldPropagateExceptions"/>.
        /// This method does not return any value, as it performs a deletion operation.
        /// </remarks>
        /// <exception cref="Exception">Thrown when an error occurs during the database operation, unless suppressed by <see cref="ShouldPropagateExceptions"/>.</exception>
        public void RemoveSpecific<T>(string key, string subKey)
            where T : CouchDoc
        {
            try
            {
                var db = _couchClient.GetDatabase<T>();
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

                HandleException(e);
            }
        }

        /// <inheritdoc />
        public void Set<T>(T value, string key, TimeSpan? ttl = null)
        {
            if (!typeof(CouchDoc).IsAssignableFrom(typeof(T)))
            {
                return;
            }

            SetSpecific((CouchDoc)(object)value, key, ttl);
        }

        /// <summary>
        /// Sets a value of type <typeparamref name="T"/> in a Couch document using the specified keys.
        /// </summary>
        /// <typeparam name="T">The type of the value to be set, which must be a subclass of <see cref="CouchDoc"/>.</typeparam>
        /// <param name="value">The value to be set in the Couch document.</param>
        /// <param name="key">The primary key under which the value will be stored.</param>
        /// <param name="subKey">The secondary key for further categorization of the value.</param>
        /// <remarks>
        /// This method checks if the provided type <typeparamref name="T"/> is assignable from <see cref="CouchDoc"/>.
        /// If it is not, the method returns without performing any action. 
        /// If the type is valid, it casts the value to <see cref="CouchDoc"/> and calls the 
        /// SetSpecific method to store the value using the provided keys.
        /// This ensures that only valid Couch document types are processed.
        /// </remarks>
        public void Set<T>(T value, string key, string subKey)
        {
            if (!typeof(CouchDoc).IsAssignableFrom(typeof(T)))
            {
                return;
            }

            SetSpecific((CouchDoc)(object)value, key, subKey);
        }

        /// <summary>
        /// Persists to a class specified database instead of the general <see cref="CouchDoc"/> database.
        /// </summary>
        /// <typeparam name="T">Type T with base type <see cref="CouchDoc"/>.</typeparam>
        /// <param name="value">The value of type T to be persisted.</param>
        /// <param name="key">A uniquely identifiable key to remove document from the specified database.</param>
        /// <param name="ttl">How long the value should be stored.</param>
        public void SetSpecific<T>(T value, string key, TimeSpan? ttl = null)
            where T : CouchDoc
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

                HandleException(e);
            }
        }

        /// <summary>
        /// Sets the specific key and subKey for a given CouchDoc object and updates the database.
        /// </summary>
        /// <typeparam name="T">The type of the CouchDoc object.</typeparam>
        /// <param name="value">The CouchDoc object to be updated.</param>
        /// <param name="key">The key to be set on the CouchDoc object.</param>
        /// <param name="subKey">The subKey to be set on the CouchDoc object.</param>
        /// <remarks>
        /// This method assigns the provided <paramref name="key"/> and <paramref name="subKey"/> to the 
        /// <paramref name="value"/> parameter, which is expected to be of type CouchDoc or a derived type. 
        /// It then attempts to create or update the corresponding entry in the database asynchronously. 
        /// If an exception occurs during this process, it will either propagate the exception or handle it 
        /// based on the value of the <c>ShouldPropagateExceptions</c> property. 
        /// This allows for flexible error handling depending on the context in which this method is called.
        /// </remarks>
        /// <exception cref="Exception">Thrown when an error occurs during the database operation, unless exceptions are suppressed.</exception>
        public void SetSpecific<T>(T value, string key, string subKey)
            where T : CouchDoc
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

                HandleException(e);
            }
        }

        /// <inheritdoc />
        public bool TryGet<T>(string key, out T value)
        {
            var get = Get<CouchDoc>(key);

            if (get != default)
            {
                value = (T)(object)get;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Tries to retrieve a value associated with the specified key and subKey.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="key">The primary key associated with the value.</param>
        /// <param name="subKey">The secondary key associated with the value.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified keys, or the default value of <typeparamref name="T"/> if the keys do not exist.</param>
        /// <returns>True if the value was found; otherwise, false.</returns>
        /// <remarks>
        /// This method attempts to retrieve a value from a data source using the provided <paramref name="key"/> and <paramref name="subKey"/>. 
        /// If a value is found, it is cast to the specified type <typeparamref name="T"/> and returned through the out parameter <paramref name="value"/>. 
        /// If no value is found, <paramref name="value"/> is set to its default value, and the method returns false. 
        /// This allows for safe retrieval of values without throwing exceptions when keys are not present.
        /// </remarks>
        public bool TryGet<T>(string key, string subKey, out T value)
        {
            var get = Get<CouchDoc>(key, subKey);

            if (get != default)
            {
                value = (T)(object)get;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Retrieves the Time-To-Live (TTL) value for a specified key.
        /// </summary>
        /// <param name="key">The key for which the TTL value is to be retrieved.</param>
        /// <returns>The TimeSpan representing the TTL for the specified <paramref name="key"/>.</returns>
        /// <remarks>
        /// This method accesses a CouchDB document associated with the provided <paramref name="key"/> 
        /// and retrieves its Time-To-Live (TTL) property. The TTL indicates the duration for which 
        /// the document is valid before it is considered expired. The method utilizes a generic 
        /// Get method to fetch the document and directly accesses its TTL property. 
        /// It is important to ensure that the key exists in the database; otherwise, 
        /// this method may not return a valid TTL value.
        /// </remarks>
        public TimeSpan TTL(string key)
        {
            return Get<CouchDoc>(key).TTL;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the resources used by the current instance of the class.
        /// </summary>
        /// <param name="disposing">A boolean value indicating whether the method was called directly or by the garbage collector.</param>
        /// <remarks>
        /// This method is part of the IDisposable pattern. When <paramref name="disposing"/> is true, 
        /// the method has been called directly or indirectly by a user's code, and managed resources 
        /// should be disposed. If <paramref name="disposing"/> is false, the method has been called 
        /// by the runtime from inside the finalizer and only unmanaged resources should be released. 
        /// This implementation disposes of the <c>_couchClient</c> if it is not null, ensuring that 
        /// any resources held by it are properly released.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _couchClient?.Dispose();
            }
        }

        /// <summary>
        /// Retrieves an authentication action based on the specified authentication type.
        /// </summary>
        /// <param name="type">The type of authentication to be used (Basic or Cookie).</param>
        /// <param name="connection">The connection object containing credentials for authentication.</param>
        /// <param name="cookieDuration">The duration for which the cookie is valid (default is 10 minutes).</param>
        /// <returns>An action that configures the <paramref name="CouchSettings"/> for the specified authentication type.</returns>
        /// <remarks>
        /// This method checks the provided <paramref name="type"/> to determine which authentication method to use.
        /// If the type is set to Basic, it returns an action that configures the <paramref name="CouchSettings"/> 
        /// to use basic authentication with the username and password from the provided <paramref name="connection"/>.
        /// If the type is set to Cookie, it returns an action that configures the settings to use cookie-based 
        /// authentication, also utilizing the credentials from the connection and applying the specified cookie duration.
        /// This allows for flexible authentication strategies based on the needs of the application.
        /// </remarks>
        private static Action<CouchSettings> GetAuth(AuthType type, IConnection connection, int cookieDuration = 10)
        {
            if (type == AuthType.Basic)
            {
                return (CouchSettings s) => s.UseBasicAuthentication(
                    connection.Credentials.Username,
                    connection.Credentials.Password
                );
            }

            return (CouchSettings s) => s.UseCookieAuthentication(
                    connection.Credentials.Username,
                    connection.Credentials.Password,
                    cookieDuration
            );
        }

        /// <summary>
        /// Resolves a CouchDB database for a specified document type.
        /// </summary>
        /// <typeparam name="T">The type of the CouchDB document.</typeparam>
        /// <param name="dbName">The name of the database to resolve. If not provided, defaults to the pluralized name of the document type.</param>
        /// <returns>A <see cref="CouchDatabase{T}"/> instance representing the resolved database.</returns>
        /// <remarks>
        /// This method checks if the provided database name is null or empty. If it is, the method generates a default database name by converting the document type's name to lowercase and appending an 's' to it, indicating a collection of documents.
        /// It then verifies if the database exists by retrieving the list of database names asynchronously. If the database does not exist, it creates a new database for the specified document type asynchronously.
        /// If the database already exists, it retrieves and returns the existing database instance.
        /// This method is essential for ensuring that the application interacts with the correct CouchDB database for the specified document type.
        /// </remarks>
        private CouchDatabase<T> ResolveDatabase<T>(string dbName = default)
            where T : CouchDoc
        {
            if (string.IsNullOrEmpty(dbName))
            {
                dbName = $"{typeof(T).Name.ToLowerInvariant()}s";
            }

            if (!_couchClient.GetDatabasesNamesAsync().Result.Contains(dbName))
            {
                return _couchClient.CreateDatabaseAsync<T>().Result;
            }

            return _couchClient.GetDatabase<T>();
        }

        /// <summary>
        /// Handles an exception by logging it and performing additional handling.
        /// </summary>
        /// <param name="e">The exception to be handled.</param>
        /// <remarks>
        /// This method takes an exception as input and first logs the exception details using the 
        /// <see cref="LogConsumer.Trace"/> method. After logging, it calls the 
        /// <see cref="LogConsumer.Handle"/> method to perform any necessary handling of the exception. 
        /// This is useful for centralized error management in applications, allowing developers to 
        /// track and respond to exceptions consistently.
        /// </remarks>
        private static void HandleException(Exception e)
        {
            LogConsumer.Trace(e);
            LogConsumer.Handle(e);
        }
    }

    /// <summary>
    /// Auth type to use.
    /// </summary>
    public enum AuthType
    {
        /// <summary>
        /// Basic auth type.
        /// </summary>
        Basic,

        /// <summary>
        /// Cookie based auth.
        /// </summary>
        Cookie
    }
}
