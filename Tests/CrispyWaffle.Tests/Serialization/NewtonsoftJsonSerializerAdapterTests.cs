using System;
using System.Collections.Generic;
using System.IO;
using CrispyWaffle.Serialization.Adapters;
using FluentAssertions;
using Xunit;

namespace CrispyWaffle.Tests.Serialization;

public class NewtonsoftJsonSerializerAdapterTests
{
    private readonly NewtonsoftJsonSerializerAdapter _serializer = new();

    [Fact]
    public void ValidateSaveToFile()
    {
        // Arrange
        const string fileName = "save-json-to-file.json";
        var instance = GenerateSampleData();

        // Act
        _serializer.Save(fileName, instance);

        // Assert
        var content = File.ReadAllText(fileName);
        content.ReplaceLineEndings().Should().Be(GetStringContent().ReplaceLineEndings());
    }

    [Fact]
    public void ValidateLoadFromFile()
    {
        // Arrange
        const string fileName = "load-json-from-file.json";
        File.WriteAllText(fileName, GetStringContent());

        // Act
        var instance = _serializer.Load<SampleJsonClass>(fileName);

        // Assert
        instance.Should().NotBeNull();
        instance.Should().BeEquivalentTo(GenerateSampleData());
    }

    private static string GetStringContent() =>
        "{\n  \"Id\": \"00000000-0000-0000-0000-000000000000\",\n  \"Date\": \"2023-10-28T10:15:00\",\n  \"ListStrong\": [\n    {\n      \"CorrelationId\": \"00000000-0000-0000-0000-000000000000\",\n      \"SomeText\": \"Test\",\n      \"Date\": \"2023-10-28T10:15:00\"\n    }\n  ]\n}";

    /// <summary>
    /// Generates a sample instance of <see cref="SampleJsonClass"/> with predefined values.
    /// </summary>
    /// <returns>A new instance of <see cref="SampleJsonClass"/> populated with sample data.</returns>
    /// <remarks>
    /// This method creates and returns a <see cref="SampleJsonClass"/> object that contains a specific date, an empty GUID for the Id,
    /// and a list of <see cref="StrongTypingClass"/> objects. The list is initialized with one item that has an empty CorrelationId,
    /// a specific date, and a sample text "Test". This method is useful for testing or initializing data structures with known values.
    /// </remarks>
    private static SampleJsonClass GenerateSampleData() =>
        new()
        {
            Date = new DateTime(2023, 10, 28, 10, 15, 0, DateTimeKind.Unspecified),
            Id = Guid.Empty,
            ListStrong = new List<StrongTypingClass>
            {
                new()
                {
                    CorrelationId = Guid.Empty,
                    Date = new DateTime(2023, 10, 28, 10, 15, 0, DateTimeKind.Unspecified),
                    SomeText = "Test",
                },
            },
        };
}
