using CrispyWaffle.Composition;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace CrispyWaffle.Tests.Composition
{
    public class ServiceLocatorTests : IClassFixture<BootstrapFixture>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceLocatorTests"/> class.
        /// </summary>
        /// <param name="fixture">The fixture.</param>
        /// <param name="testOutputHelper">The test output helper.</param>
        public ServiceLocatorTests(BootstrapFixture fixture, ITestOutputHelper testOutputHelper)
        {
            fixture.SetLog(testOutputHelper);
        }

        /// <summary>
        /// Validates the singleton creation and persistence.
        /// </summary>
        [Fact]
        public void ValidateSingletonCreationAndPersistence()
        {

            var instanceA = ServiceLocator.Resolve<TestObjects.SingletonTest>();
            Thread.Sleep(1000);
            var instanceB = ServiceLocator.Resolve<TestObjects.SingletonTest>();
            Thread.Sleep(1000);
            var instanceC = ServiceLocator.Resolve<TestObjects.SingletonTest>();
            Assert.Equal(instanceA.Date, instanceB.Date);
            Assert.Equal(instanceA.Date, instanceC.Date);
        }

        /// <summary>
        /// Validates the singleton creation with dependency.
        /// </summary>
        [Fact]
        public void ValidateSingletonCreationWithDependency()
        {
            var instanceInner = ServiceLocator.Resolve<TestObjects.SingletonTest>();
            var instance = ServiceLocator.Resolve<TestObjects.SingletonWithDependencyTest>();
            Assert.NotNull(instance.Singleton);
            Assert.Equal(instanceInner.Date, instance.Singleton.Date);
        }
    }
}
