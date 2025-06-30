using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using CrispyWaffle.Composition;
using CrispyWaffle.Log;
using Newtonsoft.Json.Linq;

namespace CrispyWaffle.Cache;

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
    /// Default timeout.
    /// </summary>
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

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
    /// <param name="cancellationToken">Cancel.</param>
    /// <exception cref="OperationCanceledException">If cancelled.</exception>
    /// <returns>void.</returns>
    public static async ValueTask SetAsync<T>(T value, [Localizable(false)] string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        LogConsumer.Trace("Adding {0} to {1} cache repositories", key, _repositories.Count);

        var tasks = _repositories.Values.Select(async repository =>
        {
            try
            {
                await repository.SetAsync(value, key, cancellationToken: cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Cache operation was cancelled");
                return false;
            }
            catch (Exception ex)
            {
                LogConsumer.Error("Failed to set {0} in repository {1}: {2}", key, repository.GetType().Name, ex.Message);
                return false;
            }
        });

        var results = await Task.WhenAll(tasks);
        var successCount = results.Count(r => r);

        LogConsumer.Info("Successfully set {0} in {1} out of {2} repositories", key, successCount, _repositories.Count);
    }

    /// <summary>
    /// Adds a value with a sub key to all cache repositories.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="value">The value to cache.</param>
    /// <param name="key">The key under which to store the value.</param>
    /// <param name="subKey">The sub key for additional categorization.</param>
    /// <param name="cancellationToken">Cancel.</param>
    /// <exception cref="OperationCanceledException">If cancelled.</exception>
    /// <returns>void.</returns>
    public static async ValueTask SetAsync<T>(
        T value,
        [Localizable(false)] string key,
        [Localizable(false)] string subKey,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        LogConsumer.Trace(
            "Adding {0}/{2} to {1} cache repositories",
            key,
            _repositories.Count,
            subKey
        );

        var tasks = _repositories.Values.Select(async repository =>
        {
            try
            {
                await repository.SetAsync(value, key, subKey, cancellationToken: cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Cache operation was cancelled");
                return false;
            }
            catch (Exception ex)
            {
                LogConsumer.Error("Failed to set {0} in repository {1}: {2}", key, repository.GetType().Name, ex.Message);
                return false;
            }
        });

        var results = await Task.WhenAll(tasks);
        var successCount = results.Count(r => r);

        LogConsumer.Info("Successfully set {0} in {1} out of {2} repositories", key, successCount, _repositories.Count);
    }

    /// <summary>
    /// Adds a value with a time-to-live (TTL) to all cache repositories.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="value">The value to cache.</param>
    /// <param name="key">The key under which to store the value.</param>
    /// <param name="ttl">The time-to-live for the cached value.</param>
    /// <param name="cancellationToken">Cancel.</param>
    /// <exception cref="OperationCanceledException">If cancelled.</exception>
    /// <returns>void.</returns>
    public static async ValueTask SetAsync<T>(T value, [Localizable(false)] string key, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        LogConsumer.Trace(
            "Adding {0} to {1} cache repositories with TTL of {2:g}",
            key,
            _repositories.Count,
            ttl
        );
        var tasks = _repositories.Values.Select(async repository =>
        {
            try
            {
                await repository.SetAsync(value, key, ttl, cancellationToken: cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Cache operation was cancelled");
                return false;
            }
            catch (Exception ex)
            {
                LogConsumer.Error("Failed to set {0} in repository {1}: {2}", key, repository.GetType().Name, ex.Message);
                return false;
            }
        });

        var results = await Task.WhenAll(tasks);
        var successCount = results.Count(r => r);

        LogConsumer.Info("Successfully set {0} in {1} out of {2} repositories", key, successCount, _repositories.Count);
    }

    /// <summary>
    /// Adds a value to a specific cache repository by type.
    /// </summary>
    /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
    /// <typeparam name="TValue">The type of the value to cache.</typeparam>
    /// <param name="value">The value to cache.</param>
    /// <param name="key">The key under which to store the value.</param>
    /// <param name="cancellationToken">Cancel.</param>
    /// <exception cref="InvalidOperationException">The repository of type {type.FullName} isn't available in the repositories providers list.</exception>
    /// <exception cref="OperationCanceledException">If cancelled.</exception>
    /// <returns>void.</returns>
    public static async ValueTask SetToAsync<TCacheRepository, TValue>(
        TValue value,
        [Localizable(false)] string key,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var type = typeof(TCacheRepository);
        LogConsumer.Trace("Adding {0} to repository of type {1}", key, type.FullName);
        var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
        if (repository == null)
        {
            throw new InvalidOperationException(
                $"The repository of type {type.FullName} isn't available in the repositories providers list"
            );
        }

        try
        {
            await repository.SetAsync(value, key, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cache operation was cancelled");
        }
        catch (Exception ex)
        {
            LogConsumer.Error("Failed to set {0} in repository {1}: {2}", key, repository.GetType().Name, ex.Message);
        }
    }

    /// <summary>
    /// Adds a value to a specific cache repository by type.
    /// </summary>
    /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
    /// <typeparam name="TValue">The type of the value to cache.</typeparam>
    /// <param name="value">The value to cache.</param>
    /// <param name="key">The key under which to store the value.</param>
    /// <param name="subKey">The sub key for additional categorization.</param>
    /// <param name="cancellationToken">Cancel.</param>
    /// <exception cref="InvalidOperationException">The repository of type {type.FullName} isn't available in the repositories providers list.</exception>
    /// <exception cref="OperationCanceledException">If cancelled.</exception>
    /// <returns>void.</returns>
    public static async ValueTask SetToAsync<TCacheRepository, TValue>(
        TValue value,
        [Localizable(false)] string key,
        [Localizable(false)] string subKey,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var type = typeof(TCacheRepository);
        LogConsumer.Trace("Adding {0}/{2} to repository of type {1}", key, type.FullName, subKey);
        var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
        if (repository == null)
        {
            throw new InvalidOperationException(
                $"The repository of type {type.FullName} isn't available in the repositories providers list"
            );
        }

        try
        {
            await repository.SetAsync(value, key, subKey, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cache operation was cancelled");
        }
        catch (Exception ex)
        {
            LogConsumer.Error("Failed to set {0} in repository {1}: {2}", key, repository.GetType().Name, ex.Message);
        }
    }

    /// <summary>
    /// Adds a value to a specific cache repository by type.
    /// </summary>
    /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
    /// <typeparam name="TValue">The type of the value to cache.</typeparam>
    /// <param name="value">The value to cache.</param>
    /// <param name="key">The key under which to store the value.</param>
    /// <param name="ttl">The time-to-live for this key.</param>
    /// <param name="cancellationToken">Cancel.</param>
    /// <exception cref="InvalidOperationException">The repository of type {type.FullName} isn't available in the repositories providers list.</exception>
    /// <exception cref="OperationCanceledException">If cancelled.</exception>
    /// <returns>void.</returns>
    public static async ValueTask SetToAsync<TCacheRepository, TValue>(
        TValue value,
        [Localizable(false)] string key,
        TimeSpan ttl,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

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

        try
        {
            await repository.SetAsync(value, key, ttl, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cache operation was cancelled");
        }
        catch (Exception ex)
        {
            LogConsumer.Error("Failed to set {0} in repository {1}: {2}", key, repository.GetType().Name, ex.Message);
        }
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
    /// <param name="cancellationToken">Cancel.</param>
    /// <exception cref="InvalidOperationException">The repository of type {type.FullName} isn't available in the repositories providers list.</exception>
    /// <exception cref="OperationCanceledException">If cancelled.</exception>
    /// <returns>void.</returns>
    public static async ValueTask SetToAsync<TCacheRepository, TValue>(
        TValue value,
        [Localizable(false)] string key,
        [Localizable(false)] string subKey,
        TimeSpan ttl,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

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

        try
        {
            await repository.SetAsync(value, key, subKey, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cache operation was cancelled");
        }
        catch (Exception ex)
        {
            LogConsumer.Error("Failed to set {0} in repository {1}: {2}", key, repository.GetType().Name, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a cached value by key from all repositories.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key of the cached value.</param>
    /// <param name="cancellationToken"> Cancellation token.</param>
    /// <returns>The retrieved value.</returns>
    /// <exception cref="InvalidOperationException">Unable to get the item with key {key}.</exception>
    /// <exception cref="OperationCanceledException">Operation cancelled.</exception>
    public static async ValueTask<T> GetAsync<T>([Localizable(false)] string key, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            LogConsumer.Trace(
                "Getting {0} from any of {1} cache repositories",
                key,
                _repositories.Count
            );

            // Create timeout for the entire operation
            using var timeoutCts = new CancellationTokenSource(DefaultTimeout);
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var errors = new List<(string Repository, Exception Error)>();

            foreach (var repository in _repositories.Values)
            {
                try
                {
                    combinedCts.Token.ThrowIfCancellationRequested();

                    var repositoryName = repository.GetType().Name;
                    var repositoryStopwatch = Stopwatch.StartNew();

                    LogConsumer.Debug("Attempting to get key {0} from repository {1}", key, repositoryName);

                    // Try to get value from repository
                    var result = await repository.GetAsync<T>(key, combinedCts.Token);

                    repositoryStopwatch.Stop();
                    LogConsumer.Info("Found key {0} in repository {1}", key, repositoryName);

                    // If found in non-memory repository, promote to memory cache
                    if (_isMemoryRepositoryInList && repository.GetType() != typeof(MemoryCacheRepository))
                    {
                        _ = Task.Run(
                            async () =>
                        {
                            try
                            {
                                await SetToAsync<MemoryCacheRepository, T>(result, key, CancellationToken.None);
                                LogConsumer.Debug("Promoted key {0} to memory cache", key);
                            }
                            catch (Exception ex)
                            {
                                LogConsumer.Error("Failed to promote key {0} to memory cache: {1}", key, ex.Message);
                            }
                        }, cancellationToken);
                    }

                    LogConsumer.Debug("Key {0} not found in repository {1} ({2}ms)",  key, repositoryName, repositoryStopwatch.ElapsedMilliseconds);
                    return result;
                }
                catch (InvalidOperationException)
                {
                    throw new InvalidOperationException($"Unable to get the item with key {key}");
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // User cancelled - stop immediately
                    LogConsumer.Info("Get operation cancelled by user for key {0}", key);
                    throw;
                }
                catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
                {
                    // Timeout occurred
                    LogConsumer.Error("Get operation timed out for key {0}", key);
                    throw new TimeoutException($"Get operation timed out for key {key}");
                }
                catch (Exception ex)
                {
                    var repositoryName = repository.GetType().Name;
                    errors.Add((repositoryName, ex));

                    LogConsumer.Error("Error getting key {0} from repository {1}: {2}", key, repositoryName, ex.Message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cache operation was cancelled");
            throw;
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
    public static async ValueTask<T> GetAsync<T>([Localizable(false)] string key, [Localizable(false)] string subKey, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            LogConsumer.Trace("Getting {0}/{2} from any of {1} cache repositories", key, _repositories.Count, subKey);

            // Create timeout for the entire operation
            using var timeoutCts = new CancellationTokenSource(DefaultTimeout);
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var errors = new List<(string Repository, Exception Error)>();

            foreach (var repository in _repositories.Values)
            {
                try
                {
                    combinedCts.Token.ThrowIfCancellationRequested();

                    var repositoryName = repository.GetType().Name;
                    var repositoryStopwatch = Stopwatch.StartNew();

                    LogConsumer.Debug("Attempting to get key {0} from repository {1}", key, repositoryName);

                    // Try to get value from repository
                    var result = await repository.GetAsync<T>(key, subKey,  combinedCts.Token);

                    repositoryStopwatch.Stop();
                    LogConsumer.Info("Found key {0} in repository {1}", key, repositoryName);

                    // If found in non-memory repository, promote to memory cache
                    if (_isMemoryRepositoryInList && repository.GetType() != typeof(MemoryCacheRepository))
                    {
                        _ = Task.Run(
                            async () =>
                            {
                                try
                                {
                                    await SetToAsync<MemoryCacheRepository, T>(result, key, subKey, CancellationToken.None);
                                    LogConsumer.Debug("Promoted key {0} to memory cache", key);
                                }
                                catch (Exception ex)
                                {
                                    LogConsumer.Error("Failed to promote key {0} to memory cache: {1}", key, ex.Message);
                                }
                            }, cancellationToken);
                    }

                    LogConsumer.Debug("Key {0} not found in repository {1} ({2}ms)", key, repositoryName, repositoryStopwatch.ElapsedMilliseconds);
                    return result;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // User cancelled - stop immediately
                    LogConsumer.Info("Get operation cancelled by user for key {0}", key);
                    throw;
                }
                catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
                {
                    // Timeout occurred
                    LogConsumer.Error("Get operation timed out for key {0}", key);
                    throw new TimeoutException($"Get operation timed out for key {key}");
                }
                catch (Exception ex)
                {
                    var repositoryName = repository.GetType().Name;
                    errors.Add((repositoryName, ex));

                    LogConsumer.Error("Error getting key {0} from repository {1}: {2}", key, repositoryName, ex.Message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cache operation was cancelled");
            throw;
        }

        throw new InvalidOperationException($"Unable to get the item with key {key}");
    }

    /// <summary>
    /// Retrieves a cached value by key from a specific repository type.
    /// </summary>
    /// <typeparam name="TCacheRepository">The type of the cache repository to retrieve the value from.</typeparam>
    /// <typeparam name="TValue">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key of the cached value.</param>
    /// <param name="cancellationToken">Cancelation token.</param>
    /// <returns>The retrieved value.</returns>
    /// <exception cref="InvalidOperationException">The repository of type {type.FullName} isn't available in the repositories providers list.</exception>
    public static async ValueTask<TValue> GetFrom<TCacheRepository, TValue>([Localizable(false)] string key, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var type = typeof(TCacheRepository);

            LogConsumer.Trace("Getting {0} from repository {1}", key, type.FullName);
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }

            // Create timeout for the operation
            using var timeoutCts = new CancellationTokenSource(DefaultTimeout);
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var result = await repository.GetAsync<TValue>(key, combinedCts.Token);

            return result;
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            LogConsumer.Info("Get operation cancelled by user for key {0} from {1}", key, typeof(TCacheRepository).Name);
            throw;
        }
        catch (OperationCanceledException)
        {
            var error = $"Get operation timed out for key {key} from {typeof(TCacheRepository).Name}";
            LogConsumer.Error(error);
            throw new TimeoutException(error);
        }
    }

    /// <summary>
    /// Retrieves a cached value by key from a specific repository type.
    /// </summary>
    /// <typeparam name="TCacheRepository">The type of the cache repository to retrieve the value from.</typeparam>
    /// <typeparam name="TValue">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key of the cached value.</param>
    /// <param name="subKey">The sub key of the cached value.</param>
    /// <param name="cancellationToken">Cancelation token.</param>
    /// <returns>The retrieved value.</returns>
    /// <exception cref="InvalidOperationException">The repository of type {type.FullName} isn't available in the repositories providers list.</exception>
    /// <exception cref="OperationCanceledException">Operation cancelled.</exception>
    public static async ValueTask<TValue> GetFromAsync<TCacheRepository, TValue>(
        [Localizable(false)] string key,
        [Localizable(false)] string subKey,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var type = typeof(TCacheRepository);

            LogConsumer.Trace("Getting {0} from repository {1}", key, type.FullName);
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }

            // Create timeout for the operation
            using var timeoutCts = new CancellationTokenSource(DefaultTimeout);
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var result = await repository.GetAsync<TValue>(key, subKey, combinedCts.Token);

            return result;
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            LogConsumer.Info("Get operation cancelled by user for key {0} from {1}", key, typeof(TCacheRepository).Name);
            throw;
        }
        catch (OperationCanceledException)
        {
            var error = $"Get operation timed out for key {key} from {typeof(TCacheRepository).Name}";
            LogConsumer.Error(error);
            throw new TimeoutException(error);
        }
    }

    /// <summary>
    /// Attempts to retrieve a cached value by key.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key of the cached value.</param>
    /// <param name="cancellationToken">Cancelation token.</param>
    /// <exception cref="OperationCanceledException">Operation cancelled.</exception>
    /// <returns>
    /// A tuple containing Success (true if found and castable to T) and Value (the cached item or default(T)).
    /// </returns>
    public static async ValueTask<(bool Success, T Value)> TryGetAsync<T>([Localizable(false)] string key, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            LogConsumer.Trace("Trying to get {0} from any of {1} repositories",key,_repositories.Count);

            // Create timeout for the entire operation
            using var timeoutCts = new CancellationTokenSource(DefaultTimeout);
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var errors = new List<(string Repository, Exception Error)>();

            foreach (var repository in _repositories.Values)
            {
                try
                {
                    combinedCts.Token.ThrowIfCancellationRequested();

                    var repositoryName = repository.GetType().Name;
                    var repositoryStopwatch = Stopwatch.StartNew();

                    LogConsumer.Debug("Attempting to get key {0} from repository {1}", key, repositoryName);

                    // Try to get value from repository
                    var result = await repository.GetAsync<T>(key, combinedCts.Token);

                    repositoryStopwatch.Stop();
                    LogConsumer.Info("Found key {0} in repository {1}", key, repositoryName);

                    // If found in non-memory repository, promote to memory cache
                    if (_isMemoryRepositoryInList && repository.GetType() != typeof(MemoryCacheRepository))
                    {
                        _ = Task.Run(
                            async () =>
                            {
                                try
                                {
                                    await SetToAsync<MemoryCacheRepository, T>(result, key, CancellationToken.None);
                                    LogConsumer.Debug("Promoted key {0} to memory cache", key);
                                }
                                catch (Exception ex)
                                {
                                    LogConsumer.Error("Failed to promote key {0} to memory cache: {1}", key, ex.Message);
                                }
                            }, cancellationToken);
                    }

                    LogConsumer.Debug("Key {0} not found in repository {1} ({2}ms)", key, repositoryName, repositoryStopwatch.ElapsedMilliseconds);
                    return (true, result);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // User cancelled - stop immediately
                    LogConsumer.Info("Get operation cancelled by user for key {0}", key);
                    throw;
                }
                catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
                {
                    // Timeout occurred
                    LogConsumer.Error("Get operation timed out for key {0}", key);
                    throw new TimeoutException($"Get operation timed out for key {key}");
                }
                catch (Exception ex)
                {
                    var repositoryName = repository.GetType().Name;
                    errors.Add((repositoryName, ex));

                    LogConsumer.Error("Error getting key {0} from repository {1}: {2}", key, repositoryName, ex.Message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cache operation was cancelled");
            throw;
        }

        return (false, default(T));
    }

    /// <summary>
    /// Attempts to retrieve a cached value by key.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key of the cached value.</param>
    /// <param name="subKey">The sub key of the cached value.</param>
    /// <param name="cancellationToken">Cancel token.</param>
    /// <returns><c>true</c> if the value was found; otherwise, <c>false</c>.</returns>
    public static async ValueTask<bool> TryGetAsync<T>(
        [Localizable(false)] string key,
        [Localizable(false)] string subKey,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            LogConsumer.Trace("Trying to get {0}/{2} from any of {1} repositories", key, _repositories.Count, subKey);

            // Create timeout for the entire operation
            using var timeoutCts = new CancellationTokenSource(DefaultTimeout);
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var errors = new List<(string Repository, Exception Error)>();

            foreach (var repository in _repositories.Values)
            {
                try
                {
                    combinedCts.Token.ThrowIfCancellationRequested();

                    var repositoryName = repository.GetType().Name;
                    var repositoryStopwatch = Stopwatch.StartNew();

                    LogConsumer.Debug("Attempting to get key {0} from repository {1}", key, repositoryName);

                    // Try to get value from repository
                    var result = await repository.GetAsync<T>(key, subKey, combinedCts.Token);

                    repositoryStopwatch.Stop();
                    LogConsumer.Info("Found key {0} in repository {1}", key, repositoryName);

                    // If found in non-memory repository, promote to memory cache
                    if (_isMemoryRepositoryInList && repository.GetType() != typeof(MemoryCacheRepository))
                    {
                        _ = Task.Run(
                            async () =>
                            {
                                try
                                {
                                    await SetToAsync<MemoryCacheRepository, T>(result, key, subKey, CancellationToken.None);
                                    LogConsumer.Debug("Promoted key {0} to memory cache", key);
                                }
                                catch (Exception ex)
                                {
                                    LogConsumer.Error("Failed to promote key {0} to memory cache: {1}", key, ex.Message);
                                }
                            }, cancellationToken);
                    }

                    LogConsumer.Debug("Key {0} not found in repository {1} ({2}ms)", key, repositoryName, repositoryStopwatch.ElapsedMilliseconds);
                    return true;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // User cancelled - stop immediately
                    LogConsumer.Info("Get operation cancelled by user for key {0}", key);
                    throw;
                }
                catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
                {
                    // Timeout occurred
                    LogConsumer.Error("Get operation timed out for key {0}", key);
                    throw new TimeoutException($"Get operation timed out for key {key}");
                }
                catch (Exception ex)
                {
                    var repositoryName = repository.GetType().Name;
                    errors.Add((repositoryName, ex));

                    LogConsumer.Error("Error getting key {0} from repository {1}: {2}", key, repositoryName, ex.Message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cache operation was cancelled");
            throw;
        }

        return false;
    }

    /// <summary>
    /// Attempts to retrieve the time-to-live (TTL) of a cached value by key.
    /// </summary>
    /// <param name="key">The key of the cached value.</param>
    /// <returns>The TTL of the cached value, or <c>TimeSpan.Zero</c> if not found.</returns>
    public static async Task<TimeSpan> TTLAsync([Localizable(false)] string key)
    {
        LogConsumer.Trace(
            "Trying to get TTL of key {0} from {1} repositories",
            key,
            _repositories.Count
        );
        TimeSpan result = new TimeSpan(0);
        foreach (var repository in _repositories.Values)
        {
            TimeSpan currentResult = await repository.TTLAsync(key);
            if (currentResult == result)
            {
                continue;
            }

            return currentResult;
        }

        return result; // Default TTL if not found in any repository
    }

    /// <summary>
    /// Removes a cached value by key from all repositories.
    /// </summary>
    /// <param name="key">The key of the cached value to remove.</param>
    /// <param name="cancellationToken">Cancel token.</param>
    /// <returns>Completed task.</returns>
    public static async ValueTask Remove([Localizable(false)] string key, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            LogConsumer.Trace("Removing key {0} from {1} repositories", key, _repositories.Count);

            // Create timeout for the entire operation
            using var timeoutCts = new CancellationTokenSource(DefaultTimeout);
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var errors = new List<(string Repository, Exception Error)>();

            foreach (var repository in _repositories.Values)
            {
                try
                {
                    combinedCts.Token.ThrowIfCancellationRequested();

                    var repositoryName = repository.GetType().Name;

                    LogConsumer.Debug("Attempting to delete key {0} from repository {1}", key, repositoryName);

                    // Try to remove value from repository
                    await repository.RemoveAsync(key, combinedCts.Token);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // User cancelled - stop immediately
                    LogConsumer.Info("Get operation cancelled by user for key {0}", key);
                    throw;
                }
                catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
                {
                    // Timeout occurred
                    LogConsumer.Error("Get operation timed out for key {0}", key);
                    throw new TimeoutException($"Get operation timed out for key {key}");
                }
                catch (Exception ex)
                {
                    var repositoryName = repository.GetType().Name;
                    errors.Add((repositoryName, ex));
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cache operation was cancelled");
            throw;
        }
    }

    /// <summary>
    /// Removes a cached value by key and sub key from all repositories.
    /// </summary>
    /// <param name="key">The key of the cached value to remove.</param>
    /// <param name="subKey">The sub key of the cached value to remove.</param>
    /// <param name="cancellationToken">Cancel token.</param>
    /// <returns>Completed task.</returns>
    public static async ValueTask Remove([Localizable(false)] string key, [Localizable(false)] string subKey, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            LogConsumer.Trace("Removing key {0} and sub key {2} from {1} repositories", key ,_repositories.Count, subKey);

            // Create timeout for the entire operation
            using var timeoutCts = new CancellationTokenSource(DefaultTimeout);
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var errors = new List<(string Repository, Exception Error)>();

            foreach (var repository in _repositories.Values)
            {
                try
                {
                    combinedCts.Token.ThrowIfCancellationRequested();

                    var repositoryName = repository.GetType().Name;

                    LogConsumer.Debug("Attempting to delete key {0} from repository {1}", key, repositoryName);

                    // Try to remove value from repository
                    await repository.RemoveAsync(key, subKey, combinedCts.Token);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // User cancelled - stop immediately
                    LogConsumer.Info("Get operation cancelled by user for key {0}", key);
                    throw;
                }
                catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
                {
                    // Timeout occurred
                    LogConsumer.Error("Get operation timed out for key {0}", key);
                    throw new TimeoutException($"Get operation timed out for key {key}");
                }
                catch (Exception ex)
                {
                    var repositoryName = repository.GetType().Name;
                    errors.Add((repositoryName, ex));
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cache operation was cancelled");
            throw;
        }
    }

    /// <summary>
    /// Attempts to remove a value by key and sub key.
    /// </summary>
    /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
    /// <param name="key">The key of the cached value.</param>
    /// <param name="cancellationToken">Cancel token.</param>
    /// <exception cref="InvalidOperationException">The repository of type {type.FullName} isn't available in the repositories providers list.</exception>
    /// <exception cref="OperationCanceledException">The operation get cancelled.</exception>
    /// <returns>Completed task.</returns>
    public static async ValueTask RemoveFrom<TCacheRepository>([Localizable(false)] string key, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var type = typeof(TCacheRepository);

            LogConsumer.Trace("Removing key {0} from {1} repository", key, _repositories.Count);
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }

            // Create timeout for the operation
            using var timeoutCts = new CancellationTokenSource(DefaultTimeout);
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            await repository.RemoveAsync(key, combinedCts.Token);
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            LogConsumer.Info("Get operation cancelled by user for key {0} from {1}", key, typeof(TCacheRepository).Name);
            throw;
        }
        catch (OperationCanceledException)
        {
            var error = $"Get operation timed out for key {key} from {typeof(TCacheRepository).Name}";
            LogConsumer.Error(error);
            throw new TimeoutException(error);
        }
    }

    /// <summary>
    /// Attempts to remove a value by key and sub key.
    /// </summary>
    /// <typeparam name="TCacheRepository">The type of the cache repository.</typeparam>
    /// <param name="key">The key of the cached value.</param>
    /// <param name="subKey">The sub key of the cached value to remove.</param>
    /// <param name="cancellationToken">Cancel token.</param>
    /// <exception cref="InvalidOperationException">The repository of type {type.FullName} isn't available in the repositories providers list.</exception>
    /// <exception cref="OperationCanceledException">The operation get cancelled.</exception>
    /// <returns>Completed task.</returns>
    public static async ValueTask RemoveFrom<TCacheRepository>(
        [Localizable(false)] string key,
        [Localizable(false)] string subKey,
        CancellationToken cancellationToken = default
    )
    {

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var type = typeof(TCacheRepository);

            LogConsumer.Trace("Removing key {0} and sub key {2} from {1} repository", key, _repositories.Count, subKey);
            var repository = _repositories.SingleOrDefault(r => type == r.Value.GetType()).Value;
            if (repository == null)
            {
                throw new InvalidOperationException(
                    $"The repository of type {type.FullName} isn't available in the repositories providers list"
                );
            }

            // Create timeout for the operation
            using var timeoutCts = new CancellationTokenSource(DefaultTimeout);
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            await repository.RemoveAsync(key, subKey, combinedCts.Token);
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            LogConsumer.Info("Get operation cancelled by user for key {0} from {1}", key, typeof(TCacheRepository).Name);
            throw;
        }
        catch (OperationCanceledException)
        {
            var error = $"Get operation timed out for key {key} from {typeof(TCacheRepository).Name}";
            LogConsumer.Error(error);
            throw new TimeoutException(error);
        }
    }
}
