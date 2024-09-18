using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CrispyWaffle.Composition;
using CrispyWaffle.Log;
using Newtonsoft.Json.Linq;

namespace CrispyWaffle.Cache
{
    /// <summary>
    /// The cache manager class.
    /// </summary>
    public static class CacheManager
    {
        /// <summary>
        /// The repositories
        /// </summary>
        private static readonly SortedList<int, ICacheRepository> _repositories =
            new SortedList<int, ICacheRepository>();

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

        /// <summary>
        /// Adds the repository.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the i cache repository.</typeparam>
        /// <returns></returns>
        public async static Task<ICacheRepository> AddRepositoryAsync<TCacheRepository>()
            where TCacheRepository : ICacheRepository
        {
            var repository = await Task.Run(() => ServiceLocator.Resolve<TCacheRepository>());
            await AddRepositoryAsync(repository, _currentPriority++);
            return repository;
        }

        /// <summary>
        /// Adds the repository to the repositories providers list.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns></returns>
        public async static Task<ICacheRepository> AddRepositoryAsync(ICacheRepository repository)
        {
            await AddRepositoryAsync(repository, _currentPriority++);
            return repository;
        }

        /// <summary>
        /// Adds the repository.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the i cache repository.</typeparam>
        /// <param name="priority">The priority.</param>
        /// <returns></returns>
        public async static Task<ICacheRepository> AddRepositoryAsync<TCacheRepository>(
            int priority
        )
            where TCacheRepository : ICacheRepository
        {
            var repository = await Task.Run(() => ServiceLocator.Resolve<TCacheRepository>());
            await AddRepositoryAsync(repository, priority);
            return repository;
        }

