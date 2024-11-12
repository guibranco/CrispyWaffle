using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CrispyWaffle.Composition;
using CrispyWaffle.Log;

namespace CrispyWaffle.Cache
{
    /// <summary>
    /// Manages cache repositories and provides utilities for adding, retrieving, and managing cached data.
    /// </summary>
    public static class CacheManager
    {
        /// <summary>
        /// The repositories.
        /// </summary>
        private static readonly SortedList<int, ICacheRepository> _repositories = new();

        /// <summary>
        /// The current priority.
        /// </summary>
        private static int _currentPriority;

        /// <summary>
        /// The memory type.
        /// </summary>
        private static readonly Type _memoryType = typeof(MemoryCacheRepository);

        /// <summary>
        /// The is memory repository in list.
        /// </summary>
        private static bool _isMemoryRepositoryInList;

        /// <summary>
        /// Adds a new cache repository of the specified type with an automatically assigned priority.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository to add.</typeparam>
        /// <returns>The added cache repository.</returns>
        public static ICacheRepository AddRepository<TCacheRepository>()
            where TCacheRepository : ICacheRepository
        {
            var repository = ServiceLocator.Resolve<TCacheRepository>();
            AddRepository(repository, _currentPriority++);
            return repository;
        }

        /// <summary>
        /// Adds a cache repository with an automatically assigned priority.
        /// </summary>
        /// <param name="repository">The cache repository to add.</param>
        /// <returns>The added cache repository.</returns>
        public static ICacheRepository AddRepository(ICacheRepository repository)
        {
            AddRepository(repository, _currentPriority++);
            return repository;
        }

        /// <summary>
        /// Adds a new cache repository of the specified type with a defined priority.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository to add.</typeparam>
        /// <param name="priority">The priority of the cache repository.</param>
        /// <returns>The added cache repository.</returns>
        public static ICacheRepository AddRepository<TCacheRepository>(int priority)
            where TCacheRepository : ICacheRepository
        {
            var repository = ServiceLocator.Resolve<TCacheRepository>();
            AddRepository(repository, priority);
            return repository;
        }

        /// <summary>
        /// Adds a cache repository with a defined priority.
        /// </summary>
        /// <param name="repository">The cache repository to add.</param>
        /// <param name="priority">The priority of the cache repository.</param>
        /// <returns>The priority assigned to the cache repository.</returns>
        public static int AddRepository(ICacheRepository repository, int priority)
        {
            while (true)
            {
                if (!_repositories.ContainsKey(priority))
                {
                    _repositories.Add(priority, repository);
                    LogConsumer.Trace(
                        "Adding cache repository of type {0} with priority {1}",
                        repository.GetType().FullName,
                        priority
                    );
                    if (repository.GetType() == _memoryType)
                    {
                        _isMemoryRepositoryInList = true;
                    }

                    return priority;
                }

                priority++;
            }
        }

        /// <summary>
        /// Adds a value to all cache repositories.
        /// </summary>
        /// <typeparam name="T">The type of the value to cache.</typeparam>
        /// <param name="value">The value to cache.</param>
        /// <param name="key">The key under which to store the value.</param>
        public static void Set<T>(T value, [Localizable(false)] string key)
        {
            LogConsumer.Trace("Adding {0} to {1} cache repositories", key, _repositories.Count);
            foreach (var repository in _repositories.Values)
            {
                repository.Set(value, key);
            }
        }

        /// <summary>
        /// Adds a value with a subkey to all cache repositories.
        /// </summary>
        /// <typeparam name="T">The type of the value to cache.</typeparam>
        /// <param name="value">The value to cache.</param>
        /// <param name="key">The key under which to store the value.</param>
        /// <param name="subKey">The sub key for additional categorization.</param>
        public static void Set<T>(
            T value,
            [Localizable(false)] string key,
            [Localizable(false)] string subKey
        )
        {
            LogConsumer.Trace(
                "Adding {0}/{2} to {1} cache repositories",
                key,
                _repositories.Count,
                subKey
            );
            foreach (var repository in _repositories.Values)
            {
                repository.Set(value, key, subKey);
            }
        }

