using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using CrispyWaffle.Extensions;
using CrispyWaffle.Log;
using CrispyWaffle.Telemetry;

namespace CrispyWaffle.Composition
{
    /// <summary>
    /// The service locator class.
    /// </summary>
    public static class ServiceLocator
    {
        /// <summary>
        /// The locks.
        /// </summary>
        private static readonly ConcurrentDictionary<string, object> _locks = new();

        /// <summary>
        /// The registrations calls log.
        /// </summary>
        private static readonly Dictionary<Type, int> _registrationsCalls = new();

        /// <summary>
        /// The dictionary holding the types and its implementations.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Func<object>> _registrations = new();

        /// <summary>
        /// The dictionary holding the types and its dependency resolver implementations.
        /// </summary>
        private static readonly Dictionary<Type, IDependencyResolver> _dependenciesResolvers =
            new();

        /// <summary>
        /// The dictionary holding the types and its dependencies as a cache system for new
        /// instances (auto registration).
        /// </summary>
        private static readonly Dictionary<Type, Type[]> _dependenciesCache = new();

        /// <summary>
        /// The instances resolved cache.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, List<Type>> _instances = new();

        /// <summary>
        /// The cancellation token source.
        /// </summary>
        private static readonly CancellationTokenSource _cancellationTokenSource = new();

        /// <summary>
        /// The not loaded assemblies.
        /// </summary>
        private static readonly Dictionary<string, Exception> _notLoadedAssemblies = new();

#pragma warning disable S1144 // Unused private types or members should be removed
        private static readonly Destructor _finalise = new();
#pragma warning restore S1144 // Unused private types or members should be removed

        /// <summary>
        /// Initializes static members of the <see cref="ServiceLocator"/> class.
        /// </summary>
        static ServiceLocator()
        {
            LoadMissingAssemblies();
            TypesCache = AppDomain
                .CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(a => a != null)
                .ToList();
            var cancellationToken = typeof(CancellationToken);
            _registrationsCalls.Add(cancellationToken, 0);
            _registrations.AddOrUpdate(
                cancellationToken,
                () => _cancellationTokenSource.Token,
                (_, existingValue) => () => existingValue
            );
        }

        /// <summary>
        /// Loads the missing assemblies.
        /// </summary>
        private static void LoadMissingAssemblies()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var missingAssemblies = loadedAssemblies
                .SelectMany(x => x.GetReferencedAssemblies())
                .Distinct()
                .Where(y => loadedAssemblies.All(a => a.FullName != y.FullName))
                .ToList();
            missingAssemblies.ForEach(LoadMissingAssembly);
        }

        /// <summary>
        /// Loads the missing assembly.
        /// </summary>
        /// <param name="missingAssembly">The missing assembly.</param>
        private static void LoadMissingAssembly(AssemblyName missingAssembly)
        {
            try
            {
                AppDomain.CurrentDomain.Load(missingAssembly);
            }
            catch (Exception e)
            {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                _notLoadedAssemblies.TryAdd(missingAssembly.FullName, e);
#else
                if (!_notLoadedAssemblies.ContainsKey(missingAssembly.FullName))
                {
                    _notLoadedAssemblies.Add(missingAssembly.FullName, e);
                }
#endif
            }
        }

        /// <summary>
        /// Registers the life styled internal.
        /// </summary>
        /// <param name="lifestyle">The lifestyle.</param>
        /// <param name="contract">The contract.</param>
        /// <param name="implementation">The implementation.</param>
        private static void RegisterLifeStyledInternal(
            LifeStyle lifestyle,
            Type contract,
            Type implementation
        )
        {
            _registrationsCalls.Add(contract, 0);

            if (lifestyle == LifeStyle.Transient)
            {
                RegisterTransientInternal(contract, implementation);
                return;
            }

            if (implementation.Implements<IDisposable>())
            {
                RegisterDisposableInternal(contract, implementation);
                return;
            }

            RegisterSingletonInternal(contract, implementation);
        }

