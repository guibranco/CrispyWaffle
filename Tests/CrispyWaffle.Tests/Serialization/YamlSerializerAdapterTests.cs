using System;
using System.IO;
using System.Text;
using CrispyWaffle.Serialization.Adapters;
using FluentAssertions;
using YamlDotNet.Core;
using Xunit;

namespace CrispyWaffle.Tests.Serialization;

public class YamlSerializerAdapterTests
{
    private readonly YamlSerializerAdapter _serializer = new();

    [Fact]
    public void SerializeWithValidObjectReturnsYaml()
    {
        // Arrange
        var instance = GenerateSampleData();

        // Act
        _serializer.Serialize(instance, out var stream);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var yaml = reader.ReadToEnd();

        // Assert
        yaml.Should().Contain("name: Jane Doe");
        yaml.Should().Contain("age: 25");
    }

    [Fact]
    public void SerializeWithNullObjectProducesEmptyStream()
    {
        // Act
        _serializer.Serialize<SampleYamlClass>(null, out var stream);

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
    public void DeserializeWithValidYamlReturnsObject()
    {
        // Arrange
        const string yaml = "name: Jane Doe\nage: 25\n";

        // Act
        var result = _serializer.Deserialize<SampleYamlClass>(yaml);

        // Assert
        result.Should().BeEquivalentTo(GenerateSampleData());
    }

    [Fact]
    public void DeserializeWithDuplicateKeysThrowsYamlException()
    {
        // Arrange
        const string yaml = "name: Jane Doe\nname: John Doe\nage: 25\n";

        // Act
        Action act = () => _serializer.Deserialize<SampleYamlClass>(yaml);

        // Assert
        act.Should().Throw<YamlException>();
    }

    [Fact]
    public void DeserializeFromStreamWithValidYamlReturnsObjectAndKeepsStreamOpen()
    {
        // Arrange
        const string yaml = "name: Jane Doe\nage: 25\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(yaml));

        // Act
        var result = _serializer.DeserializeFromStream<SampleYamlClass>(stream);

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
            var result = _serializer.Load<SampleYamlClass>(fileName);

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
            _serializer.DeserializeFromStream<SampleYamlClass>(null)
        );
    }

    private static SampleYamlClass GenerateSampleData() => new() { Name = "Jane Doe", Age = 25 };
}
