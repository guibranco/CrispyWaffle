using CrispyWaffle.Composition;
using CrispyWaffle.Tests.Composition;
using System;
using Xunit.Abstractions;

namespace CrispyWaffle.Tests
{
    public class BootstrapFixture : IDisposable
    {
        public BootstrapFixture()
        {
            ServiceLocator.Register<TestObjects.SingletonTest>(LifeStyle.SINGLETON);
            ServiceLocator.Register<TestObjects.SingletonWithDependencyTest>(LifeStyle.SINGLETON);

            //LogConsumer.AddProvider<TestLogProvider>().SetLevel(LogLevel.ALL);
        }

        public void SetLog(ITestOutputHelper testOutputHelper)
        {
            testOutputHelper.WriteLine("teste");
        }

        #region IDisposable Support

        private bool _disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
                return;
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
