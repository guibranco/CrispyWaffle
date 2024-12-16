using Microsoft.Extensions.DependencyInjection;
using System;

public class CustomContainerBuilder
{
    public void RegisterTransient<TService, TImplementation>() where TImplementation : TService
    {
        // Implementation
    }

    public void RegisterScoped<TService, TImplementation>() where TImplementation : TService
    {
        // Implementation
    }

    public void RegisterSingleton<TService, TImplementation>() where TImplementation : TService
    {
        // Implementation
    }

    public void Populate(IServiceCollection services)
    {
        // Transfer registrations from IServiceCollection to the custom container
    }

    public IServiceProvider BuildServiceProvider()
    {
        // Build and return the IServiceProvider
    }
}
