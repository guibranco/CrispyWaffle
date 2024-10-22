using Microsoft.Extensions.DependencyInjection;
using System;

public class CustomServiceProviderFactory : IServiceProviderFactory<CustomContainerBuilder>
{
    public CustomContainerBuilder CreateBuilder(IServiceCollection services)
    {
        var builder = new CustomContainerBuilder();
        builder.Populate(services);
        return builder;
    }

    public IServiceProvider CreateServiceProvider(CustomContainerBuilder containerBuilder)
    {
        return containerBuilder.BuildServiceProvider();
    }
}

public class CustomContainerBuilder
{
    // Implementation of registration methods and BuildServiceProvider
}
