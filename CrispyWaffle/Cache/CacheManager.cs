namespace CrispyWaffle.Cache
{
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
        private static readonly SortedList<int, ICacheRepository> Repositories;

        /// <summary>
        /// The current priority
        /// </summary>
        private static int _currentPriority;

        /// <summary>
        /// The memory type
        /// </summary>
        private static readonly Type MemoryType;

        /// <summary>
        /// The is memory repository in list
        /// </summary>
        private static bool _isMemoryRepositoryInList;

        #endregion

        #region ~Ctor

        /// <summary>
        /// Initializes the <see cref="CacheManager"/> class.
        /// </summary>
        static CacheManager()
        {
            Repositories = new SortedList<int, ICacheRepository>();
            MemoryType = typeof(MemoryCacheRepository);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds the repository.
        /// </summary>
        /// <typeparam name="TICacheRepository">The type of the i cache repository.</typeparam>
        /// <returns></returns>
        public static ICacheRepository AddRepository<TICacheRepository>() where TICacheRepository : ICacheRepository
        {
            var repository = ServiceLocator.Resolve<TICacheRepository>();
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
        /// <typeparam name="TICacheRepository">The type of the i cache repository.</typeparam>
        /// <param name="priority">The priority.</param>
        /// <returns></returns>
        public static ICacheRepository AddRepository<TICacheRepository>(int priority)
            where TICacheRepository : ICacheRepository
        {
            var repository = ServiceLocator.Resolve<TICacheRepository>();
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
                if (!Repositories.ContainsKey(priority))
                {
                    Repositories.Add(priority, repository);
                    LogConsumer.Trace(Resources.CacheManager_AddRepository, repository.GetType().FullName, priority);
                    if (repository.GetType() == MemoryType)
                        _isMemoryRepositoryInList = true;
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
            LogConsumer.Trace(Resources.CacheManager_Set, key, Repositories.Count);
            foreach (var repository in Repositories.Values)
                repository.Set(value, key);
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
            LogConsumer.Trace(Resources.CacheManager_SetSub, key, Repositories.Count, subKey);
            foreach (var repository in Repositories.Values)
                repository.Set(value, key, subKey);
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
            LogConsumer.Trace(Resources.CacheManager_Set_TTL, key, Repositories.Count, ttl);
            foreach (var repository in Repositories.Values)
                repository.Set(value, key, ttl);
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
            LogConsumer.Trace(Resources.CacheManager_SetTo, key, type.FullName);
            var repository = Repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
                throw new InvalidOperationException(string.Format(Resources.CacheManager_RepositoryUnavailableInProvidersList, type.FullName));
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
            LogConsumer.Trace(Resources.CacheManager_SetSubTo, key, type.FullName, subKey);
            var repository = Repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
                throw new InvalidOperationException(string.Format(Resources.CacheManager_RepositoryUnavailableInProvidersList, type.FullName));
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
            LogConsumer.Trace(Resources.CacheManager_SetTo_TTL, key, type.FullName, ttl);
            var repository = Repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
                throw new InvalidOperationException(string.Format(Resources.CacheManager_RepositoryUnavailableInProvidersList, type.FullName));
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
            LogConsumer.Trace(Resources.CacheManager_SetSubTo_TTL, key, type.FullName, ttl, subKey);
            var repository = Repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
                throw new InvalidOperationException(string.Format(Resources.CacheManager_RepositoryUnavailableInProvidersList, type.FullName));
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
            LogConsumer.Trace(Resources.CacheManager_Get, key, Repositories.Count);
            foreach (var repository in Repositories.Values)
            {
                if (!repository.TryGet(key, out T value))
                    continue;
                if (_isMemoryRepositoryInList && repository.GetType() != typeof(MemoryCacheRepository))
                    SetTo<MemoryCacheRepository, T>(value, key);
                return value;
            }
            throw new InvalidOperationException(string.Format(Resources.CacheManager_Get_UnableToGetKey, key));
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
            LogConsumer.Trace(Resources.CacheManager_GetSub, key, Repositories.Count, subKey);
            foreach (var repository in Repositories.Values)
            {
                if (!repository.TryGet(key, subKey, out T value))
                    continue;
                if (_isMemoryRepositoryInList && repository.GetType() != typeof(MemoryCacheRepository))
                    SetTo<MemoryCacheRepository, T>(value, key, subKey);
                return value;
            }
            throw new InvalidOperationException(string.Format(Resources.CacheManager_GetSub_UnableToGetKey, key, subKey));
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
            LogConsumer.Trace(Resources.CacheManager_GetFrom, key, type.FullName);
            var repository = Repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
                throw new InvalidOperationException(string.Format(Resources.CacheManager_RepositoryUnavailableInProvidersList, type.FullName));
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
            LogConsumer.Trace(Resources.CacheManager_GetSubFrom, key, type.FullName, subKey);
            var repository = Repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
                throw new InvalidOperationException(string.Format(Resources.CacheManager_RepositoryUnavailableInProvidersList, type.FullName));
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
            LogConsumer.Trace(Resources.CacheManager_TryGet, key, Repositories.Count);
            value = default;
            foreach (var repository in Repositories.Values)
            {
                if (!repository.TryGet(key, out value))
                    continue;
                if (_isMemoryRepositoryInList && repository.GetType() != typeof(MemoryCacheRepository))
                    SetTo<MemoryCacheRepository, T>(value, key);
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
            LogConsumer.Trace(Resources.CacheManager_TryGetSub, key, Repositories.Count, subKey);
            value = default;
            foreach (var repository in Repositories.Values)
            {
                if (!repository.TryGet(key, subKey, out value))
                    continue;
                if (_isMemoryRepositoryInList && repository.GetType() != typeof(MemoryCacheRepository))
                    SetTo<MemoryCacheRepository, T>(value, key, subKey);
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
            LogConsumer.Trace(Resources.CacheManager_TTL, key, Repositories.Count);
            var result = new TimeSpan(0);
            foreach (var repository in Repositories.Values)
            {
                var currentResult = repository.TTL(key);
                if (currentResult == result)
                    continue;
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
            LogConsumer.Trace(Resources.CacheManager_Remove, key, Repositories.Count);
            foreach (var repository in Repositories.Values)
                repository.Remove(key);
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        public static void Remove([Localizable(false)] string key, [Localizable(false)]string subKey)
        {
            LogConsumer.Trace(Resources.CacheManager_RemoveSub, key, Repositories.Count, subKey);
            foreach (var repository in Repositories.Values)
                repository.Remove(key, subKey);
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
            LogConsumer.Trace(Resources.CacheManager_RemoveFrom, key, Repositories.Count);
            var repository = Repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
                throw new InvalidOperationException(string.Format(Resources.CacheManager_RepositoryUnavailableInProvidersList, type.FullName));
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
            LogConsumer.Trace(Resources.CacheManager_RemoveSubFrom, key, Repositories.Count, subKey);
            var repository = Repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
                throw new InvalidOperationException(string.Format(Resources.CacheManager_RepositoryUnavailableInProvidersList, type.FullName));
            repository.Remove(key, subKey);
        }

        #endregion

        #endregion
    }
}
