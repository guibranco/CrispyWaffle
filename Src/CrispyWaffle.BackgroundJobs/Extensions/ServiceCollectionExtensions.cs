using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CrispyWaffle.BackgroundJobs.Core;
using CrispyWaffle.BackgroundJobs.Abstractions;
using CrispyWaffle.BackgroundJobs.Persistence;
using CrispyWaffle.BackgroundJobs.Monitoring;
using Microsoft.AspNetCore.Hosting;

namespace CrispyWaffle.BackgroundJobs.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register core background job services. By default uses InMemoryJobStore. To use EF Core, call AddDbContext<JobDbContext> before this and pass useEfCore=true.
        /// </summary>
        public static IServiceCollection AddCrispyBackgroundJobs(this IServiceCollection services, Action<JobOptions>? configureOptions = null, bool useEfCoreStore = false)
        {
            var options = new JobOptions();
            configureOptions?.Invoke(options);

            services.AddSingleton(options);
            services.AddSingleton<IJobHandlerRegistry, JobHandlerRegistry>();
            services.AddSingleton<JobDispatcher>();
            services.AddSingleton<JobMetrics>();

            // Register appropriate store
            if (useEfCoreStore)
            {
                // JobDbContext must be registered by caller
                services.TryAddScoped<IJobStore, EfCoreJobStore>();
            }
            else
            {
                services.TryAddSingleton<IJobStore, InMemoryJobStore>();
                services.TryAddSingleton<BackgroundJobQueue>();
            }

            // Scheduler & worker
            services.AddSingleton<IJobScheduler, CrispyWaffle.BackgroundJobs.Scheduling.DelayedJobScheduler>();

            // Hosted service (worker)
            services.AddHostedService<BackgroundWorker>();

            return services;
        }

        /// <summary>
        /// Register a typed job handler where handler is resolved from DI and payload is serialized as JSON.
        /// handlerName is the logical identifier used when enqueueing jobs.
        /// </summary>
        public static IServiceCollection AddJobHandler<THandler, TData>(this IServiceCollection services, string handlerName)
            where THandler : class, IBackgroundJobHandler<TData>
        {
            services.AddScoped<THandler>();
            // The registry registration happens at runtime; to ensure the registry has the entry we add a post-processor
            services.AddSingleton<IStartupFilter>(sp => new JobHandlerStartupFilter(sp, handlerName, typeof(THandler), typeof(TData)));
            return services;
        }

        // StartupFilter ensures the registry is populated after DI container is built
        private class JobHandlerStartupFilter : Microsoft.AspNetCore.Hosting.IStartupFilter
        {
            private readonly IServiceProvider _sp;
            private readonly string _handlerName;
            private readonly Type _handlerType;
            private readonly Type _dataType;

            public JobHandlerStartupFilter(IServiceProvider sp, string handlerName, Type handlerType, Type dataType)
            {
                _sp = sp; _handlerName = handlerName; _handlerType = handlerType; _dataType = dataType;
            }

            public Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> Configure(Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> next)
            {
                return app =>
                {
                    var registry = _sp.GetRequiredService<IJobHandlerRegistry>();
                    var registerMethod = registry.GetType().GetMethod("Register")?.MakeGenericMethod(_handlerType, _dataType);
                    // If Register<THandler, TData>(string) exists we call via reflection
                    if (registerMethod != null)
                    {
                        registerMethod.Invoke(registry, new object[] { _handlerName });
                    }

                    next(app);
                };
            }
        }
    }
}
