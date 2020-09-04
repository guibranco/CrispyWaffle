// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 05-28-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 06-06-2020
// ***********************************************************************
// <copyright file="ServiceLocatorTests.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using CrispyWaffle.Composition;
using System.Threading;
using Xunit;

namespace CrispyWaffle.Tests.Composition
{
    /// <summary>
    /// Class ServiceLocatorTests.
    /// </summary>
    [Collection("ServiceLocator collection")]
    public class ServiceLocatorTests
    {
        /// <summary>
        /// Defines the test method ValidateSingletonCreationAndPersistence.
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
        /// Defines the test method ValidateSingletonCreationWithDependency.
        /// </summary>
        [Fact]
        public void ValidateSingletonCreationWithDependency()
        {
            var instanceInner = ServiceLocator.Resolve<TestObjects.SingletonTest>();
            var instance = ServiceLocator.Resolve<TestObjects.SingletonWithDependencyTest>();

            Assert.NotNull(instance.Singleton);

            Assert.Equal(instanceInner.Date, instance.Singleton.Date);
        }

        /// <summary>
        /// Defines the test method ValidateCancellationTokenUsage.
        /// </summary>
        [Fact]
        public void ValidateCancellationTokenUsage()
        {
            var instance = ServiceLocator.Resolve<TestObjects.CancellationTokenDependencyTest>();

            Assert.NotNull(instance);

            Assert.False(instance.CancellationToken.IsCancellationRequested);
            Assert.True(instance.CancellationToken.CanBeCanceled);
        }


        /// <summary>
        /// Defines the test method ValidateCancellationTokenCall.
        /// </summary>
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
}
