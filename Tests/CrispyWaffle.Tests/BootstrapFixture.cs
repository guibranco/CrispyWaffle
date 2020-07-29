using CrispyWaffle.Composition;
using CrispyWaffle.Log;
using CrispyWaffle.Tests.Composition;
using System;
using Xunit.Abstractions;

namespace CrispyWaffle.Tests
{
    /// <summary>
    /// Class BootstrapFixture.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class BootstrapFixture : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BootstrapFixture"/> class.
        /// </summary>
        public BootstrapFixture()
        {
            ServiceLocator.Register<TestObjects.SingletonTest>(LifeStyle.SINGLETON);
            ServiceLocator.Register<TestObjects.SingletonWithDependencyTest>(LifeStyle.SINGLETON);

            LogConsumer.AddProvider<TestLogProvider>().SetLevel(LogLevel.ALL);
        }

        /// <summary>
        /// Sets the log.
        /// </summary>
        /// <param name="testOutputHelper">The test output helper.</param>
        public void SetLog(ITestOutputHelper testOutputHelper)
        {
            testOutputHelper.WriteLine("Tests");
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
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
