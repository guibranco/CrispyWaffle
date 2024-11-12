using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CrispyWaffle.Tests.Serialization.TestObjects_;

[ExcludeFromCodeCoverage]
public static class TestObjects
{
    public static SampleXmlClass GetSampleXml()
    {
        var correlationId = Guid.NewGuid();
        return new SampleXmlClass
        {
            Code = new Random().Next(1, 99999),
            CorrelationId = correlationId,
            String = "Some text",
            StrongTyping = GetStrongTyping(correlationId),
        };
    }

    public static StrongTypingClass GetStrongTyping(Guid correlationId) =>
        new()
        {
            CorrelationId = correlationId,
            Date = DateTime.Now,
            SomeText = DateTime.Today.ToString("R"),
        };

    public static SampleJsonClass GetSampleJson() =>
        new()
        {
            Date = DateTime.Now,
            Id = Guid.NewGuid(),
            ListStrong =
            [
                GetStrongTyping(Guid.Empty),
                GetStrongTyping(Guid.NewGuid()),
                GetStrongTyping(Guid.NewGuid()),
            ],
        };

    public static SampleJsonNotStrictClass GetSampleJsonNotStrict() =>
        new() { Date = DateTime.Now };

    internal static SampleNonSerializableClass GetNonSerializable() =>
        new() { Date = DateTime.Now };

    public static IEnumerable<SampleXmlClass> GetEnumerableJson()
    {
        for (var i = 0; i < 1; i++)
        {
            yield return GetSampleXml();
        }
    }
}
