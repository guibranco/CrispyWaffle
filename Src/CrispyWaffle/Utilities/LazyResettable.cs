namespace CrispyWaffle.Utilities
{
    using System;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.Threading;

    /// <summary>
    /// The resettable lazy class.
    /// This class is based on https://stackoverflow.com/a/6255398/1890220
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="ILazyResettable" />
    [HostProtection(Action = SecurityAction.Demand, Resources = HostProtectionResource.Synchronization | HostProtectionResource.SharedState)]
    public class LazyResettable<T> : ILazyResettable
    {
        #region Internal classes

        /// <summary>
        /// The internal class box
        /// </summary>
        /// <seealso cref="ILazyResettable" />
        private sealed class Box
        {
            /// <summary>
            /// The value
            /// </summary>
            public readonly T Value;

            /// <summary>
            /// Initializes a new instance of the <see cref="Box"/> class.
            /// </summary>
            /// <param name="value">The value.</param>
            public Box(T value) => Value = value;
        }

        #endregion

        #region Private fields

        /// <summary>
        /// The mode
        /// </summary>
        private readonly LazyThreadSafetyMode _mode;
        /// <summary>
        /// The value factory
        /// </summary>
        private readonly Func<T> _valueFactory;
        /// <summary>
        /// The synchronize lock
        /// </summary>
        private readonly object _syncLock = new object();
        /// <summary>
        /// The box
        /// </summary>
        private Box _box;

        public int Loads;
        public int Hits;
        public int Resets;
        public TimeSpan SumLoadTime;

        #endregion

        #region ~Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyResettable{T}"/> class.
        /// </summary>
        /// <param name="valueFactory">The value factory.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="declaringType">Type of the declaring.</param>
        /// <exception cref="ArgumentNullException">valueFactory</exception>
        public LazyResettable(
            Func<T> valueFactory,
            LazyThreadSafetyMode mode = LazyThreadSafetyMode.PublicationOnly,
            Type declaringType = null)
        {
            _mode = mode;
            _valueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
            DeclaringType = declaringType ?? valueFactory.Method.DeclaringType;
        }

        #endregion

        #region Implementation of ILazyResettable

        public event EventHandler OnReset;

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            if (_mode != LazyThreadSafetyMode.None)
            {
                lock (_syncLock)
                {
                    _box = null;
                }
            }
            else
            {
                _box = null;
            }

            Interlocked.Increment(ref Resets);
            OnReset?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        public void Load()
        {
            var _ = Value;
        }

        /// <summary>
        /// Gets the type of the declaring.
        /// </summary>
        /// <value>
        /// The type of the declaring.
        /// </value>
        public Type DeclaringType { get; }

        public ResetLazyStats Stats() =>
            new ResetLazyStats(typeof(T))
            {
                SumLoadTime = SumLoadTime,
                Hits = Hits,
                Loads = Loads,
                Resets = Resets,
            };

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public T Value
        {
            get
            {
                var b1 = _box;
                if (b1 != null)
                {
                    Interlocked.Increment(ref Hits);
                    return b1.Value;
                }

                switch (_mode)
                {
                    case LazyThreadSafetyMode.ExecutionAndPublication:
                        lock (_syncLock)
                        {
                            var b2 = _box;
                            if (b2 != null)
                            {
                                return b2.Value;
                            }

                            _box = new Box(InternalLoaded());
                            return _box.Value;
                        }
                    case LazyThreadSafetyMode.PublicationOnly:
                        var newValue = InternalLoaded();
                        lock (_syncLock)
                        {
                            var b2 = _box;
                            if (b2 != null)
                            {
                                return b2.Value;
                            }

                            _box = new Box(newValue);
                            return _box.Value;
                        }
                }
                var b = new Box(InternalLoaded());
                _box = b;
                return b.Value;
            }

        }

        /// <summary>
        /// Gets a value indicating whether this instance is value created.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is value created; otherwise, <c>false</c>.
        /// </value>
        public bool IsValueCreated => _box != null;

        #endregion

        #region Private methods

        private T InternalLoaded()
        {
            var sw = Stopwatch.StartNew();
            var result = _valueFactory();
            sw.Stop();
            this.SumLoadTime += sw.Elapsed;
            Interlocked.Increment(ref Loads);
            return result;
        }

        #endregion
    }
}
