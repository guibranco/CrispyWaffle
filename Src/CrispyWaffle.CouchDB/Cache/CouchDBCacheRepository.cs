using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CouchDB.Driver;
using CrispyWaffle.Cache;
using CrispyWaffle.CouchDB.Utils.Communications;
using CrispyWaffle.Log;

namespace CrispyWaffle.CouchDB.Cache
{
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
        public CouchDBCacheRepository(CouchDBConnector connector)
        {
            _connector = connector;
        }

        /// <summary>
        /// Get total count of documents in a database.
        /// </summary>
        /// <typeparam name="T">Type T with base type <see cref="CouchDBCacheDocument"/>.</typeparam>
        /// <returns>Total count of documents.</returns>
        public int GetDocCount<T>()
            where T : CouchDBCacheDocument
        {
            return ResolveDatabase<T>().Where(x => x.Id != null).ToList().Count;
        }

        /// <inheritdoc />
        public void Clear()
        {
            Clear<CouchDBCacheDocument>();
        }

        /// <summary>
        /// Clears a class specified database instead of the general <see cref="CouchDBCacheDocument"/> database.
        /// </summary>
        /// <typeparam name="T">Type T with base type <see cref="CouchDBCacheDocument"/>.</typeparam>
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

        /// <inheritdoc />
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
        /// Gets from a class specified database instead of the general <see cref="CouchDBCacheDocument"/> database.
        /// </summary>
        /// <typeparam name="T">Type T with base type <see cref="CouchDBCacheDocument"/>.</typeparam>
        /// <param name="key">A uniquely identifiable key to get document from the specified database.</param>
        /// <param name="subKey">The sub key.</param>
        /// <returns>The document if found.</returns>
        /// <exception cref="InvalidOperationException">Thrown in case the operation fails.</exception>
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
        public void Remove(string key)
        {
            RemoveSpecific<CouchDBCacheDocument>(key);
        }

        /// <inheritdoc />
        public void Remove(string key, string subKey)
        {
            RemoveSpecific<CouchDBCacheDocument>(key, subKey);
        }

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
        /// Removes from a class specified database instead of the general <see cref="CouchDBCacheDocument"/> database.
        /// </summary>
        /// <typeparam name="T">Type T with base type <see cref="CouchDBCacheDocument"/>.</typeparam>
        /// <param name="key">A uniquely identifiable key to remove document from the specified database.</param>
        /// <param name="subKey">The sub key.</param>
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

        /// <inheritdoc />
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
        /// Persists to a class specified database instead of the general <see cref="CouchDBCacheDocument"/> database.
        /// </summary>
        /// <typeparam name="T">Type T with base type <see cref="CouchDBCacheDocument"/>.</typeparam>
        /// <param name="value">The value of type T to be persisted.</param>
        /// <param name="key">A uniquely identifiable key to remove document from the specified database.</param>
        /// <param name="subKey">The sub key.</param>
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

        /// <inheritdoc />
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

        /// <inheritdoc/>
        public TimeSpan TTL(string key)
        {
            return Get<CouchDBCacheDocument>(key).TTL;
        }

        /// <summary>
        /// Resolves the database.
        /// </summary>
        /// <typeparam name="T">The type of the document.</typeparam>
        /// <param name="dbName">Name of the database.</param>
        /// <returns>CouchDatabase&lt;T&gt;.</returns>
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
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _connector.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
