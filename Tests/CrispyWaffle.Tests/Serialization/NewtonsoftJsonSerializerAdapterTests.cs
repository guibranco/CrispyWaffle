using System;
using System.IO;
using CrispyWaffle.Serialization.Adapters;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace CrispyWaffle.Tests.Serialization;

public class NewtonsoftJsonSerializerAdapterTests
{
    private readonly NewtonsoftJsonSerializerAdapter _serializer = new();

    [Fact]
    public void ValidateSaveToFile()
    {
        // Arrange
        const string fileName = "save-json-to-file-newtonsoft.json";
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
        const string fileName = "load-json-from-file-newtonsoft.json";
        File.WriteAllText(fileName, GetStringContent());

        // Act
        var instance = _serializer.Load<SampleJsonClass>(fileName);

        // Assert
        instance.Should().NotBeNull();
        instance.Should().BeEquivalentTo(GenerateSampleData());
    }

    [Fact]
    public void SerializeHonoursConfiguredNullValueHandlingIgnore()
    {
        var payload = new NullableSample { Name = "Alice", Optional = null };

        _serializer.Serialize(payload, out var stream);
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        json.Should().Contain("\"Name\": \"Alice\"");
        json.Should().NotContain("\"Optional\"");
    }

    [Fact]
    public void SerializeHonoursCustomJsonSerializerSettings()
    {
        var serializer = new NewtonsoftJsonSerializerAdapter(
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include }
        );
        var payload = new NullableSample { Name = "Alice", Optional = null };

        serializer.Serialize(payload, out var stream);
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        json.Should().Contain("\"Optional\": null");
    }

    private sealed class NullableSample
    {
        public string Name { get; set; }

        public string Optional { get; set; }
    }

    private static string GetStringContent() =>
        "{\n  \"Id\": \"00000000-0000-0000-0000-000000000000\",\n  \"Date\": \"2023-10-28T10:15:00\",\n  \"ListStrong\": [\n    {\n      \"CorrelationId\": \"00000000-0000-0000-0000-000000000000\",\n      \"SomeText\": \"Test\",\n      \"Date\": \"2023-10-28T10:15:00\"\n    }\n  ]\n}";

    private static SampleJsonClass GenerateSampleData() =>
        new()
        {
            Date = new DateTime(2023, 10, 28, 10, 15, 0, DateTimeKind.Unspecified),
            Id = Guid.Empty,
            ListStrong =
            [
                new()
                {
                    CorrelationId = Guid.Empty,
                    Date = new DateTime(2023, 10, 28, 10, 15, 0, DateTimeKind.Unspecified),
                    SomeText = "Test",
                },
            ],
        };
}