        /// <summary>
        /// Adds the repository with the defined priority.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="priority">The priority.</param>
        /// <returns>Returns <see cref="int"/> that represents the priority with the repository was added</returns>
        public async static Task<int> AddRepositoryAsync(ICacheRepository repository, int priority)
        {
            while (true)
            {
                if (!_repositories.ContainsKey(priority))
                {
                    _repositories.Add(priority, repository);
                    await Task.Run(
                        () =>
                            LogConsumer.Trace(
                                "Adding cache repository of type {0} with priority {1}",
                                repository.GetType().FullName,
                                priority
                            )
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
        /// Stores the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        public async static Task SetAsync<T>(T value, [Localizable(false)] string key)
        {
            await Task.Run(
                () =>
                    LogConsumer.Trace(
                        "Adding {0} to {1} cache repositories",
                        key,
                        _repositories.Count
                    )
            );
            foreach (var repository in _repositories.Values)
            {
                await repository.SetAsync(value, key);
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        public async static Task SetAsync<T>(
            T value,
            [Localizable(false)] string key,
            [Localizable(false)] string subKey
        )
        {
            await Task.Run(
                () =>
                    LogConsumer.Trace(
                        "Adding {0}/{2} to {1} cache repositories",
                        key,
                        _repositories.Count,
                        subKey
                    )
            );
            foreach (var repository in _repositories.Values)
            {
                await repository.SetAsync(value, key, subKey);
            }
        }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        /// <param name="ttl">The TTL.</param>
        public async static Task SetAsync<T>(T value, [Localizable(false)] string key, TimeSpan ttl)
        {
            await Task.Run(
                () =>
                    LogConsumer.Trace(
                        "Adding {0} to {1} cache repositories with TTL of {2:g}",
                        key,
                        _repositories.Count,
                        ttl
                    )
            );
            foreach (var repository in _repositories.Values)
            {
                await repository.SetAsync(value, key, ttl);
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
        public async static Task SetToAsync<TCacheRepository, TValue>(
            TValue value,
            [Localizable(false)] string key
        )
        {
            var type = typeof(TCacheRepository);
            await Task.Run(
                () => LogConsumer.Trace("Adding {0} to repository of type {1}", key, type.FullName)
            );
            var repository = await Task.Run(
                () => _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value
            );
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }

            await repository.SetAsync(value, key);
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
        public async static Task SetToAsync<TCacheRepository, TValue>(
            TValue value,
            [Localizable(false)] string key,
            [Localizable(false)] string subKey
        )
        {
            var type = typeof(TCacheRepository);
            await Task.Run(
                () =>
                    LogConsumer.Trace(
                        "Adding {0}/{2} to repository of type {1}",
                        key,
                        type.FullName,
                        subKey
                    )
            );
            var repository = await Task.Run(
                () => _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value
            );
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }

            await repository.SetAsync(value, key, subKey);
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
        public async static Task SetToAsync<TCacheRepository, TValue>(
            TValue value,
            [Localizable(false)] string key,
            TimeSpan ttl
        )
        {
            var type = typeof(TCacheRepository);
            await Task.Run(
                () =>
                    LogConsumer.Trace(
                        "Adding {0} to repository of type {1} with TTL of {2:g}",
                        key,
                        type.FullName,
                        ttl
                    )
            );
            var repository = await Task.Run(
                () => _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value
            );
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }

            await repository.SetAsync(value, key, ttl);
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
        public async static Task SetToAsync<TCacheRepository, TValue>(
            TValue value,
            [Localizable(false)] string key,
            [Localizable(false)] string subKey,
            TimeSpan ttl
        )
        {
            var type = typeof(TCacheRepository);
            await Task.Run(
                () =>
                    LogConsumer.Trace(
                        "Adding {0}/{2} to repository of type {1} with TTL of {2:g}",
                        key,
                        type.FullName,
                        ttl,
                        subKey
                    )
            );
            var repository = await Task.Run(
                () => _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value
            );
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }

            await repository.SetAsync(value, key, subKey);
        }

        /// <summary>
        /// Gets the object with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of object (the object will be cast to this type)</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The object as <typeparamref name="T"/></returns>
        /// <exception cref="InvalidOperationException">Throws when the object with the specified key doesn't exists</exception>
        public async static Task<T> GetAsync<T>([Localizable(false)] string key)
        {
            await Task.Run(
                () =>
                    LogConsumer.Trace(
                        "Getting {0} from any of {1} cache repositories",
                        key,
                        _repositories.Count
                    )
            );
            T value;
            foreach (var repository in _repositories.Values)
            {
                var result = await repository.TryGetAsync<T>(key);
                value = result.value;
                if (!result.Exists)
                {
                    continue;
                }

                if (
                    _isMemoryRepositoryInList
                    && repository.GetType() != typeof(MemoryCacheRepository)
                )
                {
                    await SetToAsync<MemoryCacheRepository, T>(value, key);
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
        public async static Task<T> GetAsync<T>(
            [Localizable(false)] string key,
            [Localizable(false)] string subKey
        )
        {
            await Task.Run(
                () =>
                    LogConsumer.Trace(
                        "Getting {0}/{2} from any of {1} cache repositories",
                        key,
                        _repositories.Count,
                        subKey
                    )
            );
            T value;
            foreach (var repository in _repositories.Values)
            {
                var result = await repository.TryGetAsync<T>(key);
                value = result.value;
                if (!result.Exists)
                {
                    continue;
                }

                if (
                    _isMemoryRepositoryInList
                    && repository.GetType() != typeof(MemoryCacheRepository)
                )
                {
                    await SetToAsync<MemoryCacheRepository, T>(value, key, subKey);
                }

                return value;
            }

            throw new InvalidOperationException(
                $"Unable to get the item with key {key} and sub key {subKey}"
            );
        }

        /// <summary>
        ///  Gets the object with the specified key from the specified repository type, if the repository is in the repositories providers list.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The <typeparamref name="TValue"/></returns>
        /// <exception cref="InvalidOperationException">Throws when the repository is not in the repositories providers list or the key isn't available at that repository</exception>
        public async static Task<TValue> GetFromAsync<TCacheRepository, TValue>(
            [Localizable(false)] string key
        )
        {
            var type = typeof(TCacheRepository);
            await Task.Run(
                () => LogConsumer.Trace("Getting {0} from repository {1}", key, type.FullName)
            );
            var repository = await Task.Run(
                () => _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value
            );
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }
            var returnValue = await repository.GetAsync<TValue>(key);
            return returnValue;
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
        public async static Task<TValue> GetFromAsync<TCacheRepository, TValue>(
            [Localizable(false)] string key,
            [Localizable(false)] string subKey
        )
        {
            var type = typeof(TCacheRepository);
            await Task.Run(
                () =>
                    LogConsumer.Trace(
                        "Getting {0}/{2} from repository {1}",
                        key,
                        type.FullName,
                        subKey
                    )
            );
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }
            var returnValue = await repository.GetAsync<TValue>(key, subKey);
            return returnValue;
        }

        /// <summary>
        /// Tries to get a value based on its key, if exists in any repository return true, else false.
        /// The out parameter value is the object requested.
        /// </summary>
        /// <typeparam name="T">The type of object (the object will be cast to this type)</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// /// <returns>A <see cref="Tuple"/> contating <see cref="bool"/> Exists that contains the success info of the get, and <typeparamref name="T"/> value which is the value.
        /// <br/>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async static Task<(bool Exists, T Value)> TryGetAsync<T>(
            [Localizable(false)] string key
        )
        {
            await Task.Run(
                () =>
                    LogConsumer.Trace(
                        "Trying to get {0} from any of {1} repositories",
                        key,
                        _repositories.Count
                    )
            );
            T value = default;
            foreach (var repository in _repositories.Values)
            {
                var result = await repository.TryGetAsync<T>(key);
                value = result.value;
                if (!result.Exists)
                {
                    continue;
                }

                if (
                    _isMemoryRepositoryInList
                    && repository.GetType() != typeof(MemoryCacheRepository)
                )
                {
                    await SetToAsync<MemoryCacheRepository, T>(value, key);
                }

                return (true, value);
            }

            return (false, value);
        }

        /// <summary>
        /// Tries the get.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="Tuple"/> contating <see cref="bool"/> Exists that contains the success info of the get, and <typeparamref name="T"/> value which is the value.
        /// <br/>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async static Task<(bool Exists, T Value)> TryGetAsync<T>(
            [Localizable(false)] string key,
            [Localizable(false)] string subKey
        )
        {
            LogConsumer.Trace(
                "Trying to get {0}/{2} from any of {1} repositories",
                key,
                _repositories.Count,
                subKey
            );
            T value = default;
            foreach (var repository in _repositories.Values)
            {
                var result = await repository.TryGetAsync<T>(key, subKey);
                value = result.value;
                if (!result.Exists)
                {
                    continue;
                }

                if (
                    _isMemoryRepositoryInList
                    && repository.GetType() != typeof(MemoryCacheRepository)
                )
                {
                    await SetToAsync<MemoryCacheRepository, T>(value, key, subKey);
                }

                return (true, value);
            }

            return (false, value);
        }

        /// <summary>
        /// TTLs the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public async static Task<TimeSpan> TTLAsync([Localizable(false)] string key)
        {
            await Task.Run(
                () =>
                    LogConsumer.Trace(
                        "Trying to get TTL of key {0} from {1} repositories",
                        key,
                        _repositories.Count
                    )
            );
            var result = new TimeSpan(0);
            foreach (var repository in _repositories.Values)
            {
                var currentResult = await repository.TTLAsync(key);
                if (currentResult == result)
                {
                    continue;
                }

                return currentResult;
            }

            return new TimeSpan(0);
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public async static Task RemoveAsync([Localizable(false)] string key)
        {
            await Task.Run(
                () =>
                    LogConsumer.Trace(
                        "Removing key {0} from {1} repositories",
                        key,
                        _repositories.Count
                    )
            );
            foreach (var repository in _repositories.Values)
            {
                await repository.RemoveAsync(key);
            }
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        public async static Task RemoveAsync(
            [Localizable(false)] string key,
            [Localizable(false)] string subKey
        )
        {
            await Task.Run(
                () =>
                    LogConsumer.Trace(
                        "Removing key {0} and sub key {2} from {1} repositories",
                        key,
                        _repositories.Count,
                        subKey
                    )
            );
            foreach (var repository in _repositories.Values)
            {
                await repository.RemoveAsync(key, subKey);
            }
        }

        /// <summary>
        /// Removes from.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
        /// <param name="key">The key.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public async static Task RemoveFromAsync<TCacheRepository>([Localizable(false)] string key)
        {
            var type = typeof(TCacheRepository);
            await Task.Run(
                () =>
                    LogConsumer.Trace(
                        "Removing key {0} from {1} repository",
                        key,
                        _repositories.Count
                    )
            );
            var repository = await Task.Run(
                () => _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value
            );
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }

            await repository.RemoveAsync(key);
        }

        /// <summary>
        /// Removes from.
        /// </summary>
        /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="subKey">The sub key.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public async static Task RemoveFromAsync<TCacheRepository>(
            [Localizable(false)] string key,
            [Localizable(false)] string subKey
        )
        {
            var type = typeof(TCacheRepository);
            await Task.Run(
                () =>
                    LogConsumer.Trace(
                        "Removing key {0} and sub key {2} from {1} repository",
                        key,
                        _repositories.Count,
                        subKey
                    )
            );
            var repository = await Task.Run(
                () => _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value
            );
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }

            await repository.RemoveAsync(key, subKey);
        }
    }
}
