using System;
using System.Diagnostics.CodeAnalysis;

namespace CrispyWaffle.Tests.Serialization.TestObjects_;

[ExcludeFromCodeCoverage]
public class SampleNonSerializableClass
{
    public DateTime Date { get; set; }
}
