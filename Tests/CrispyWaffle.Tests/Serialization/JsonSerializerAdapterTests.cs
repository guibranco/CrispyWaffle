using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using CrispyWaffle.Serialization;
using CrispyWaffle.Serialization.Adapters;
using CrispyWaffle.Serialization.SystemTextJson;
using FluentAssertions;
using Xunit;

namespace CrispyWaffle.Tests.Serialization;

public class JsonSerializerAdapterTests
{
    private readonly JsonSerializerAdapter _serializer = new();

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

    [Fact]
    public void SerializeWithNonNullObjectReturnsValidJson()
    {
        // Arrange
        var testObject = new TestObject { Property = "Test" };
        Stream resultStream;

        // Act
        _serializer.Serialize(testObject, out resultStream);
        resultStream.Seek(0, SeekOrigin.Begin);
        var serializedJson = new StreamReader(resultStream).ReadToEnd();

        // Assert
        serializedJson.Should().NotBeNullOrEmpty();
        serializedJson
            .ReplaceLineEndings()
            .Should()
            .BeEquivalentTo("{\r\n  \"Property\": \"Test\"\r\n}".ReplaceLineEndings());
    }

    [Fact]
    public void DeserializeWithValidJsonReturnsObject()
    {
        // Arrange
        var json = "{\"Property\":\"Test\"}";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        // Act
        var result = _serializer.DeserializeFromStream<TestObject>(stream);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<TestObject>();
        result.Property.Should().Be("Test");
    }

    [Fact]
    public void DeserializeNonEmptyJsonThrowsNotNullObserverException()
    {
        // Arrange
        var json = "{\"UnknownProperty\":\"Value\"}";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        // Act & Assert
        Assert.Throws<NotNullObserverException>(
            () => _serializer.DeserializeFromStream<NotNullObserver>(stream)
        );
    }

    [Fact]
    public void SerializeWithNullValueIgnoresNullProperty()
    {
        // Arrange
        var testObject = new TestObject { Property = null };
        Stream resultStream;

        // Act
        _serializer.Serialize(testObject, out resultStream);
        resultStream.Seek(0, SeekOrigin.Begin);
        var serializedJson = new StreamReader(resultStream).ReadToEnd();

        // Assert
        serializedJson.Should().NotBeNull();
        serializedJson.Should().NotContain("\"Property\":");
    }

    [Fact]
    public void DeserializeWithInvalidJsonThrowsException()
    {
        // Arrange
        var invalidJson = "Invalid JSON";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));

        // Act & Assert
        Assert.Throws<JsonException>(() => _serializer.DeserializeFromStream<TestObject>(stream));
    }

    // Additional tests for Custom Converter, Error Handling, etc.

    private class TestObject
    {
        public string Property { get; set; }
    }

    private static string GetStringContent() =>
        "{\n  \"Id\": \"00000000-0000-0000-0000-000000000000\",\n  \"Date\": \"2023-10-28T10:15:00\",\n  \"ListStrong\": [\n    {\n      \"CorrelationId\": \"00000000-0000-0000-0000-000000000000\",\n      \"SomeText\": \"Test\",\n      \"Date\": \"2023-10-28T10:15:00\"\n    }\n  ]\n}";

    /// <summary>
    /// Generates a sample instance of <see cref="SampleJsonClass"/> with predefined values.
    /// </summary>
    /// <returns>A new instance of <see cref="SampleJsonClass"/> populated with sample data.</returns>
    /// <remarks>
    /// This method creates a sample JSON object for testing or demonstration purposes.
    /// The generated object includes a specific date, an empty GUID for the Id, and a list containing one instance of <see cref="StrongTypingClass"/>.
    /// The <see cref="StrongTypingClass"/> instance is initialized with an empty CorrelationId, a specific date, and a sample text "Test".
    /// This method is useful for scenarios where you need to provide mock data without relying on external sources.
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
