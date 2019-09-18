using System;
using System.Collections.Generic;

namespace CrispyWaffle.Composition
{
    using Extensions;
    using Log;
    using Log.Handlers;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using Telemetry;

    /// <summary>
    /// The service locator class.
    /// </summary>
    public static class ServiceLocator
    {
        #region Private fields

        /// <summary>
        /// The locks
        /// </summary>
        private static readonly ConcurrentDictionary<string, object> Locks = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// The registrations calls log
        /// </summary>
        private static readonly IDictionary<Type, int> RegistrationsCalls = new Dictionary<Type, int>();

        /// <summary>
        /// The dictionary holding the types and its implementations
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Func<object>> Registrations = new ConcurrentDictionary<Type, Func<object>>();

        /// <summary>
        /// The dictionary holding the types and its dependency resolver implementations
        /// </summary>
        private static readonly IDictionary<Type, IDependencyResolver> DependenciesResolvers = new Dictionary<Type, IDependencyResolver>();

        /// <summary>
        /// The dictionary holding the types and its dependencies as a cache system for new instances (auto registration)
        /// </summary>
        private static readonly IDictionary<Type, Type[]> DependenciesCache = new Dictionary<Type, Type[]>();

        /// <summary>
        /// The instances resolved cache
        /// </summary>
        private static readonly ConcurrentDictionary<Type, List<Type>> Instances = new ConcurrentDictionary<Type, List<Type>>();

        #endregion

        #region ~Ctor

        /// <summary>
        /// Initializes the <see cref="ServiceLocator"/> class.
        /// </summary>
        static ServiceLocator()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var missingAssemblies = loadedAssemblies
                                    .SelectMany(x => x.GetReferencedAssemblies())
                                    .Distinct()
                                    .Where(y => loadedAssemblies.All(a => a.FullName != y.FullName))
                                    .ToList();
            foreach (var missingAssembly in missingAssemblies)
            {
                try
                {
                    loadedAssemblies.Add(AppDomain.CurrentDomain.Load(missingAssembly));
                }
                catch (Exception e)
                {
                    FailOverExceptionHandler.Handle(e);
                }
            }

            TypesCache = AppDomain
                          .CurrentDomain
                          .GetAssemblies()
                          .SelectMany(
                                      a =>
                                      {
                                          try
                                          {
                                              return a.GetTypes();
                                          }
                                          catch (ReflectionTypeLoadException e)
                                          {
                                              //using (var eventLog = new EventLog("Application"))
                                              //{
                                              //    eventLog.Source = "Application";
                                              //    foreach (var loaderException in e.LoaderExceptions)
                                              //        eventLog.WriteEntry($@"{loaderException.Message}{loaderException.StackTrace}", EventLogEntryType.Error);
                                              //}
                                          }
                                          return Enumerable.Empty<Type>();
                                      })
                          .Where(a => a != null && a.Name.IndexOf(@"_canon", StringExtensions.Comparison) == -1)
                          .ToList();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Registers the lifeStyled internal.
        /// </summary>
        /// <param name="lifeStyle">The life style.</param>
        /// <param name="contract">The contract.</param>
        /// <param name="implementation">The implementation.</param>
        private static void RegisterlifeStyledInternal(LifeStyle lifeStyle, Type contract, Type implementation)
        {
            RegistrationsCalls.Add(contract, 0);

            #region Transient

            if (lifeStyle == LifeStyle.TRANSIENT)
            {
                Registrations.AddOrUpdate(contract,
                                          () =>
                                          {
                                              RegistrationsCalls[contract]++;
                                              return GetInstance(implementation);
                                          },
                                          (key, existingValue) => () => existingValue);
                return;
            }

            #endregion

            #region Singleton - IDisposable

            if (implementation.Implements<IDisposable>())
            {
                var lazyDisposable = new LazyDisposable<IDisposable>(() => (IDisposable)CreateInstance(implementation));
                Registrations.AddOrUpdate(contract,
                                          () =>
                                          {
                                              RegistrationsCalls[contract]++;
                                              lock (Locks.GetOrAdd(implementation.FullName ?? implementation.Name, new object()))
                                                  return lazyDisposable.Value;
                                          },
                                          (key, existingValue) => () => existingValue);
                return;
            }

            #endregion

            #region Singleton

            var lazy = new Lazy<object>(() => CreateInstance(implementation));
            Registrations.AddOrUpdate(contract,
                                      () =>
                                      {
                                          RegistrationsCalls[contract]++;
                                          lock (Locks.GetOrAdd(implementation.FullName ?? implementation.Name, new object()))
                                              return lazy.Value;
                                      },
                                      (key, existingValue) => () => existingValue);
            #endregion

        }

