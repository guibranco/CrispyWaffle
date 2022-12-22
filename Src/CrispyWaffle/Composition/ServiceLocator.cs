namespace CrispyWaffle.Composition
{
    using System.Threading;
    using CrispyWaffle.Extensions;
    using CrispyWaffle.Log;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CrispyWaffle.Telemetry;

    /// <summary>
    /// The service locator class.
    /// </summary>
    public static class ServiceLocator
    {
        #region Private fields

        /// <summary>
        /// The locks
        /// </summary>
        private static readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// The registrations calls log
        /// </summary>
        private static readonly IDictionary<Type, int> _registrationsCalls = new Dictionary<Type, int>();

        /// <summary>
        /// The dictionary holding the types and its implementations
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Func<object>> _registrations = new ConcurrentDictionary<Type, Func<object>>();

        /// <summary>
        /// The dictionary holding the types and its dependency resolver implementations
        /// </summary>
        private static readonly IDictionary<Type, IDependencyResolver> _dependenciesResolvers = new Dictionary<Type, IDependencyResolver>();

        /// <summary>
        /// The dictionary holding the types and its dependencies as a cache system for new instances (auto registration)
        /// </summary>
        private static readonly IDictionary<Type, Type[]> _dependenciesCache = new Dictionary<Type, Type[]>();

        /// <summary>
        /// The instances resolved cache
        /// </summary>
        private static readonly ConcurrentDictionary<Type, List<Type>> _instances = new ConcurrentDictionary<Type, List<Type>>();

        /// <summary>
        /// The cancellation token source
        /// </summary>
        private static readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// The not loaded assemblies
        /// </summary>
        private static readonly IDictionary<string, Exception> _notLoadedAssemblies = new Dictionary<string, Exception>();

        #endregion

        #region ~Ctor

        /// <summary>
        /// Initializes the <see cref="ServiceLocator"/> class.
        /// </summary>
        static ServiceLocator()
        {
            LoadMissingAssemblies();
            TypesCache = AppDomain
                         .CurrentDomain
                         .GetAssemblies()
                         .SelectMany(a => a.GetTypes())
                         .Where(a => a != null && a.Name.IndexOf(@"_canon", StringComparison.InvariantCultureIgnoreCase) == -1)
                         .ToList();
            var cancellationToken = typeof(CancellationToken);
            _registrationsCalls.Add(cancellationToken, 0);
            _registrations.AddOrUpdate(cancellationToken, () => _cancellationTokenSource.Token, (key, existingValue) => () => existingValue);
        }

        #endregion

        #region Private methods

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
                if (!_notLoadedAssemblies.Keys.Contains(missingAssembly.FullName))
                {
                    _notLoadedAssemblies.Add(missingAssembly.FullName, e);
                }
            }
        }

        /// <summary>
        /// Registers the life styled internal.
        /// </summary>
        /// <param name="lifeStyle">The life style.</param>
        /// <param name="contract">The contract.</param>
        /// <param name="implementation">The implementation.</param>
        private static void RegisterLifeStyledInternal(LifeStyle lifeStyle, Type contract, Type implementation)
        {
            _registrationsCalls.Add(contract, 0);

            if (lifeStyle == LifeStyle.Transient)
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
            _registrations.AddOrUpdate(contract,
                () =>
                {
                    _registrationsCalls[contract]++;
                    lock (_locks.GetOrAdd(implementation.FullName ?? implementation.Name, new object()))
                    {
                        return lazy.Value;
                    }
                },
                (key, existingValue) => () => existingValue);
        }

        /// <summary>
        /// Registers the lifeStyled creator internal.
        /// </summary>
        /// <typeparam name="TContract">The type of the contract.</typeparam>
        /// <param name="lifeStyle">The life style.</param>
        /// <param name="instanceCreator">The instance creator.</param>
        private static void RegisterLifeStyledCreatorInternal<TContract>(
            LifeStyle lifeStyle,
            Func<TContract> instanceCreator)
        {
            var contract = typeof(TContract);
            _registrationsCalls.Add(contract, 0);

            if (lifeStyle == LifeStyle.Transient)
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
        private static void RegisterSingleton<TContract>(Func<TContract> instanceCreator, Type contract)
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
                (key, existingValue) => () => existingValue);
        }

        /// <summary>
        /// Registers the disposable internal.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="implementation">The implementation.</param>
        private static void RegisterDisposableInternal(Type contract, Type implementation)
        {
            var lazyDisposable = new LazyDisposable<IDisposable>(() => (IDisposable)CreateInstance(implementation));
            _registrations.AddOrUpdate(contract,
                () =>
                {
                    _registrationsCalls[contract]++;
                    lock (_locks.GetOrAdd(implementation.FullName ?? implementation.Name, new object()))
                    {
                        return lazyDisposable.Value;
                    }
                },
                (key, existingValue) => () => existingValue);
        }

        /// <summary>
        /// Registers the disposable internal.
        /// </summary>
        /// <typeparam name="TContract">The type of the t contract.</typeparam>
        /// <param name="instanceCreator">The instance creator.</param>
        /// <param name="contract">The contract.</param>
        private static void RegisterDisposableInternal<TContract>(Func<TContract> instanceCreator, Type contract)
        {
            var lazyDisposable = new LazyDisposable<IDisposable>(() => (IDisposable)instanceCreator());
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
                (key, existingValue) => () => existingValue);
        }

        /// <summary>
        /// Registers the transient internal.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="implementation">The implementation.</param>
        private static void RegisterTransientInternal(Type contract, Type implementation)
        {
            _registrations.AddOrUpdate(contract,
                () =>
                {
                    _registrationsCalls[contract]++;
                    return GetInstance(implementation);
                },
                (key, existingValue) => () => existingValue);
        }

        /// <summary>
        /// Registers the transient internal.
        /// </summary>
        /// <typeparam name="TContract">The type of the t contract.</typeparam>
        /// <param name="instanceCreator">The instance creator.</param>
        /// <param name="contract">The contract.</param>
        private static void RegisterTransientInternal<TContract>(Func<TContract> instanceCreator, Type contract)
        {
            _registrations.AddOrUpdate(
                contract,
                () =>
                {
                    _registrationsCalls[contract]++;
                    return instanceCreator();
                },
                (key, existingValue) => () => existingValue);
        }

        /// <summary>
        /// Get all instances for a contract
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns> 
        private static IEnumerable<object> GetAllInstances(Type contract)
        {
            if (!_instances.ContainsKey(contract))
            {
                lock (_locks.GetOrAdd(contract.FullName ?? contract.Name, new object()))
                {
                    if (!_instances.ContainsKey(contract))
                    {
                        _instances.TryAdd(contract,
                            TypesCache.Where(t => contract.IsAssignableFrom(t) && !t.IsAbstract).ToList());
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
        /// Get the instance of a contract based on where it is requested (parent contract) using a dependency resolver if any available for the type to get a context for the dependency.
        /// </summary>
        /// <param name="contract">The interface looking implementation</param>
        /// <param name="parentContract">The interface where the contract is requested and requires a context</param>
        /// <param name="order">The order of the <paramref name="contract"></paramref> in the <paramref name="parentContract"/> arguments list</param>
        /// <returns>The implementation of <paramref name="contract" /> from the <see cref="IDependencyResolver"/> or from the method <see cref="GetInstance"/></returns>
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
        /// Creates a new instance of a type <paramref name="implementationType" />
        /// </summary>
        /// <param name="implementationType">The type to create a new instance</param>
        /// <returns>A new instance of <paramref name="implementationType" /></returns>
        private static object CreateInstance(Type implementationType)
        {
            try
            {
                return CreateInstanceInternal(implementationType);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Unable to create instance of type {implementationType.FullName}", e);
            }

        }

        /// <summary>
        /// Creates the instance internal.
        /// </summary>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <returns></returns>
        private static object CreateInstanceInternal(Type implementationType)
        {
            if (_dependenciesCache.TryGetValue(implementationType, out var dependencies))
            {
                return Activator.CreateInstance(implementationType, dependencies.Select((type, i) => GetInstanceWithContext(type, implementationType, i))
                    .ToArray());
            }

            lock (_locks.GetOrAdd(implementationType.FullName ?? implementationType.Name, new object()))
            {
                if (_dependenciesCache.TryGetValue(implementationType, out dependencies))
                {
                    return Activator.CreateInstance(implementationType,
                        dependencies
                            .Select((type, i) =>
                                GetInstanceWithContext(type, implementationType, i))
                            .ToArray());
                }

                var constructors = implementationType.GetConstructors();

                var ctor = constructors.Length == 1
                    ? constructors.Single()
                    : ResolveMultipleConstructors(constructors, implementationType);

                if (ctor == null)
                {
                    return null;
                }

                dependencies = ctor.GetParameters().Select(p => p.ParameterType).ToArray();
                _dependenciesCache.Add(implementationType, dependencies);

                return Activator.CreateInstance(implementationType,
                    dependencies
                        .Select((type, i) =>
                            GetInstanceWithContext(type, implementationType, i))
                        .ToArray());
            }
        }

        /// <summary>
        /// Creates the instance with.
        /// </summary>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static object CreateInstanceWith(Type implementationType, Dictionary<int, object> parameters)
        {
            try
            {
                return CreateInstanceWithInternal(implementationType, parameters);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    $"Unable to create instance of type {implementationType.FullName} using parameters", e);
            }
        }

        /// <summary>
        /// Creates the instance with internal.
        /// </summary>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private static object CreateInstanceWithInternal(Type implementationType, Dictionary<int, object> parameters)
        {
            var constructors = implementationType.GetConstructors();
            var ctor = constructors.Length == 1
                ? constructors.Single()
                : ResolveMultipleConstructors(constructors, implementationType);
            if (ctor == null)
            {
                return null;
            }

            var dependencies = ctor.GetParameters().Select(p => p.ParameterType).ToArray();
            var arguments = dependencies.Select((type, i) =>
                parameters.ContainsKey(i)
                    ? parameters[i]
                    : GetInstanceWithContext(type, implementationType, i));
            return Activator.CreateInstance(implementationType, arguments);
        }

        /// <summary>
        /// Resolves implementation with multiple constructors
        /// </summary>
        /// <param name="constructors">Array of <see cref="ConstructorInfo"/> to be resolved</param>
        /// <param name="parentType">The parent type</param>
        /// <returns>A single instance of <see cref="ConstructorInfo"/>. The winner of the resolution.</returns>
        private static ConstructorInfo ResolveMultipleConstructors(ConstructorInfo[] constructors, Type parentType = null)
        {
            var candidates = constructors.Where(c =>
                c.GetParameters().All(p => !p.ParameterType.IsSimpleType() && p.ParameterType != parentType)).ToList();

            if (candidates.Count == 0)
            {
                return null;
            }

            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            return candidates.OrderByDescending(c => c.GetParameters().Length)
                .First(
                    c => c.GetParameters()
                        .Select((p, i) => new { p.ParameterType, Index = i })
                        .All(p => (parentType == null
                            ? GetInstance(p.ParameterType)
                            : GetInstanceWithContext(p.ParameterType, parentType, p.Index)) != null));

        }

        /// <summary>
        /// Tries the automatic registration.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="TooManyImplementationsException">Thrown when more than one implementation is available for the same type</exception>
        private static object TryAutoRegistration(Type type)
        {
            var types = TypesCache
                        .Where(t =>
                                   !t.IsAbstract &&
                                   type.IsAssignableFrom(t) &&
                                   t.GetConstructors().Any(c => !c.GetParameters().Any()))
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
            _registrations.AddOrUpdate(type, () => instance, (key, existingVal) => () => instance);
            return instance;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets the types cache.
        /// </summary>
        /// <value>
        /// The types cache.
        /// </value>
        public static List<Type> TypesCache { get; }

        #endregion

        #region Public methods

        #region Registrators

        /// <summary>
        /// A method for registering a bootstrapper class
        /// </summary>
        /// <typeparam name="TBootstrapper">The bootstrapper class that inherits <see cref="IBootstrapper"/></typeparam>
        public static void RegisterBootstrapper<TBootstrapper>() where TBootstrapper : class, IBootstrapper, new()
        {
            new TBootstrapper().RegisterServices();
        }

        /// <summary>
        /// Register an instance for a contract/implementation as a singleton
        /// </summary>
        /// <typeparam name="TContract">The type of implementation/contract</typeparam>
        /// <param name="instance">The singleton instance of the contract</param>
        public static void Register<TContract>(TContract instance)
        {
            var type = typeof(TContract);
            _registrationsCalls.Add(type, 0);
            _registrations.AddOrUpdate(type,
                                      () =>
                                      {
                                          _registrationsCalls[type]++;
                                          return instance;
                                      },
                                      (key, existingVal) => () => existingVal);
        }

        /// <summary>
        /// Registers the specified lifeStyle.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        /// <param name="lifeStyle">The life style.</param>
        public static void Register<TImplementation>(LifeStyle lifeStyle = LifeStyle.Transient)
        {
            var type = typeof(TImplementation);
            RegisterLifeStyledInternal(lifeStyle, type, type);
        }

        /// <summary>
        /// The basic register for an interface and for its implementation.
        /// </summary>
        /// <typeparam name="TContract">The interface binding implementation</typeparam>
        /// <typeparam name="TImplementation">The concrete implementation of <typeparamref name="TContract" /></typeparam>
        public static void Register<TContract, TImplementation>(LifeStyle lifeStyle = LifeStyle.Transient) where TImplementation : TContract
        {
            var contract = typeof(TContract);
            var implementation = typeof(TImplementation);
            RegisterLifeStyledInternal(lifeStyle, contract, implementation);
        }

        /// <summary>
        /// A register with a custom instance creator as a function.
        /// </summary>
        /// <typeparam name="TContract">The interface binding implementation</typeparam>
        /// <param name="instanceCreator">The instance creator for an implementation onf <typeparamref name="TContract" /></param>
        /// <param name="lifeStyle">The lifecycle lifeStyle of the registration </param>
        public static void Register<TContract>(Func<TContract> instanceCreator, LifeStyle lifeStyle = LifeStyle.Transient)
        {
            RegisterLifeStyledCreatorInternal(lifeStyle, instanceCreator);
        }

        /// <summary>
        /// Registers the dependency resolver.
        /// </summary>
        /// <typeparam name="TContract">The type of the contract.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        public static void RegisterDependencyResolver<TContract, TImplementation>() where TImplementation : IDependencyResolver
        {
            var lazy = new Lazy<IDependencyResolver>(() => (IDependencyResolver)GetInstance(typeof(TImplementation)));
            _dependenciesResolvers.Add(typeof(TContract), lazy.Value);
        }

        /// <summary>
        /// Register a dependency resolver of <typeparamref name="TContract"></typeparamref>
        /// </summary>
        /// <typeparam name="TContract">The interface binding dependency resolver</typeparam>
        /// <param name="resolver">The <see cref="IDependencyResolver"/> implementation</param>
        public static void RegisterDependencyResolver<TContract>(IDependencyResolver resolver)
        {
            _dependenciesResolvers.Add(typeof(TContract), resolver);
        }

        #endregion

        #region Finalizers

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
                .SelectMany(call => _registrations.Where(implementation =>
                                                        type.IsAssignableFrom(implementation.Key) &&
                                                        call.Key.IsAssignableFrom(implementation.Key)))
                                                        .ToList();

            foreach (var instance in instances)
            {
                ((IDisposable)instance.Value()).Dispose();
            }
        }

        #endregion

        #region Resolvers

        /// <summary>
        /// Resolves a interface, returning a instance of its implementation.
        /// </summary>
        /// <typeparam name="T">The interface looking implementation</typeparam>
        /// <returns>The implementation of a <typeparamref name="T" />, creating new instance or return a singleton instance (depending how it was registered)</returns>
        public static T Resolve<T>()
        {
            return (T)GetInstance(typeof(T));
        }

        /// <summary>
        /// Tries the resolve.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T TryResolve<T>()
        {
            var instance = TryGetInstance(typeof(T));
            return (T)instance;
        }

        /// <summary>
        /// Resolves a interface, returning all instances of its implementations
        /// </summary>
        /// <typeparam name="T">The interface looking implementation</typeparam>
        /// <returns>All implementations of a <typeparamref name="T" />, creating new instances or returning singleton instances (depending how it was registered)</returns>
        public static IEnumerable<T> ResolveAll<T>()
        {
            return GetAllInstances(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// Resolves the with.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static T ResolveWith<T>(Dictionary<int, object> parameters)
        {
            return (T)GetInstanceWith(typeof(T), parameters);
        }

        /// <summary>
        /// Get the instance of a contract.
        /// </summary>
        /// <param name="contract">The interface looking implementation</param>
        /// <returns>The implementation of <paramref name="contract" />, a new instance or the singleton instance, based on registered method</returns>
        /// <exception cref="InvalidOperationException">No registrations for " + contract</exception>
        /// <exception cref="InvalidOperationException">No registration for " + contract</exception>
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
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
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
        /// <returns></returns>
        public static object TryGetInstance(Type contract)
        {
            if (_registrations.TryGetValue(contract, out var creator))
            {
                return creator();
            }

            return !contract.IsAbstract
                ? CreateInstance(contract)
                : TryAutoRegistration(contract);
        }

        #endregion

        #region Helpers

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

        #endregion

        #endregion
    }
}
