// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 07-29-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-06-2020
// ***********************************************************************
// <copyright file="LoggingFixture.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using CrispyWaffle.Composition;
using CrispyWaffle.Log;
using CrispyWaffle.TemplateRendering.Engines;
using CrispyWaffle.Tests.Composition;
using System;
using Xunit.Abstractions;

namespace CrispyWaffle.Tests.Fixtures
{
    /// <summary>
    /// Class LoggingFixture.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class LoggingFixture : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingFixture" /> class.
        /// </summary>
        public LoggingFixture()
        {
            ServiceLocator.Register<TestObjects.SingletonTest>(LifeStyle.SINGLETON);
            ServiceLocator.Register<TestObjects.SingletonWithDependencyTest>(LifeStyle.SINGLETON);

            ServiceLocator.Register<ITemplateRender, MustacheTemplateRender>();
        }

        /// <summary>
        /// Sets the log provider.
        /// </summary>
        /// <param name="testOutputHelper">The test output helper.</param>
        public void SetLogProvider(ITestOutputHelper testOutputHelper)
        {
            LogConsumer.AddProvider(new TestLogProvider(testOutputHelper)).SetLevel(LogLevel.ALL);
        }

        #region IDisposable Support

        /// <summary>
        /// The disposed value
        /// </summary>
        private bool _disposedValue; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
            {
                return;
            }

            if (disposing)
            {
                //ServiceLocator.DisposeAllRegistrations();
            }

            _disposedValue = true;
        }

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion
    }
}
