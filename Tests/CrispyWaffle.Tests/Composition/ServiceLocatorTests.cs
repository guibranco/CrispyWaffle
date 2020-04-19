using CrispyWaffle.Composition;
using System.Threading;
using Xunit;

namespace CrispyWaffle.Tests.Composition
{
    public class ServiceLocatorTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceLocatorTests"/> class.
        /// </summary>
        public ServiceLocatorTests()
        {
            ServiceLocator.Register<TestObjects.SingletonTest>(LifeStyle.SINGLETON);
            ServiceLocator.Register<TestObjects.SingletonWithDependencyTest>(LifeStyle.SINGLETON);
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