        /// <summary>
        /// Registers the lifeStyled creator internal.
        /// </summary>
        /// <typeparam name="TContract">The type of the contract.</typeparam>
        /// <param name="lifeStyle">The life style.</param>
        /// <param name="instanceCreator">The instance creator.</param>
        private static void RegisterlifeStyledCreatorInternal<TContract>(
            LifeStyle lifeStyle,
            Func<TContract> instanceCreator)
        {
            var contract = typeof(TContract);
            RegistrationsCalls.Add(contract, 0);

            #region Transient 

            if (lifeStyle == LifeStyle.TRANSIENT)
            {
                Registrations.AddOrUpdate(
                                          contract,
                                          () =>
                                          {
                                              RegistrationsCalls[contract]++;
                                              return instanceCreator();
                                          },
                                          (key, existingValue) => () => existingValue);
                return;
            }

            #endregion

            #region Singleton - IDisposable

            if (contract.Implements<IDisposable>())
            {
                var lazyDisposable = new LazyDisposable<IDisposable>(() => (IDisposable)instanceCreator());
                Registrations.AddOrUpdate(
                                          contract,
                                          () =>
                                          {
                                              RegistrationsCalls[contract]++;
                                              lock (Locks.GetOrAdd(contract.FullName ?? contract.Name, new object()))
                                                  return lazyDisposable.Value;
                                          },
                                          (key, existingValue) => () => existingValue);
                return;
            }

            #endregion

            #region Singleton

            var lazy = new Lazy<object>(() => instanceCreator());
            Registrations.AddOrUpdate(
                                      contract,
                                      () =>
                                      {
                                          RegistrationsCalls[contract]++;
                                          lock (Locks.GetOrAdd(contract.FullName ?? contract.Name, new object()))
                                              return lazy.Value;
                                      },
                                      (key, existingValue) => () => existingValue);


            #endregion
        }

