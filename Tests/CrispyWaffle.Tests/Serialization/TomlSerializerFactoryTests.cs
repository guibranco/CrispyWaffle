using CrispyWaffle.Serialization;
using Xunit;

namespace CrispyWaffle.Tests.Serialization;

public class TomlSerializerFactoryTests
{
    [Fact]
    public void ValidateSerializerToml()
    {
        // Arrange
        var deserialized = new SampleTomlClass { Name = "Jane Doe", Age = 25 };

        // Act
        var serializedResult = (string)deserialized.GetSerializer();
        var deserializedResult = SerializerFactory
            .GetSerializer<SampleTomlClass>()
            .Deserialize(serializedResult);

        // Assert
        Assert.Contains("name = \"Jane Doe\"", serializedResult);
        Assert.Contains("age = 25", serializedResult);
        Assert.Equal(deserialized.Name, deserializedResult.Name);
        Assert.Equal(deserialized.Age, deserializedResult.Age);
    }

    [Fact]
    public void ValidateCustomSerializerToml()
    {
        // Arrange
        var deserialized = new SampleTomlClass { Name = "Jane Doe", Age = 25 };

        // Act
        var serializer = deserialized.GetCustomSerializer(SerializerFormat.Toml);
        var serializedResult = (string)serializer;
        var deserializedResult = serializer.Deserialize(serializedResult);

        // Assert
        Assert.Equal(deserialized.Name, deserializedResult.Name);
        Assert.Equal(deserialized.Age, deserializedResult.Age);
    }
}
