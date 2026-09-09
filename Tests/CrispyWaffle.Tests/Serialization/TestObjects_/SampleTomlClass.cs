using System.Diagnostics.CodeAnalysis;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Tests.Serialization;

[Serializer(SerializerFormat.Toml)]
[ExcludeFromCodeCoverage]
public class SampleTomlClass
{
    public string Name { get; set; }

    public int Age { get; set; }
}