        /// <summary>
        /// Registers the singleton internal.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="implementation">The implementation.</param>
        private static void RegisterSingletonInternal(Type contract, Type implementation)
        {
            var lazy = new Lazy<object>(() => CreateInstance(implementation));
            _registrations.AddOrUpdate(
                contract,
                () =>
                {
                    _registrationsCalls[contract]++;
                    lock (_locks.GetOrAdd(
                        implementation.FullName ?? implementation.Name,
                        new object()
                    ))
                    {
                        return lazy.Value;
                    }
                },
                (_, existingValue) => () => existingValue
            );
        }

        /// <summary>
        /// Registers the lifestyle creator internal.
        /// </summary>
        /// <typeparam name="TContract">The type of the contract.</typeparam>
        /// <param name="lifestyle">The lifestyle.</param>
        /// <param name="instanceCreator">The instance creator.</param>
        private static void RegisterLifestyleCreatorInternal<TContract>(
            LifeStyle lifestyle,
            Func<TContract> instanceCreator
        )
        {
            var contract = typeof(TContract);
            _registrationsCalls.Add(contract, 0);

            if (lifestyle == LifeStyle.Transient)
            {
                RegisterTransientInternal(instanceCreator, contract);
                return;
            }

            if (contract.Implements<IDisposable>())
            {
                RegisterDisposableInternal(instanceCreator, contract);
                return;
            }

            RegisterSingleton(instanceCreator, contract);
        }

        /// <summary>
        /// Registers the singleton.
        /// </summary>
        /// <typeparam name="TContract">The type of the t contract.</typeparam>
        /// <param name="instanceCreator">The instance creator.</param>
        /// <param name="contract">The contract.</param>
        private static void RegisterSingleton<TContract>(
            Func<TContract> instanceCreator,
            Type contract
        )
        {
            var lazy = new Lazy<object>(() => instanceCreator());
            _registrations.AddOrUpdate(
                contract,
                () =>
                {
                    _registrationsCalls[contract]++;
                    lock (_locks.GetOrAdd(contract.FullName ?? contract.Name, new object()))
                    {
                        return lazy.Value;
                    }
                },
                (_, existingValue) => () => existingValue
            );
        }

        /// <summary>
        /// Registers the disposable internal.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="implementation">The implementation.</param>
        private static void RegisterDisposableInternal(Type contract, Type implementation)
        {
            var lazyDisposable = new LazyDisposable<IDisposable>(
                () => (IDisposable)CreateInstance(implementation)
            );
            _registrations.AddOrUpdate(
                contract,
                () =>
                {
                    _registrationsCalls[contract]++;
                    lock (_locks.GetOrAdd(
                        implementation.FullName ?? implementation.Name,
                        new object()
                    ))
                    {
                        return lazyDisposable.Value;
                    }
                },
                (_, existingValue) => () => existingValue
            );
        }

        /// <summary>
        /// Registers the disposable internal.
        /// </summary>
        /// <typeparam name="TContract">The type of the t contract.</typeparam>
        /// <param name="instanceCreator">The instance creator.</param>
        /// <param name="contract">The contract.</param>
        private static void RegisterDisposableInternal<TContract>(
            Func<TContract> instanceCreator,
            Type contract
        )
        {
            var lazyDisposable = new LazyDisposable<IDisposable>(
                () => (IDisposable)instanceCreator()
            );
            _registrations.AddOrUpdate(
                contract,
                () =>
                {
                    _registrationsCalls[contract]++;
                    lock (_locks.GetOrAdd(contract.FullName ?? contract.Name, new object()))
                    {
                        return lazyDisposable.Value;
                    }
                },
                (_, existingValue) => () => existingValue
            );
        }

        /// <summary>
        /// Registers the transient internal.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="implementation">The implementation.</param>
        private static void RegisterTransientInternal(Type contract, Type implementation)
        {
            _registrations.AddOrUpdate(
                contract,
                () =>
                {
                    _registrationsCalls[contract]++;
                    return GetInstance(implementation);
                },
                (_, existingValue) => () => existingValue
            );
        }

