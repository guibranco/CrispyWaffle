using System.Threading.Tasks;
using CrispyWaffle.Composition;
using CrispyWaffle.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace CrispyWaffle.Tests.Composition;

[Collection("Logged collection")]
public class ServiceLocatorTests
{
    public ServiceLocatorTests(ITestOutputHelper testOutputHelper, LoggingFixture fixture) =>
        fixture.SetLogProvider(testOutputHelper);

    [Fact]
    public async Task ValidateSingletonCreationAndPersistence()
    {
        var instanceA = ServiceLocator.Resolve<TestObjects.SingletonTest>();
        await Task.Delay(1000);
        var instanceB = ServiceLocator.Resolve<TestObjects.SingletonTest>();
        await Task.Delay(1000);
        var instanceC = ServiceLocator.Resolve<TestObjects.SingletonTest>();

        Assert.Equal(instanceA.Date, instanceB.Date);
        Assert.Equal(instanceA.Date, instanceC.Date);
    }

    [Fact]
    public void ValidateSingletonCreationWithDependency()
    {
        var instanceInner = ServiceLocator.Resolve<TestObjects.SingletonTest>();
        var instance = ServiceLocator.Resolve<TestObjects.SingletonWithDependencyTest>();

        Assert.NotNull(instance.Singleton);

        Assert.Equal(instanceInner.Date, instance.Singleton.Date);
    }

    [Fact]
    public void ValidateCancellationTokenUsage()
    {
        var instance = ServiceLocator.Resolve<TestObjects.CancellationTokenDependencyTest>();

        Assert.NotNull(instance);

        Assert.False(instance.CancellationToken.IsCancellationRequested);
        Assert.True(instance.CancellationToken.CanBeCanceled);
    }

    [Fact]
    public void ValidateCancellationTokenCall()
    {
        var instance = ServiceLocator.Resolve<TestObjects.CancellationTokenDependencyTest>();

        Assert.NotNull(instance);

        Assert.False(instance.CancellationToken.IsCancellationRequested);
        Assert.True(instance.CancellationToken.CanBeCanceled);

        var success = ServiceLocator.RequestCancellation();

        Assert.True(success);

        Assert.True(instance.CancellationToken.IsCancellationRequested);
    }
}
