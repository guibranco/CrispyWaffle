namespace CrispyWaffle.Cache
{
    using Composition;
    using Log;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// The cache manager class.
    /// </summary>
    public static class CacheManager
    {
        #region Private fields

        /// <summary>
        /// The repositories
        /// </summary>
        private static readonly SortedList<int, ICacheRepository> _repositories = new SortedList<int, ICacheRepository>();

        /// <summary>
        /// The current priority
        /// </summary>
        private static int _currentPriority;

        /// <summary>
        /// The memory type
        /// </summary>
        private static readonly Type _memoryType = typeof(MemoryCacheRepository);

        /// <summary>
        /// The is memory repository in list
        /// </summary>
        private static bool _isMemoryRepositoryInList;

        #endregion

        #region Public methods

        /// <summary>
        /// Adds the repository.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the i cache repository.</typeparam>
        /// <returns></returns>
        public static ICacheRepository AddRepository<TCacheRepository>() where TCacheRepository : ICacheRepository
        {
            var repository = ServiceLocator.Resolve<TCacheRepository>();
            AddRepository(repository, _currentPriority++);
            return repository;
        }


        /// <summary>
        /// Adds the repository to the repositories providers list.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns></returns>
        public static ICacheRepository AddRepository(ICacheRepository repository)
        {
            AddRepository(repository, _currentPriority++);
            return repository;
        }

        /// <summary>
        /// Adds the repository.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the i cache repository.</typeparam>
        /// <param name="priority">The priority.</param>
        /// <returns></returns>
        public static ICacheRepository AddRepository<TCacheRepository>(int priority)
            where TCacheRepository : ICacheRepository
        {
            var repository = ServiceLocator.Resolve<TCacheRepository>();
            AddRepository(repository, priority);
            return repository;
        }

        /// <summary>
        /// Adds the repository with the defined priority.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="priority">The priority.</param>
        /// <returns>Returns the priority with the repository was added</returns>
        public static int AddRepository(ICacheRepository repository, int priority)
        {
            while (true)
            {
                if (!_repositories.ContainsKey(priority))
                {
                    _repositories.Add(priority, repository);
                    LogConsumer.Trace("Adding cache repository of type {0} with priority {1}", repository.GetType().FullName, priority);
                    if (repository.GetType() == _memoryType)
                    {
                        _isMemoryRepositoryInList = true;
                    }

                    return priority;
                }
                priority++;
            }
        }

        #region Set

        /// <summary>
        /// Stores the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        public static void Set<T>(T value, [Localizable(false)]string key)
        {
            LogConsumer.Trace("Adding {0} to {1} cache repositories", key, _repositories.Count);
            foreach (var repository in _repositories.Values)
            {
                repository.Set(value, key);
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        public static void Set<T>(T value, [Localizable(false)] string key, [Localizable(false)] string subKey)
        {
            LogConsumer.Trace("Adding {0}/{2} to {1} cache repositories", key, _repositories.Count, subKey);
            foreach (var repository in _repositories.Values)
            {
                repository.Set(value, key, subKey);
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        /// <param name="ttl">The TTL.</param>
        public static void Set<T>(T value, [Localizable(false)] string key, TimeSpan ttl)
        {
            LogConsumer.Trace("Adding {0} to {1} cache repositories with TTL of {2:g}", key, _repositories.Count, ttl);
            foreach (var repository in _repositories.Values)
            {
                repository.Set(value, key, ttl);
            }
        }

        /// <summary>
        /// Sets to.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void SetTo<TCacheRepository, TValue>(TValue value, [Localizable(false)]string key)
        {
            var type = typeof(TCacheRepository);
            LogConsumer.Trace("Adding {0} to repository of type {1}", key, type.FullName);
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException($"The repository of type {type.FullName} isn't available in the repositories providers list");
            }

            repository.Set(value, key);
        }

        /// <summary>
        /// Sets to.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void SetTo<TCacheRepository, TValue>(TValue value, [Localizable(false)]string key, [Localizable(false)]string subKey)
        {
            var type = typeof(TCacheRepository);
            LogConsumer.Trace("Adding {0}/{2} to repository of type {1}", key, type.FullName, subKey);
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException($"The repository of type {type.FullName} isn't available in the repositories providers list");
            }

            repository.Set(value, key, subKey);
        }

        /// <summary>
        /// Sets to.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        /// <param name="ttl">The TTL.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void SetTo<TCacheRepository, TValue>(TValue value, [Localizable(false)]string key, TimeSpan ttl)
        {
            var type = typeof(TCacheRepository);
            LogConsumer.Trace("Adding {0} to repository of type {1} with TTL of {2:g}", key, type.FullName, ttl);
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException($"The repository of type {type.FullName} isn't available in the repositories providers list");
            }

            repository.Set(value, key, ttl);
        }

        /// <summary>
        /// Sets to.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        /// <param name="ttl">The TTL.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void SetTo<TCacheRepository, TValue>(TValue value, [Localizable(false)]string key, [Localizable(false)]string subKey, TimeSpan ttl)
        {
            var type = typeof(TCacheRepository);
            LogConsumer.Trace("Adding {0}/{2} to repository of type {1} with TTL of {2:g}", key, type.FullName, ttl, subKey);
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException($"The repository of type {type.FullName} isn't available in the repositories providers list");
            }

            repository.Set(value, key, subKey);
        }

        #endregion

        #region Get

        /// <summary>
        /// Gets the object with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of object (the object will be cast to this type)</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The object as <typeparamref name="T"/></returns>
        /// <exception cref="InvalidOperationException">Throws when the object with the specified key doesn't exists</exception>
        public static T Get<T>([Localizable(false)]string key)
        {
            LogConsumer.Trace("Getting {0} from any of {1} cache repositories", key, _repositories.Count);
            foreach (var repository in _repositories.Values)
            {
                if (!repository.TryGet(key, out T value))
                {
                    continue;
                }

                if (_isMemoryRepositoryInList && repository.GetType() != typeof(MemoryCacheRepository))
                {
                    SetTo<MemoryCacheRepository, T>(value, key);
                }

                return value;
            }
            throw new InvalidOperationException($"Unable to get the item with key {key}");
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static T Get<T>([Localizable(false)]string key, [Localizable(false)] string subKey)
        {
            LogConsumer.Trace("Getting {0}/{2} from any of {1} cache repositories", key, _repositories.Count, subKey);
            foreach (var repository in _repositories.Values)
            {
                if (!repository.TryGet(key, subKey, out T value))
                {
                    continue;
                }

                if (_isMemoryRepositoryInList && repository.GetType() != typeof(MemoryCacheRepository))
                {
                    SetTo<MemoryCacheRepository, T>(value, key, subKey);
                }

                return value;
            }
            throw new InvalidOperationException($"Unable to get the item with key {key} and sub key {subKey}");
        }

        /// <summary>
        ///  Gets the object with the specified key from the specified repository type, if the repository is in the repositories providers list.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The <typeparamref name="TValue"/></returns>
        /// <exception cref="InvalidOperationException">Throws when the repository is not in the repositories providers list or the key isn't available at that repository</exception>
        public static TValue GetFrom<TCacheRepository, TValue>([Localizable(false)]string key)
        {
            var type = typeof(TCacheRepository);
            LogConsumer.Trace("Getting {0} from repository {1}", key, type.FullName);
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException($"The repository of type {type.FullName} isn't available in the repositories providers list");
            }

            return repository.Get<TValue>(key);
        }

        /// <summary>
        /// Gets from.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static TValue GetFrom<TCacheRepository, TValue>([Localizable(false)]string key, [Localizable(false)]string subKey)
        {
            var type = typeof(TCacheRepository);
            LogConsumer.Trace("Getting {0}/{2} from repository {1}", key, type.FullName, subKey);
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException($"The repository of type {type.FullName} isn't available in the repositories providers list");
            }

            return repository.Get<TValue>(key, subKey);
        }

        #endregion

        #region Try get

        /// <summary>
        /// Tries to get a value based on its key, if exists in any repository return true, else false. 
        /// The out parameter value is the object requested.
        /// </summary>
        /// <typeparam name="T">The type of object (the object will be cast to this type)</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>Returns <b>True</b> if the object with the key exists, false otherwise</returns>
        public static bool TryGet<T>([Localizable(false)]string key, out T value)
        {
            LogConsumer.Trace("Trying to get {0} from any of {1} repositories", key, _repositories.Count);
            value = default;
            foreach (var repository in _repositories.Values)
            {
                if (!repository.TryGet(key, out value))
                {
                    continue;
                }

                if (_isMemoryRepositoryInList && repository.GetType() != typeof(MemoryCacheRepository))
                {
                    SetTo<MemoryCacheRepository, T>(value, key);
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries the get.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool TryGet<T>([Localizable(false)]string key, [Localizable(false)]string subKey, out T value)
        {
            LogConsumer.Trace("Trying to get {0}/{2} from any of {1} repositories", key, _repositories.Count, subKey);
            value = default;
            foreach (var repository in _repositories.Values)
            {
                if (!repository.TryGet(key, subKey, out value))
                {
                    continue;
                }

                if (_isMemoryRepositoryInList && repository.GetType() != typeof(MemoryCacheRepository))
                {
                    SetTo<MemoryCacheRepository, T>(value, key, subKey);
                }

                return true;
            }
            return false;
        }

        #endregion

        #region TTL

        /// <summary>
        /// TTLs the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static TimeSpan TTL([Localizable(false)] string key)
        {
            LogConsumer.Trace("Trying to get TTL of key {0} from {1} repositories", key, _repositories.Count);
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

        #endregion

        #region Remove

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public static void Remove([Localizable(false)] string key)
        {
            LogConsumer.Trace("Removing key {0} from {1} repositories", key, _repositories.Count);
            foreach (var repository in _repositories.Values)
            {
                repository.Remove(key);
            }
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        public static void Remove([Localizable(false)] string key, [Localizable(false)]string subKey)
        {
            LogConsumer.Trace("Removing key {0} and sub key {2} from {1} repositories", key, _repositories.Count, subKey);
            foreach (var repository in _repositories.Values)
            {
                repository.Remove(key, subKey);
            }
        }

        /// <summary>
        /// Removes from.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
        /// <param name="key">The key.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void RemoveFrom<TCacheRepository>([Localizable(false)] string key)
        {
            var type = typeof(TCacheRepository);
            LogConsumer.Trace("Removing key {0} from {1} repository", key, _repositories.Count);
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException($"The repository of type {type.FullName} isn't available in the repositories providers list");
            }

            repository.Remove(key);
        }

        /// <summary>
        /// Removes from.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void RemoveFrom<TCacheRepository>([Localizable(false)] string key, [Localizable(false)]string subKey)
        {
            var type = typeof(TCacheRepository);
            LogConsumer.Trace("Removing key {0} and sub key {2} from {1} repository", key, _repositories.Count, subKey);
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException($"The repository of type {type.FullName} isn't available in the repositories providers list");
            }

            repository.Remove(key, subKey);
        }

        #endregion

        #endregion
    }
}