        /// <summary>
        /// Registers the transient internal.
        /// </summary>
        /// <typeparam name="TContract">The type of the t contract.</typeparam>
        /// <param name="instanceCreator">The instance creator.</param>
        /// <param name="contract">The contract.</param>
        private static void RegisterTransientInternal<TContract>(
            Func<TContract> instanceCreator,
            Type contract
        )
        {
            _registrations.AddOrUpdate(
                contract,
                () =>
                {
                    _registrationsCalls[contract]++;
                    return instanceCreator();
                },
                (_, existingValue) => () => existingValue
            );
        }

        /// <summary>
        /// Gets all instances.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <returns>IEnumerable&lt;System.Object&gt;.</returns>
        private static IEnumerable<object> GetAllInstances(Type contract)
        {
            if (!_instances.ContainsKey(contract))
            {
                lock (_locks.GetOrAdd(contract.FullName ?? contract.Name, new object()))
                {
                    if (!_instances.ContainsKey(contract))
                    {
                        _instances.TryAdd(
                            contract,
                            TypesCache
                                .Where(t => contract.IsAssignableFrom(t) && !t.IsAbstract)
                                .ToList()
                        );
                    }
                }
            }

            if (!_instances.TryGetValue(contract, out var types))
            {
                yield break;
            }

            foreach (var type in types)
            {
                yield return GetInstance(type);
            }
        }

        /// <summary>
        /// Get the instance of a contract based on where it is requested (parent contract) using a
        /// dependency resolver if any available for the type to get a context for the dependency.
        /// </summary>
        /// <param name="contract">The interface looking implementation.</param>
        /// <param name="parentContract">
        /// The interface where the contract is requested and requires a context.
        /// </param>
        /// <param name="order">
        /// The order of the <paramref name="contract"></paramref> in the <paramref
        /// name="parentContract"/> arguments list.
        /// </param>
        /// <returns>
        /// The implementation of <paramref name="contract"/> from the <see
        /// cref="IDependencyResolver"/> or from the method <see cref="GetInstance"/>.
        /// </returns>
        private static object GetInstanceWithContext(Type contract, Type parentContract, int order)
        {
            try
            {
                return _dependenciesResolvers.TryGetValue(contract, out var resolver)
                    ? resolver.Resolve(parentContract, order)
                    : TryGetInstance(contract);
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException($"No registrations for {parentContract}", e);
            }
        }

