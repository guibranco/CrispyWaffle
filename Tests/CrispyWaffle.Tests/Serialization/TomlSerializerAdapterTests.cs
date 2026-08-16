using System;
using System.IO;
using System.Text;
using CrispyWaffle.Serialization.Adapters;
using FluentAssertions;
using Tomlyn;
using Xunit;

namespace CrispyWaffle.Tests.Serialization;

public class TomlSerializerAdapterTests
{
    private readonly TomlSerializerAdapter _serializer = new();

    [Fact]
    public void SerializeWithValidObjectReturnsToml()
    {
        // Arrange
        var instance = GenerateSampleData();

        // Act
        _serializer.Serialize(instance, out var stream);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var toml = reader.ReadToEnd();

        // Assert
        toml.Should().Contain("name = \"Jane Doe\"");
        toml.Should().Contain("age = 25");
    }

    [Fact]
    public void SerializeWithNullObjectProducesEmptyStream()
    {
        // Act
        _serializer.Serialize<SampleTomlClass>(null, out var stream);

        using (stream)
        {
            // Assert
            stream.Should().NotBeNull();
            stream.CanRead.Should().BeTrue();
            stream.Length.Should().Be(0);
            stream.Position.Should().Be(0);
        }
    }

    [Fact]
    public void SerializeToStringWithNullReturnsEmptyString()
    {
        // Act
        var result = _serializer.SerializeToString<SampleTomlClass>(null);

        // Assert
        result.Should().Be(string.Empty);
    }

    [Fact]
    public void DeserializeWithValidTomlReturnsObject()
    {
        // Arrange
        const string toml = "name = \"Jane Doe\"\nage = 25\n";

        // Act
        var result = _serializer.Deserialize<SampleTomlClass>(toml);

        // Assert
        result.Should().BeEquivalentTo(GenerateSampleData());
    }

    [Fact]
    public void DeserializeWithDuplicateKeysThrowsTomlException()
    {
        // Arrange
        const string toml = "name = \"Jane Doe\"\nname = \"John Doe\"\nage = 25\n";

        // Act
        Action act = () => _serializer.Deserialize<SampleTomlClass>(toml);

        // Assert
        act.Should().Throw<TomlException>();
    }

    [Fact]
    public void DeserializeWithNullSerializedThrowsArgumentNullException()
    {
        // Act
        Action act = () => _serializer.Deserialize<SampleTomlClass>(null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("serialized");
    }

    [Fact]
    public void DeserializeWithNonStringThrowsArgumentException()
    {
        // Act
        Action act = () => _serializer.Deserialize<SampleTomlClass>(25);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DeserializeFromStreamWithValidTomlReturnsObjectAndKeepsStreamOpen()
    {
        // Arrange
        const string toml = "name = \"Jane Doe\"\nage = 25\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(toml));

        // Act
        var result = _serializer.DeserializeFromStream<SampleTomlClass>(stream);

        // Assert
        result.Should().BeEquivalentTo(GenerateSampleData());
        stream.CanRead.Should().BeTrue();
    }

    [Fact]
    public void SaveAndLoadRoundTripReturnsEquivalentObject()
    {
        // Arrange
        var fileName = Path.GetTempFileName();
        var instance = GenerateSampleData();

        try
        {
            // Act
            _serializer.Save(fileName, instance);
            var result = _serializer.Load<SampleTomlClass>(fileName);

            // Assert
            result.Should().BeEquivalentTo(instance);
        }
        finally
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
    }

    [Fact]
    public void DeserializeFromNullStreamThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _serializer.DeserializeFromStream<SampleTomlClass>(null)
        );
    }

    private static SampleTomlClass GenerateSampleData() => new() { Name = "Jane Doe", Age = 25 };
}
