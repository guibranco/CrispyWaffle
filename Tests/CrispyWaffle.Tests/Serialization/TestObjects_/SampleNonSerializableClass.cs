using System;
using System.Diagnostics.CodeAnalysis;

namespace CrispyWaffle.Tests.Serialization;

/// <summary>
/// Class SampleNonSerializableClass.
/// </summary>
[ExcludeFromCodeCoverage]
public class SampleNonSerializableClass
{
    /// <summary>
    /// Gets or sets the date.
    /// </summary>
    /// <value>The date.</value>
    public DateTime Date { get; set; }
}
