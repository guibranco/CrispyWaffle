using System;
using System.Diagnostics.CodeAnalysis;

namespace CrispyWaffle.Tests.Serialization;

[ExcludeFromCodeCoverage]
public class SampleNonSerializableClass
{
    public DateTime Date { get; set; }
}
