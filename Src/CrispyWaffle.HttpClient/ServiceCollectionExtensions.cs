using System;
using Microsoft.Extensions.DependencyInjection;

namespace CrispyWaffle.HttpClient
{
    /// <summary>
    /// Extension methods to register the HttpClient wrapper in an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the HttpClient wrapper with defaults and a System.Text.Json serializer.
        /// Make sure you call this after configuring your application's dependency injection.
        /// </summary>
        /// <param name="services">The service collection to add the wrapper to.</param>
        /// <param name="configureOptions">An optional action to configure the default <see cref="HttpRequestOptions"/>.</param>
        /// <returns>The same <see cref="IServiceCollection"/>, so that calls can be chained.</returns>
        /// <remarks>
        /// Improvements:
        /// 1. Use services.Configure(...) to register options in a standard and thread-safe way.
        /// 2. Ensure serializer is thread-safe by registering a single instance.
        /// 3. Prepare for retry logic improvements in HttpClientWrapper by ensuring that per-request serializers are
        ///    handled without mutating shared state.
        /// </remarks>
        public static IServiceCollection AddCrispyWaffleHttpClient(
            this IServiceCollection services,
            Action<HttpRequestOptions>? configureOptions = null
        )
        {
            // Ensure HttpClientFactory is registered (safe to call multiple times).
            services.AddHttpClient();

            // Register options using the built-in Microsoft.Extensions.Options pattern.
            // This ensures thread-safety and that options can be injected anywhere via IOptions<T>.
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }
            else
            {
                services.Configure<HttpRequestOptions>(_ => { });
            }

            // Register serializer as a singleton for thread-safety.
            // This assumes the serializer implementation is stateless or internally thread-safe.
            services.AddSingleton<
                Serialization.IJsonSerializer,
                Serialization.SystemTextJsonSerializer
            >();

            // Register the HttpClient wrapper.
            services.AddSingleton<IHttpClientWrapper, HttpClientWrapper>();

            return services;
        }
    }
}
