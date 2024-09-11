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
        /// Get total count of documents in a database.
        /// </summary>
        /// <typeparam name="T">Type T with base type <see cref="CouchDoc"/>.</typeparam>
        /// <returns>Total cound of documents.</returns>
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
        /// Clears a class specified database instead of the general <see cref="CouchDoc"/> database.
        /// </summary>
        /// <typeparam name="T">Type T with base type <see cref="CouchDoc"/>.</typeparam>
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

        /// <inheritdoc />
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
        /// Gets from a class specified database instead of the general <see cref="CouchDoc"/> database.
        /// </summary>
        /// <typeparam name="T">Type T with base type <see cref="CouchDoc"/>.</typeparam>
        /// <param name="key">A uniquely identifiable key to get document from the specified database.</param>
        /// <param name="subKey">The sub key.</param>
        /// <returns>The document if found.</returns>
        /// <exception cref="InvalidOperationException">Thrown in case the operation fails.</exception>
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

        /// <inheritdoc />
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
        /// Removes from a class specified database instead of the general <see cref="CouchDoc"/> database.
        /// </summary>
        /// <typeparam name="T">Type T with base type <see cref="CouchDoc"/>.</typeparam>
        /// <param name="key">A uniquely identifiable key to remove document from the specified database.</param>
        /// <param name="subKey">The sub key.</param>
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

        /// <inheritdoc />
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
        /// Persists to a class specified database instead of the general <see cref="CouchDoc"/> database.
        /// </summary>
        /// <typeparam name="T">Type T with base type <see cref="CouchDoc"/>.</typeparam>
        /// <param name="value">The value of type T to be persisted.</param>
        /// <param name="key">A uniquely identifiable key to remove document from the specified database.</param>
        /// <param name="subKey">The sub key.</param>
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

        /// <inheritdoc />
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

        /// <inheritdoc/>
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
        /// Disposes resources.
        /// </summary>
        /// <param name="disposing">True if called explicitly.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _couchClient?.Dispose();
            }
        }

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