        /// <summary>
        /// Creates a new instance of a type <paramref name="implementationType"/>.
        /// </summary>
        /// <param name="implementationType">The type to create a new instance.</param>
        /// <returns>A new instance of <paramref name="implementationType"/>.</returns>
        private static object CreateInstance(Type implementationType)
        {
            try
            {
                return CreateInstanceInternal(implementationType);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    $"Unable to create instance of type {implementationType.FullName}",
                    e
                );
            }
        }

        /// <summary>
        /// Creates the instance internal.
        /// </summary>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <returns>System.Object.</returns>
        private static object CreateInstanceInternal(Type implementationType)
        {
            if (_dependenciesCache.TryGetValue(implementationType, out var dependencies))
            {
                return Activator.CreateInstance(
                    implementationType,
                    dependencies
                        .Select((type, i) => GetInstanceWithContext(type, implementationType, i))
                        .ToArray()
                );
            }

            lock (_locks.GetOrAdd(
                implementationType.FullName ?? implementationType.Name,
                new object()
            ))
            {
                if (_dependenciesCache.TryGetValue(implementationType, out dependencies))
                {
                    return Activator.CreateInstance(
                        implementationType,
                        dependencies
                            .Select(
                                (type, i) => GetInstanceWithContext(type, implementationType, i)
                            )
                            .ToArray()
                    );
                }

                var constructors = implementationType.GetConstructors();

                var ctor =
                    constructors.Length == 1
                        ? constructors.Single()
                        : ResolveMultipleConstructors(constructors, implementationType);

                if (ctor == null)
                {
                    return null;
                }

                dependencies = ctor.GetParameters().Select(p => p.ParameterType).ToArray();
                _dependenciesCache.Add(implementationType, dependencies);

                return Activator.CreateInstance(
                    implementationType,
                    dependencies
                        .Select((type, i) => GetInstanceWithContext(type, implementationType, i))
                        .ToArray()
                );
            }
        }

        /// <summary>
        /// Creates the instance with parameters.
        /// </summary>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>System.Object.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Unable to create instance of type {implementationType.FullName} using parameters.
        /// </exception>
        private static object CreateInstanceWith(
            Type implementationType,
            Dictionary<int, object> parameters
        )
        {
            try
            {
                return CreateInstanceWithInternal(implementationType, parameters);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    $"Unable to create instance of type {implementationType.FullName} using parameters",
                    e
                );
            }
        }

        /// <summary>
        /// Creates the instance with parameters internal.
        /// </summary>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>System.Object.</returns>
        private static object CreateInstanceWithInternal(
            Type implementationType,
            Dictionary<int, object> parameters
        )
        {
            var constructors = implementationType.GetConstructors();
            var ctor =
                constructors.Length == 1
                    ? constructors.Single()
                    : ResolveMultipleConstructors(constructors, implementationType);
            if (ctor == null)
            {
                return null;
            }

            var dependencies = ctor.GetParameters().Select(p => p.ParameterType).ToArray();
            var arguments = dependencies.Select(
                (type, i) =>
                    parameters.TryGetValue(i, out var parameter)
                        ? parameter
                        : GetInstanceWithContext(type, implementationType, i)
            );
            return Activator.CreateInstance(implementationType, arguments);
        }

        /// <summary>
        /// Resolves implementation with multiple constructors.
        /// </summary>
        /// <param name="constructors">Array of <see cref="ConstructorInfo"/> to be resolved.</param>
        /// <param name="parentType">The parent type.</param>
        /// <returns>A single instance of <see cref="ConstructorInfo"/>. The winner of the resolution.</returns>
        private static ConstructorInfo ResolveMultipleConstructors(
            ConstructorInfo[] constructors,
            Type parentType = null
        )
        {
            var candidates = constructors
                .Where(c =>
                    c.GetParameters()
                        .All(p => !p.ParameterType.IsSimpleType() && p.ParameterType != parentType)
                )
                .ToList();

            switch (candidates.Count)
            {
                case 0:
                    return null;

                case 1:
                    return candidates[0];

                default:
                    return candidates
                        .OrderByDescending(c => c.GetParameters().Length)
                        .First(c =>
                            c.GetParameters()
                                .Select((p, i) => new { p.ParameterType, Index = i })
                                .All(p =>
                                    (
                                        parentType == null
                                            ? GetInstance(p.ParameterType)
                                            : GetInstanceWithContext(
                                                p.ParameterType,
                                                parentType,
                                                p.Index
                                            )
                                    ) != null
                                )
                        );
            }
        }

        /// <summary>
        /// Tries the automatic registration.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.Object.</returns>
        /// <exception cref="TooManyImplementationsException">
        /// The type {type.FullName} has many implementations available, please consider registering
        /// one of them.
        /// </exception>
        private static object TryAutoRegistration(Type type)
        {
            var types = TypesCache
                .Where(t =>
                    !t.IsAbstract
                    && type.IsAssignableFrom(t)
                    && t.GetConstructors().Any(c => c.GetParameters().Length == 0)
                )
                .ToList();
            if (types.Count == 0)
            {
                return null;
            }

            if (types.Count > 1)
            {
                throw new TooManyImplementationsException(type);
            }

            var instance = GetInstance(types.Single());
            _registrations.AddOrUpdate(type, () => instance, (_, _) => () => instance);
            return instance;
        }

        /// <summary>
        /// Gets the types cache.
        /// </summary>
        /// <value>The types cache.</value>
        public static List<Type> TypesCache { get; }

        /// <summary>
        /// A method for registering a bootstrapper class.
        /// </summary>
        /// <typeparam name="TBootstrapper">The bootstrapper class that inherits <see cref="IBootstrapper"/>.</typeparam>
        public static void RegisterBootstrapper<TBootstrapper>()
            where TBootstrapper : class, IBootstrapper, new()
        {
            new TBootstrapper().RegisterServices();
        }

        /// <summary>
        /// Register an instance for a contract/implementation as a singleton.
        /// </summary>
        /// <typeparam name="TContract">The type of implementation/contract.</typeparam>
        /// <param name="instance">The singleton instance of the contract.</param>
        public static void Register<TContract>(TContract instance)
        {
            var type = typeof(TContract);
            _registrationsCalls.Add(type, 0);
            _registrations.AddOrUpdate(
                type,
                () =>
                {
                    _registrationsCalls[type]++;
                    return instance;
                },
                (_, existingVal) => () => existingVal
            );
        }

        /// <summary>
        /// Registers the specified lifestyle.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        /// <param name="lifestyle">The lifestyle.</param>
        public static void Register<TImplementation>(LifeStyle lifestyle = LifeStyle.Transient)
        {
            var type = typeof(TImplementation);
            RegisterLifeStyledInternal(lifestyle, type, type);
        }

        /// <summary>
        /// Registers the specified lifestyle.
        /// </summary>
        /// <typeparam name="TContract">The type of the t contract.</typeparam>
        /// <typeparam name="TImplementation">The type of the t implementation.</typeparam>
        /// <param name="lifestyle">The lifestyle.</param>
        public static void Register<TContract, TImplementation>(
            LifeStyle lifestyle = LifeStyle.Transient
        )
            where TImplementation : TContract
        {
            var contract = typeof(TContract);
            var implementation = typeof(TImplementation);
            RegisterLifeStyledInternal(lifestyle, contract, implementation);
        }

        /// <summary>
        /// A register with a custom instance creator as a function.
        /// </summary>
        /// <typeparam name="TContract">The interface binding implementation.</typeparam>
        /// <param name="instanceCreator">
        /// The instance creator for an implementation onf <typeparamref name="TContract"/>.
        /// </param>
        /// <param name="lifestyle">The lifecycle lifestyle of the registration.</param>
        public static void Register<TContract>(
            Func<TContract> instanceCreator,
            LifeStyle lifestyle = LifeStyle.Transient
        )
        {
            RegisterLifestyleCreatorInternal(lifestyle, instanceCreator);
        }

        /// <summary>
        /// Registers the dependency resolver.
        /// </summary>
        /// <typeparam name="TContract">The type of the contract.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        public static void RegisterDependencyResolver<TContract, TImplementation>()
            where TImplementation : IDependencyResolver
        {
            var lazy = new Lazy<IDependencyResolver>(
                () => (IDependencyResolver)GetInstance(typeof(TImplementation))
            );
            _dependenciesResolvers.Add(typeof(TContract), lazy.Value);
        }

        /// <summary>
        /// Register a dependency resolver of <typeparamref name="TContract"></typeparamref>.
        /// </summary>
        /// <typeparam name="TContract">The interface binding dependency resolver.</typeparam>
        /// <param name="resolver">The <see cref="IDependencyResolver"/> implementation.</param>
        public static void RegisterDependencyResolver<TContract>(IDependencyResolver resolver)
        {
            _dependenciesResolvers.Add(typeof(TContract), resolver);
        }

        /// <summary>
        /// Disposes all registrations.
        /// </summary>
        public static void DisposeAllRegistrations()
        {
            LogConsumer.Trace("Service locator statistics");
            var temp = new Dictionary<Type, int>(_registrationsCalls);
            foreach (var calls in temp)
            {
                TelemetryAnalytics.TrackDependency(calls.Key, calls.Value);
            }

            var type = typeof(IDisposable);
            var instances = _registrationsCalls
                .Where(call => call.Value > 0)
                .SelectMany(call =>
                    _registrations.Where(implementation =>
                        type.IsAssignableFrom(implementation.Key)
                        && call.Key.IsAssignableFrom(implementation.Key)
                    )
                )
                .ToList();

            foreach (var instance in instances)
            {
                ((IDisposable)instance.Value()).Dispose();
            }
        }

        /// <summary>
        /// Resolves a interface, returning a instance of its implementation.
        /// </summary>
        /// <typeparam name="T">The interface looking implementation.</typeparam>
        /// <returns>
        /// The implementation of a <typeparamref name="T"/>, creating new instance or return a
        /// singleton instance (depending on how it was registered).
        /// </returns>
        public static T Resolve<T>()
        {
            return (T)GetInstance(typeof(T));
        }

        /// <summary>
        /// Tries the resolve.
        /// </summary>
        /// <typeparam name="T">The generic type to resolve.</typeparam>
        /// <returns>T.</returns>
        public static T TryResolve<T>()
        {
            var instance = TryGetInstance(typeof(T));
            return (T)instance;
        }

        /// <summary>
        /// Resolves an interface, returning all instances of its implementations.
        /// </summary>
        /// <typeparam name="T">The interface looking implementation.</typeparam>
        /// <returns>
        /// All implementations of a <typeparamref name="T"/>, creating new instances or returning
        /// singleton instances (depending on how it was registered).
        /// </returns>
        public static IEnumerable<T> ResolveAll<T>()
        {
            return GetAllInstances(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// Resolves the with parameters.
        /// </summary>
        /// <typeparam name="T">The generic type to resolve.</typeparam>
        /// <param name="parameters">The parameters.</param>
        /// <returns>T.</returns>
        public static T ResolveWith<T>(Dictionary<int, object> parameters)
        {
            return (T)GetInstanceWith(typeof(T), parameters);
        }

        /// <summary>
        /// Get the instance of a contract.
        /// </summary>
        /// <param name="contract">The interface looking implementation.</param>
        /// <returns>
        /// The implementation of <paramref name="contract"/>, a new instance or the singleton
        /// instance, based on registered method.
        /// </returns>
        /// <exception cref="InvalidOperationException">No registrations for " + contract.</exception>
        /// <exception cref="InvalidOperationException">No registration for " + contract.</exception>
        public static object GetInstance(Type contract)
        {
            var instance = TryGetInstance(contract);
            if (instance != null)
            {
                return instance;
            }

            throw new InvalidOperationException($"No registrations for {contract}");
        }

        /// <summary>
        /// Gets the instance with.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>System.Object.</returns>
        /// <exception cref="System.InvalidOperationException">No registrations for {contract}.</exception>
        public static object GetInstanceWith(Type contract, Dictionary<int, object> parameters)
        {
            var instance = CreateInstanceWith(contract, parameters);
            if (instance != null)
            {
                return instance;
            }

            throw new InvalidOperationException($"No registrations for {contract}");
        }

        /// <summary>
        /// Tries the get instance.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <returns>System.Object.</returns>
        public static object TryGetInstance(Type contract)
        {
            if (_registrations.TryGetValue(contract, out var creator))
            {
                return creator();
            }

            return !contract.IsAbstract ? CreateInstance(contract) : TryAutoRegistration(contract);
        }

        /// <summary>
        /// Requests the cancellation.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool RequestCancellation()
        {
            if (!_cancellationTokenSource.Token.CanBeCanceled)
            {
                return false;
            }

            _cancellationTokenSource.Cancel();
            return true;
        }

        /// <summary>
        /// Class Destructor. This class cannot be inherited.
        /// </summary>
        private sealed class Destructor
        {
            ~Destructor()
            {
                DisposeAllRegistrations();
                _cancellationTokenSource.Dispose();
            }
        }
    }
}