        /// <summary>
        /// Get all instances for a contract
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns> 
        private static IEnumerable<object> GetAllInstances(Type contract)
        {
            if (!Instances.ContainsKey(contract))
            {
                lock (Locks.GetOrAdd(contract.FullName ?? contract.Name, new object()))
                    if (!Instances.ContainsKey(contract))
                        Instances.TryAdd(contract,
                                         TypesCache.Where(t => contract.IsAssignableFrom(t) && !t.IsAbstract).ToList());

            }

            if (!Instances.TryGetValue(contract, out var types))
                yield break;
            foreach (var type in types)
                yield return GetInstance(type);
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
                return DependenciesResolvers.TryGetValue(contract, out var resolver)
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
            if (DependenciesCache.TryGetValue(implementationType, out var dependencies))
                return Activator.CreateInstance(implementationType, dependencies.Select((type, i) => GetInstanceWithContext(type, implementationType, i))
                    .ToArray());

            lock (Locks.GetOrAdd(implementationType.FullName ?? implementationType.Name, new object()))
            {
                if (DependenciesCache.TryGetValue(implementationType, out dependencies))
                    return Activator.CreateInstance(implementationType,
                        dependencies
                            .Select((type, i) =>
                                GetInstanceWithContext(type, implementationType, i))
                            .ToArray());
                var ctors = implementationType.GetConstructors();
                var ctor = ctors.Length == 1
                    ? ctors.Single()
                    : ResolveMultipleConstructors(ctors, implementationType);
                if (ctor == null)
                    return null;
                dependencies = ctor.GetParameters().Select(p => p.ParameterType).ToArray();
                DependenciesCache.Add(implementationType, dependencies);
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
            var ctors = implementationType.GetConstructors();
            var ctor = ctors.Length == 1
                ? ctors.Single()
                : ResolveMultipleConstructors(ctors, implementationType);
            if (ctor == null)
                return null;
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
        /// <param name="ctors">Array of <see cref="ConstructorInfo"/> to be resolved</param>
        /// <param name="parentType">The parent type</param>
        /// <returns>A single instance of <see cref="ConstructorInfo"/>. The winner of the resolution.</returns>
        private static ConstructorInfo ResolveMultipleConstructors(ConstructorInfo[] ctors, Type parentType = null)
        {
            var candidates = ctors.Where(c => c.GetParameters().All(p => !p.ParameterType.IsSimpleType())).ToList();
            return candidates.Count == 0
                ? null
                : candidates.Count == 1
                       ? candidates.Single()
                       : candidates.OrderByDescending(c => c.GetParameters().Length)
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
                return null;
            if (types.Count > 1)
            {
                //using (var eventLog = new EventLog("Application"))
                //{
                //    eventLog.Source = "Application";
                //    foreach (var t in types)
                //        eventLog.WriteEntry(string.Format(Resources.ServiceLocator_TryAutoRegistration, type.FullName, t.FullName), EventLogEntryType.Warning);
                //}
                throw new TooManyImplementationsException(type);
            }
            var instance = GetInstance(types.Single());
            Registrations.AddOrUpdate(type, () => instance, (key, existingVal) => () => instance);
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
            var bootstrapper = new TBootstrapper();
            try
            {
                bootstrapper.RegisterServices();
            }
            catch (Exception e)
            {
                FailOverExceptionHandler.Handle(e);
            }
        }

        /// <summary>
        /// Register an instance for a contract/implementation as a singleton
        /// </summary>
        /// <typeparam name="TContract">The type of implementation/contract</typeparam>
        /// <param name="instance">The singleton instance of the contract</param>
        public static void Register<TContract>(TContract instance)
        {
            var type = typeof(TContract);
            RegistrationsCalls.Add(type, 0);
            Registrations.AddOrUpdate(type,
                                      () =>
                                      {
                                          RegistrationsCalls[type]++;
                                          return instance;
                                      },
                                      (key, existingVal) => () => existingVal);
        }

        /// <summary>
        /// Registers the specified lifeStyle.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        /// <param name="lifeStyle">The life style.</param>
        public static void Register<TImplementation>(LifeStyle lifeStyle = LifeStyle.TRANSIENT)
        {
            var type = typeof(TImplementation);
            RegisterlifeStyledInternal(lifeStyle, type, type);
        }

        /// <summary>
        /// The basic register for an interface and for its implementation.
        /// </summary>
        /// <typeparam name="TContract">The interface binding implementation</typeparam>
        /// <typeparam name="TImplementation">The concrete implementation of <typeparamref name="TContract" /></typeparam>
        public static void Register<TContract, TImplementation>(LifeStyle lifeStyle = LifeStyle.TRANSIENT) where TImplementation : TContract
        {
            var contract = typeof(TContract);
            var implementation = typeof(TImplementation);
            RegisterlifeStyledInternal(lifeStyle, contract, implementation);
        }

        /// <summary>
        /// A register with a custom instance creator as a function.
        /// </summary>
        /// <typeparam name="TContract">The interface binding implementation</typeparam>
        /// <param name="instanceCreator">The instance creator for an implementation onf <typeparamref name="TContract" /></param>
        /// <param name="lifeStyle">The lifecycle lifeStyle of the registration </param>
        public static void Register<TContract>(Func<TContract> instanceCreator, LifeStyle lifeStyle = LifeStyle.TRANSIENT)
        {
            RegisterlifeStyledCreatorInternal(lifeStyle, instanceCreator);
        }

        /// <summary>
        /// Registers the dependency resolver.
        /// </summary>
        /// <typeparam name="TContract">The type of the contract.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        public static void RegisterDependencyResolver<TContract, TImplementation>() where TImplementation : IDependencyResolver
        {
            var lazy = new Lazy<IDependencyResolver>(() => (IDependencyResolver)GetInstance(typeof(TImplementation)));
            DependenciesResolvers.Add(typeof(TContract), lazy.Value);
        }

        /// <summary>
        /// Register a dependency resolver of <typeparamref name="TContract"></typeparamref>
        /// </summary>
        /// <typeparam name="TContract">The interface binding dependency resolver</typeparam>
        /// <param name="resolver">The <see cref="IDependencyResolver"/> implementation</param>
        public static void RegisterDependencyResolver<TContract>(IDependencyResolver resolver)
        {
            DependenciesResolvers.Add(typeof(TContract), resolver);
        }

        #endregion

        #region Finalizers

        /// <summary>
        /// Disposes all registrations.
        /// </summary>
        public static void DisposeAllRegistrations()
        {
            LogConsumer.Trace("Service locator statistics");
            var temp = new Dictionary<Type, int>(RegistrationsCalls);
            foreach (var calls in temp)
                TelemetryAnalytics.TrackDependency(calls.Key, calls.Value);
            var type = typeof(IDisposable);
            var instances = RegistrationsCalls
                .Where(call => call.Value > 0)
                .SelectMany(call => Registrations.Where(implementation =>
                                                        type.IsAssignableFrom(implementation.Key) &&
                                                        call.Key.IsAssignableFrom(implementation.Key)))
                                                        .ToList();

            foreach (var instance in instances)
                ((IDisposable)instance.Value()).Dispose();

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
                return instance;
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
                return instance;
            throw new InvalidOperationException($"No registrations for {contract}");
        }

        /// <summary>
        /// Tries the get instance.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <returns></returns>
        public static object TryGetInstance(Type contract)
        {
            return Registrations.TryGetValue(contract, out var creator)
                ? creator()
                : !contract.IsAbstract
                       ? CreateInstance(contract)
                       : TryAutoRegistration(contract);
        }

        #endregion

        #endregion
    }
}
