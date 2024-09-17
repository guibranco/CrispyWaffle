﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CrispyWaffle.Cache;
using CrispyWaffle.Log;
using CrispyWaffle.Redis.Utils.Communications;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace CrispyWaffle.Redis.Cache
{
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
            _connector
                .GetDatabase(databaseNumber)
                .StringSet(key, inputBytes, ttl, When.Always, flags);
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
        public void RemoveFromDatabase(string key, int databaseNumber)
        {
            _connector.GetDatabase(databaseNumber).KeyDelete(key);
        }

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
        public async Task SetAsync<T>(T value, string key, TimeSpan? ttl = null)
        {
            try
            {
                if (ttl.HasValue)
                {
                    await _cacheClient.Db0.AddAsync(key, value, ttl.Value);
                }
                else
                {
                    await _cacheClient.Db0.AddAsync(key, value);
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
        /// Sets the specified value.
        /// </summary>
        /// <typeparam name="T">The type of the document.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        public async Task SetAsync<T>(T value, string key, string subKey)
        {
            try
            {
                var allValues = _cacheClient.Db0.ExistsAsync(key).Result
                    ? _cacheClient.Db0.HashGetAllAsync<T>(key, CommandFlags.PreferReplica).Result
                    : new Dictionary<string, T>();
                allValues[subKey] = value;
                await _cacheClient.Db0.HashSetAsync(key, allValues, CommandFlags.FireAndForget);
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
        /// Gets the specified key.
        /// </summary>
        /// <typeparam name="T">The type of object (the object will be cast to this type).</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>T.</returns>
        /// <exception cref="InvalidOperationException">"Unable to get the item with key.</exception>
        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                if (_cacheClient.Db0.ExistsAsync(key).Result)
                {
                    var returnResult = await Task.Run(() => _cacheClient.Db0.GetAsync<T>(key, CommandFlags.PreferReplica).Result);
                    return returnResult;
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

            throw new InvalidOperationException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    "Unable to get the item with key {0}",
                    key
                )
            );
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the document.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        /// <returns>T.</returns>
        /// <exception cref="InvalidOperationException">Unable to get the item with key and sub key.</exception>
        public Task<T> GetAsync<T>(string key, string subKey)
        {
            try
            {
                if (
                    _cacheClient.Db0.HashExistsAsync(key, subKey, CommandFlags.PreferReplica).Result
                )
                {
                    return Task.FromResult(_cacheClient
                        .Db0.HashGetAsync<T>(key, subKey, CommandFlags.PreferReplica)
                        .Result);
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

            throw new InvalidOperationException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    "Unable to get the item with key {0} and sub key {1}",
                    key,
                    subKey
                )
            );
        }

        /// <summary>
        /// Tries to get a value based on its key, if exists return true, else false.
        /// The out parameter value is the object requested.
        /// </summary>
        /// <typeparam name="T">The type of object (the object will be cast to this type).</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>Returns <b>True</b> if the object with the key exists, false otherwise.</returns>
        public async Task<(bool Exists, T value)> TryGetAsync<T>(string key)
        {
            T value = default;
            try
            {
                value = _cacheClient.Db0.GetAsync<T>(key, CommandFlags.PreferReplica).Result;
                return (_cacheClient.Db0.ExistsAsync(key).Result,value);
            }
            catch (Exception e)
            {
                if (ShouldPropagateExceptions)
                {
                    throw;
                }

                HandleException(e);
            }

            return (false, value);
        }

        /// <summary>
        /// Tries the get.
        /// </summary>
        /// <typeparam name="T">The type of the document.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if get the key, <c>false</c> otherwise.</returns>
        public async Task<(bool Exists, T value)> TryGetAsync<T>(string key, string subKey)
        {
            T value = default;
            try
            {
                value = _cacheClient
                    .Db0.HashGetAsync<T>(key, subKey, CommandFlags.PreferReplica)
                    .Result;
                return (_cacheClient.Db0.HashExistsAsync(key, subKey).Result, value);
            }
            catch (Exception e)
            {
                if (ShouldPropagateExceptions)
                {
                    throw;
                }

                HandleException(e);
            }

            return (false, value);
        }

        /// <summary>
        /// Removes the specified key from the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cacheClient.Db0.RemoveAsync(key);
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
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        public async Task RemoveAsync(string key, string subKey)
        {
            try
            {
                await _cacheClient.Db0.HashDeleteAsync(key, subKey, CommandFlags.FireAndForget);
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
        /// <returns>The timespan until this key is expired from the cache or 0 if it's already expired or doesn't exists.</returns>
        public async Task<TimeSpan> TTLAsync(string key)
        {
            try
            {
                return _cacheClient.Db0.Database.KeyTimeToLive(key, CommandFlags.PreferReplica)
                    ?? TimeSpan.Zero;
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
        public async Task ClearAsync()
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
}
