using System;
using System.Threading;

namespace CrispyWaffle.Tests.Composition;

internal class TestObjects
{
    public sealed class SingletonTest
    {
        public DateTime Date { get; }

        public SingletonTest() => Date = DateTime.Now;
    }

    public sealed class SingletonWithDependencyTest
    {
        public SingletonTest Singleton { get; }

        public SingletonWithDependencyTest(SingletonTest singleton) => Singleton = singleton;
    }

    public sealed class CancellationTokenDependencyTest
    {
        public CancellationToken CancellationToken { get; }

        public CancellationTokenDependencyTest(CancellationToken cancellationToken) =>
            CancellationToken = cancellationToken;
    }
}
