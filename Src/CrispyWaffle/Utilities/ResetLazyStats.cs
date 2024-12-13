using System;

namespace CrispyWaffle.Utilities;

/// <summary>
/// Represents statistics for a lazy-loaded type, including the number of loads, resets, hits, and total load time.
/// </summary>
/// <remarks>
/// This class is used to track and accumulate statistics related to a lazy-loaded type. It records the
/// number of times the type has been loaded, reset, and hit, as well as the total time spent on loading.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="ResetLazyStats"/> class for a specified type.
/// </remarks>
/// <param name="type">The type for which the statistics will be tracked.</param>
/// <remarks>
/// The constructor sets the <see cref="Type"/> property and initializes all the statistics properties to their default values.
/// </remarks>
public class ResetLazyStats(Type type)
{
    /// <summary>
    /// Gets or sets the type for which statistics are being tracked.
    /// </summary>
    /// <value>
    /// The <see cref="Type"/> of the lazy-loaded object being tracked.
    /// </value>
    public Type Type = type;

    /// <summary>
    /// Gets or sets the number of times the lazy-loaded object has been accessed.
    /// </summary>
    /// <value>
    /// The number of loads for the lazy-loaded object.
    /// </value>
    public int Loads;

    /// <summary>
    /// Gets or sets the number of times the lazy-loaded object has been reset.
    /// </summary>
    /// <value>
    /// The number of resets for the lazy-loaded object.
    /// </value>
    public int Resets;

    /// <summary>
    /// Gets or sets the number of times the lazy-loaded object has been accessed successfully.
    /// </summary>
    /// <value>
    /// The number of hits for the lazy-loaded object.
    /// </value>
    public int Hits;

    /// <summary>
    /// Gets or sets the total time spent loading the lazy-loaded object.
    /// </summary>
    /// <value>
    /// The cumulative time spent loading the lazy-loaded object, in <see cref="TimeSpan"/>.
    /// </value>
    public TimeSpan SumLoadTime;
}
