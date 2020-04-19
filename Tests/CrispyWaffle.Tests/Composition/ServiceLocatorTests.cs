using CrispyWaffle.Composition;
using System.Threading;
using Xunit;

namespace CrispyWaffle.Tests.Composition
{
    public class ServiceLocatorTests
    {
        /// <summary>
        /// Validates the singleton creation and persistence.
        /// </summary>
        [Fact]
        public void ValidateSingletonCreationAndPersistence()
        {
            ServiceLocator.Register<TestObjects.SingletonTest>(LifeStyle.SINGLETON);
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
            ServiceLocator.Register<TestObjects.SingletonTest>(LifeStyle.SINGLETON);
            var instanceInner = ServiceLocator.Resolve<TestObjects.SingletonTest>();
            ServiceLocator.Register<TestObjects.SingletonWithDependencyTest>(LifeStyle.SINGLETON);
            var instance = ServiceLocator.Resolve<TestObjects.SingletonWithDependencyTest>();
            Assert.NotNull(instance.Singleton);
            Assert.Equal(instanceInner.Date, instance.Singleton.Date);
        }
    }
}
