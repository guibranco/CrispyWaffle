namespace CrispyWaffle.Composition
{
    using System;
    using System.Threading;

    /// <summary>
    ///  A <see cref="Lazy{T}"/> object that implements <see cref="IDisposable"/>.
    /// </summary>
    /// <typeparam name="T">
    ///  The object being lazily created.
    /// </typeparam>

    public class LazyDisposable<T> : Lazy<T>, IDisposable where T : IDisposable
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="LazyDisposable{T}"/> class.
        ///  When lazy initialization occurs, the default constructor is used.
        /// </summary>
        public LazyDisposable()
        { }

        /// <summary>
        ///  Initializes a new instance of the <see cref="LazyDisposable{T}"/> class.
        ///  When lazy initialization occurs, the default constructor of the target type
        ///  and the specified initialization mode are used.
        /// </summary>
        /// <param name="isThreadSafe">
        ///  true to make this instance usable concurrently by multiple threads;
        ///  false to make the instance usable by only one thread at a time. 
        /// </param>
        public LazyDisposable(bool isThreadSafe) : base(isThreadSafe) { }

        /// <summary>
        ///  Initializes a new instance of the <see cref="LazyDisposable{T}"/> class
        ///  that uses the default constructor of T and the specified thread-safety mode.
        /// </summary>
        /// <param name="mode">
        ///  One of the enumeration values that specifies the thread safety mode. 
        /// </param>
        public LazyDisposable(LazyThreadSafetyMode mode) : base(mode) { }

        /// <summary>
        ///  Initializes a new instance of the <see cref="LazyDisposable{T}"/> class.
        ///  When lazy initialization occurs, the specified initialization function is used.
        /// </summary>
        /// <param name="valueFactory">
        ///  The delegate that is invoked to produce the lazily initialized value when it is needed. 
        /// </param>
        public LazyDisposable(Func<T> valueFactory) : base(valueFactory) { }

        /// <summary>
        ///  Initializes a new instance of the <see cref="LazyDisposable{T}"/> class.
        ///  When lazy initialization occurs, the specified initialization function
        ///  and initialization mode are used.
        /// </summary>
        /// <param name="valueFactory">
        ///  The delegate that is invoked to produce the lazily initialized value when it is needed. 
        /// </param>
        /// <param name="isThreadSafe">
        ///  true to make this instance usable concurrently by multiple threads;
        ///  false to make this instance usable by only one thread at a time. 
        /// </param>
        public LazyDisposable(Func<T> valueFactory, bool isThreadSafe) : base(valueFactory, isThreadSafe) { }

        /// <summary>
        ///  Initializes a new instance of the <see cref="LazyDisposable{T}"/> class
        ///  using the specified initialization function and thread-safety mode.
        /// </summary>
        /// <param name="valueFactory">
        ///  The delegate that is invoked to produce the lazily initialized value when it is needed. 
        /// </param>
        /// <param name="mode">
        ///  One of the enumeration values that specifies the thread safety mode. 
        /// </param>
        public LazyDisposable(Func<T> valueFactory, LazyThreadSafetyMode mode) : base(valueFactory, mode) { }

        /// <summary>
        ///  Performs tasks defined in the created instance associated with freeing, releasing,
        ///  or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the specified disposing.
        /// </summary>
        /// <param name="disposing">if set to <c>true</c> [disposing].</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && IsValueCreated)
            {
                Value.Dispose();
            }
        }
    }
}