        /// <summary>
        /// Adds a value with a time-to-live (TTL) to all cache repositories.
        /// </summary>
        /// <typeparam name="T">The type of the value to cache.</typeparam>
        /// <param name="value">The value to cache.</param>
        /// <param name="key">The key under which to store the value.</param>
        /// <param name="ttl">The time-to-live for the cached value.</param>
        public static void Set<T>(T value, [Localizable(false)] string key, TimeSpan ttl)
        {
            LogConsumer.Trace(
                "Adding {0} to {1} cache repositories with TTL of {2:g}",
                key,
                _repositories.Count,
                ttl
            );
            foreach (var repository in _repositories.Values)
            {
                repository.Set(value, key, ttl);
            }
        }

        /// <summary>
        /// Adds a value to a specific cache repository by type.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
        /// <typeparam name="TValue">The type of the value to cache.</typeparam>
        /// <param name="value">The value to cache.</param>
        /// <param name="key">The key under which to store the value.</param>
        /// <exception cref="InvalidOperationException">The repository of type {type.FullName} isn't available in the repositories providers list.</exception>
        public static void SetTo<TCacheRepository, TValue>(
            TValue value,
            [Localizable(false)] string key
        )
        {
            var type = typeof(TCacheRepository);
            LogConsumer.Trace("Adding {0} to repository of type {1}", key, type.FullName);
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }

            repository.Set(value, key);
        }

        /// <summary>
        /// Adds a value to a specific cache repository by type.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
        /// <typeparam name="TValue">The type of the value to cache.</typeparam>
        /// <param name="value">The value to cache.</param>
        /// <param name="key">The key under which to store the value.</param>
        /// <param name="subKey">The sub key for additional categorization.</param>
        /// <exception cref="InvalidOperationException">The repository of type {type.FullName} isn't available in the repositories providers list.</exception>
        public static void SetTo<TCacheRepository, TValue>(
            TValue value,
            [Localizable(false)] string key,
            [Localizable(false)] string subKey
        )
        {
            var type = typeof(TCacheRepository);
            LogConsumer.Trace(
                "Adding {0}/{2} to repository of type {1}",
                key,
                type.FullName,
                subKey
            );
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }

            repository.Set(value, key, subKey);
        }

        /// <summary>
        /// Adds a value to a specific cache repository by type.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
        /// <typeparam name="TValue">The type of the value to cache.</typeparam>
        /// <param name="value">The value to cache.</param>
        /// <param name="key">The key under which to store the value.</param>
        /// <param name="ttl">The time-to-live for this key.</param>
        /// <exception cref="InvalidOperationException">The repository of type {type.FullName} isn't available in the repositories providers list.</exception>
        public static void SetTo<TCacheRepository, TValue>(
            TValue value,
            [Localizable(false)] string key,
            TimeSpan ttl
        )
        {
            var type = typeof(TCacheRepository);
            LogConsumer.Trace(
                "Adding {0} to repository of type {1} with TTL of {2:g}",
                key,
                type.FullName,
                ttl
            );
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }

            repository.Set(value, key, ttl);
        }

        /// <summary>
        /// Adds a value to a specific cache repository by type.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
        /// <typeparam name="TValue">The type of the value to cache.</typeparam>
        /// <param name="value">The value to cache.</param>
        /// <param name="key">The key under which to store the value.</param>
        /// <param name="subKey">The sub key of the cached value.</param>
        /// <param name="ttl">The time-to-live for this key.</param>
        /// <exception cref="InvalidOperationException">The repository of type {type.FullName} isn't available in the repositories providers list.</exception>
        public static void SetTo<TCacheRepository, TValue>(
            TValue value,
            [Localizable(false)] string key,
            [Localizable(false)] string subKey,
            TimeSpan ttl
        )
        {
            var type = typeof(TCacheRepository);
            LogConsumer.Trace(
                "Adding {0}/{2} to repository of type {1} with TTL of {2:g}",
                key,
                type.FullName,
                ttl,
                subKey
            );
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }

            repository.Set(value, key, subKey);
        }

        /// <summary>
        /// Retrieves a cached value by key from all repositories.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="key">The key of the cached value.</param>
        /// <returns>The retrieved value.</returns>
        /// <exception cref="InvalidOperationException">Unable to get the item with key {key}.</exception>
        public static T Get<T>([Localizable(false)] string key)
        {
            LogConsumer.Trace(
                "Getting {0} from any of {1} cache repositories",
                key,
                _repositories.Count
            );
            foreach (var repository in _repositories.Values)
            {
                if (!repository.TryGet(key, out T value))
                {
                    continue;
                }

                if (
                    _isMemoryRepositoryInList
                    && repository.GetType() != typeof(MemoryCacheRepository)
                )
                {
                    SetTo<MemoryCacheRepository, T>(value, key);
                }

                return value;
            }

            throw new InvalidOperationException($"Unable to get the item with key {key}");
        }

        /// <summary>
        /// Retrieves a cached value by key from all repositories.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="key">The key of the cached value.</param>
        /// <param name="subKey">The sub key of the cached value.</param>
        /// <returns>The retrieved value.</returns>
        /// <exception cref="InvalidOperationException">Unable to get the item with key {key} and sub key {subKey}.</exception>
        public static T Get<T>([Localizable(false)] string key, [Localizable(false)] string subKey)
        {
            LogConsumer.Trace(
                "Getting {0}/{2} from any of {1} cache repositories",
                key,
                _repositories.Count,
                subKey
            );
            foreach (var repository in _repositories.Values)
            {
                if (!repository.TryGet(key, subKey, out T value))
                {
                    continue;
                }

                if (
                    _isMemoryRepositoryInList
                    && repository.GetType() != typeof(MemoryCacheRepository)
                )
                {
                    SetTo<MemoryCacheRepository, T>(value, key, subKey);
                }

                return value;
            }

            throw new InvalidOperationException(
                $"Unable to get the item with key {key} and sub key {subKey}"
            );
        }

        /// <summary>
        /// Retrieves a cached value by key from a specific repository type.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository to retrieve the value from.</typeparam>
        /// <typeparam name="TValue">The type of the value to retrieve.</typeparam>
        /// <param name="key">The key of the cached value.</param>
        /// <returns>The retrieved value.</returns>
        /// <exception cref="InvalidOperationException">The repository of type {type.FullName} isn't available in the repositories providers list.</exception>
        public static TValue GetFrom<TCacheRepository, TValue>([Localizable(false)] string key)
        {
            var type = typeof(TCacheRepository);
            LogConsumer.Trace("Getting {0} from repository {1}", key, type.FullName);
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }

            return repository.Get<TValue>(key);
        }

        /// <summary>
        /// Retrieves a cached value by key from a specific repository type.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository to retrieve the value from.</typeparam>
        /// <typeparam name="TValue">The type of the value to retrieve.</typeparam>
        /// <param name="key">The key of the cached value.</param>
        /// <param name="subKey">The sub key of the cached value.</param>
        /// <returns>The retrieved value.</returns>
        /// <exception cref="InvalidOperationException">The repository of type {type.FullName} isn't available in the repositories providers list.</exception>
        public static TValue GetFrom<TCacheRepository, TValue>(
            [Localizable(false)] string key,
            [Localizable(false)] string subKey
        )
        {
            var type = typeof(TCacheRepository);
            LogConsumer.Trace("Getting {0}/{2} from repository {1}", key, type.FullName, subKey);
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }

            return repository.Get<TValue>(key, subKey);
        }

        /// <summary>
        /// Attempts to retrieve a cached value by key.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="key">The key of the cached value.</param>
        /// <param name="value">The retrieved value, if found.</param>
        /// <returns><c>true</c> if the value was found; otherwise, <c>false</c>.</returns>
        public static bool TryGet<T>([Localizable(false)] string key, out T value)
        {
            LogConsumer.Trace(
                "Trying to get {0} from any of {1} repositories",
                key,
                _repositories.Count
            );
            value = default;
            foreach (var repository in _repositories.Values)
            {
                if (!repository.TryGet(key, out value))
                {
                    continue;
                }

                if (
                    _isMemoryRepositoryInList
                    && repository.GetType() != typeof(MemoryCacheRepository)
                )
                {
                    SetTo<MemoryCacheRepository, T>(value, key);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to retrieve a cached value by key.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="key">The key of the cached value.</param>
        /// <param name="subKey">The sub key of the cached value.</param>
        /// <param name="value">The retrieved value, if found.</param>
        /// <returns><c>true</c> if the value was found; otherwise, <c>false</c>.</returns>
        public static bool TryGet<T>(
            [Localizable(false)] string key,
            [Localizable(false)] string subKey,
            out T value
        )
        {
            LogConsumer.Trace(
                "Trying to get {0}/{2} from any of {1} repositories",
                key,
                _repositories.Count,
                subKey
            );
            value = default;
            foreach (var repository in _repositories.Values)
            {
                if (!repository.TryGet(key, subKey, out value))
                {
                    continue;
                }

                if (
                    _isMemoryRepositoryInList
                    && repository.GetType() != typeof(MemoryCacheRepository)
                )
                {
                    SetTo<MemoryCacheRepository, T>(value, key, subKey);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to retrieve the time-to-live (TTL) of a cached value by key.
        /// </summary>
        /// <param name="key">The key of the cached value.</param>
        /// <returns>The TTL of the cached value, or <c>TimeSpan.Zero</c> if not found.</returns>
        public static TimeSpan TTL([Localizable(false)] string key)
        {
            LogConsumer.Trace(
                "Trying to get TTL of key {0} from {1} repositories",
                key,
                _repositories.Count
            );
            var result = new TimeSpan(0);
            foreach (var repository in _repositories.Values)
            {
                var currentResult = repository.TTL(key);
                if (currentResult == result)
                {
                    continue;
                }

                return currentResult;
            }

            return new TimeSpan(0);
        }

        /// <summary>
        /// Removes a cached value by key from all repositories.
        /// </summary>
        /// <param name="key">The key of the cached value to remove.</param>
        public static void Remove([Localizable(false)] string key)
        {
            LogConsumer.Trace("Removing key {0} from {1} repositories", key, _repositories.Count);
            foreach (var repository in _repositories.Values)
            {
                repository.Remove(key);
            }
        }

        /// <summary>
        /// Removes a cached value by key and sub key from all repositories.
        /// </summary>
        /// <param name="key">The key of the cached value to remove.</param>
        /// <param name="subKey">The sub key of the cached value to remove.</param>
        public static void Remove(
            [Localizable(false)] string key,
            [Localizable(false)] string subKey
        )
        {
            LogConsumer.Trace(
                "Removing key {0} and sub key {2} from {1} repositories",
                key,
                _repositories.Count,
                subKey
            );
            foreach (var repository in _repositories.Values)
            {
                repository.Remove(key, subKey);
            }
        }

        /// <summary>
        /// Attempts to remove a value by key and sub key.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
        /// <param name="key">The key of the cached value.</param>
        /// <exception cref="InvalidOperationException">The repository of type {type.FullName} isn't available in the repositories providers list.</exception>
        public static void RemoveFrom<TCacheRepository>([Localizable(false)] string key)
        {
            var type = typeof(TCacheRepository);
            LogConsumer.Trace("Removing key {0} from {1} repository", key, _repositories.Count);
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }

            repository.Remove(key);
        }

        /// <summary>
        /// Attempts to remove a value by key and sub key.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
        /// <param name="key">The key of the cached value.</param>
        /// <param name="subKey">The sub key of the cached value to remove.</param>
        /// <exception cref="InvalidOperationException">The repository of type {type.FullName} isn't available in the repositories providers list.</exception>
        public static void RemoveFrom<TCacheRepository>(
            [Localizable(false)] string key,
            [Localizable(false)] string subKey
        )
        {
            var type = typeof(TCacheRepository);
            LogConsumer.Trace(
                "Removing key {0} and sub key {2} from {1} repository",
                key,
                _repositories.Count,
                subKey
            );
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }

            repository.Remove(key, subKey);
        }
    }
}
