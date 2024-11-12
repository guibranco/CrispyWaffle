﻿using System;
using System.Diagnostics;
using System.Security.Permissions;
using System.Threading;

namespace CrispyWaffle.Utilities;

/// <summary>
/// The resettable lazy class. This class is based on https://stackoverflow.com/a/6255398/1890220.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="ILazyResettable" />
/// <remarks>
/// Initializes a new instance of the <see cref="LazyResettable{T}" /> class.
/// </remarks>
/// <param name="valueFactory">The value factory.</param>
/// <param name="mode">The mode.</param>
/// <param name="declaringType">Type of the declaring.</param>
/// <exception cref="ArgumentNullException">valueFactory</exception>
[HostProtection(
    Action = SecurityAction.Demand,
    Resources = HostProtectionResource.Synchronization | HostProtectionResource.SharedState
)]
public class LazyResettable<T>(
    Func<T> valueFactory,
    LazyThreadSafetyMode mode = LazyThreadSafetyMode.PublicationOnly,
    Type declaringType = null
) : ILazyResettable
{
    /// <summary>
    /// The internal class box.
    /// </summary>
    /// <seealso cref="ILazyResettable" />
    /// <remarks>
    /// Initializes a new instance of the <see cref="Box" /> class.
    /// </remarks>
    /// <param name="value">The value.</param>
    private sealed class Box(T value)
    {
        /// <summary>
        /// The value.
        /// </summary>
        public readonly T Value = value;
    }

    /// <summary>
    /// The value factory.
    /// </summary>
    private readonly Func<T> _valueFactory =
        valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));

    /// <summary>
    /// The synchronize lock.
    /// </summary>
    private readonly object _syncLock = new object();

    /// <summary>
    /// The box.
    /// </summary>
    private Box _box;

    /// <summary>
    /// The loads.
    /// </summary>
    public int Loads;

    /// <summary>
    /// The hits.
    /// </summary>
    public int Hits;

    /// <summary>
    /// The resets.
    /// </summary>
    public int Resets;

    /// <summary>
    /// The sum load time.
    /// </summary>
    public TimeSpan SumLoadTime;

    /// <summary>
    /// Occurs when [on reset].
    /// </summary>
    public event EventHandler OnReset;

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        if (mode != LazyThreadSafetyMode.None)
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
    /// <value>The type of the declaring.</value>
    public Type DeclaringType { get; } = declaringType ?? valueFactory.Method.DeclaringType;

    /// <summary>
    /// Stats this instance.
    /// </summary>
    /// <returns>ResetLazyStats.</returns>
    public ResetLazyStats Stats() =>
        new ResetLazyStats(typeof(T))
        {
            SumLoadTime = SumLoadTime,
            Hits = Hits,
            Loads = Loads,
            Resets = Resets,
        };

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>The value.</value>
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

            switch (mode)
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
    /// <value><c>true</c> if this instance is value created; otherwise, <c>false</c>.</value>
    public bool IsValueCreated => _box != null;

    /// <summary>
    /// Internals the loaded.
    /// </summary>
    /// <returns>T.</returns>
    private T InternalLoaded()
    {
        var sw = Stopwatch.StartNew();
        var result = _valueFactory();
        sw.Stop();
        SumLoadTime += sw.Elapsed;
        Interlocked.Increment(ref Loads);
        return result;
    }
}
