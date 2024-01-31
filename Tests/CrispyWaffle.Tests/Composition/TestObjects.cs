using System;
using System.Threading;

namespace CrispyWaffle.Tests.Composition;

/// <summary>
/// Class TestObjects.
/// </summary>
internal class TestObjects
{
    /// <summary>
    /// Class SingletonTest. This class cannot be inherited.
    /// </summary>
    public sealed class SingletonTest
    {
        /// <summary>
        /// Gets the date.
        /// </summary>
        /// <value>The date.</value>
        public DateTime Date { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonTest" /> class.
        /// </summary>
        public SingletonTest() => Date = DateTime.Now;
    }

    /// <summary>
    /// Class SingletonWithDependencyTest. This class cannot be inherited.
    /// </summary>
    public sealed class SingletonWithDependencyTest
    {
        /// <summary>
        /// Gets the singleton.
        /// </summary>
        /// <value>The singleton.</value>
        public SingletonTest Singleton { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonWithDependencyTest" /> class.
        /// </summary>
        /// <param name="singleton">The singleton.</param>
        public SingletonWithDependencyTest(SingletonTest singleton)
        {
            Singleton = singleton;
        }
    }

    /// <summary>
    /// Class CancellationTokenDependencyTest. This class cannot be inherited.
    /// </summary>
    public sealed class CancellationTokenDependencyTest
    {
        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        /// <value>The cancellation token.</value>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CancellationTokenDependencyTest" /> class.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public CancellationTokenDependencyTest(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }
    }
}
