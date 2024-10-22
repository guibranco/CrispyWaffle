using Xunit;
using Microsoft.Extensions.DependencyInjection;

public class CustomIoCTests
{
    [Fact]
    public void TestTransientRegistration()
    {
        var services = new ServiceCollection();
        var builder = new CustomContainerBuilder();
        builder.RegisterTransient<IService, ServiceImplementation>();
        builder.Populate(services);
        var provider = builder.BuildServiceProvider();

        var service1 = provider.GetService<IService>();
        var service2 = provider.GetService<IService>();

        Assert.NotSame(service1, service2);
    }

    [Fact]
    public void TestSingletonRegistration()
    {
        var services = new ServiceCollection();
        var builder = new CustomContainerBuilder();
        builder.RegisterSingleton<IService, ServiceImplementation>();
        builder.Populate(services);
        var provider = builder.BuildServiceProvider();

        var service1 = provider.GetService<IService>();
        var service2 = provider.GetService<IService>();

        Assert.Same(service1, service2);
    }
}
