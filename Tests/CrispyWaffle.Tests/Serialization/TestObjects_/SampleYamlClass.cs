using System.Diagnostics.CodeAnalysis;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Tests.Serialization;

[Serializer(SerializerFormat.Yaml)]
[ExcludeFromCodeCoverage]
public class SampleYamlClass
{
    public string Name { get; set; }

    public int Age { get; set; }
}
