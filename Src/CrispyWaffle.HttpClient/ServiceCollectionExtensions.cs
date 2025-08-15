using System;
using Microsoft.Extensions.DependencyInjection;

namespace CrispyWaffle.HttpClient
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers HttpClient wrapper with defaults and adds a system-text JSON serializer.
        /// Ensure you called services.AddHttpClient() earlier / or it will be added automatically.
        /// </summary>
        public static IServiceCollection AddCrispyWaffleHttpClient(this IServiceCollection services, Action<HttpRequestOptions>? configureOptions = null)
        {
            services.AddHttpClient(); // safe to call multiple times
            var options = new HttpRequestOptions();
            configureOptions?.Invoke(options);
            services.Configure(configureOptions ?? (_ => { }));
            // Serializer registration
            services.AddSingleton<Serialization.IJsonSerializer, Serialization.SystemTextJsonSerializer>();
            services.AddSingleton<IHttpClientWrapper, HttpClientWrapper>();
            return services;
        }
    }
}