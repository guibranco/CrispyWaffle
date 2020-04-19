using System;

namespace CrispyWaffle.Tests.Composition
{
    internal partial class TestObjects
    {
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
            public SingletonTest()
            {
                Date = DateTime.Now;
            }
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
            /// Initializes a new instance of the <see cref="SingletonWithDependencyTest"/> class.
            /// </summary>
            /// <param name="singleton">The singleton.</param>
            public SingletonWithDependencyTest(SingletonTest singleton)
            {
                Singleton = singleton;
            }
        }
    }
}
