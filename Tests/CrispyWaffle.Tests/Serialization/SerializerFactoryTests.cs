using System;
using System.Linq;
using CrispyWaffle.Serialization;
using Xunit;

namespace CrispyWaffle.Tests.Serialization;

public class SerializerFactoryTests
{
    [Fact]
    public void ValidateSerializerXml()
    {
        var deserialized = TestObjects.GetSampleXml();

        var serializedResult = (string)deserialized.GetSerializer();

        var deserializedResult = SerializerFactory
            .GetSerializer<SampleXmlClass>()
            .Deserialize(serializedResult);

        Assert.Equal(deserialized, deserializedResult);
    }

    [Fact]
    public void ValidateSerializerJsonStrict()
    {
        var deserialized = TestObjects.GetSampleJson();

        var serializedResult = (string)deserialized.GetSerializer();

        var deserializedResult = SerializerFactory
            .GetSerializer<SampleJsonClass>()
            .Deserialize(serializedResult);

        Assert.Equal(deserialized, deserializedResult);
    }

    [Fact]
    public void ValidateSerializerYaml()
    {
        var deserialized = new SampleYamlClass { Name = "Jane Doe", Age = 25 };

        var serializedResult = (string)deserialized.GetSerializer();

        Assert.Contains("name: Jane Doe", serializedResult);
        Assert.Contains("age: 25", serializedResult);

        var deserializedResult = SerializerFactory
            .GetSerializer<SampleYamlClass>()
            .Deserialize(serializedResult);

        Assert.Equal(deserialized.Name, deserializedResult.Name);
        Assert.Equal(deserialized.Age, deserializedResult.Age);
    }

    [Fact]
    public void ValidateGetSerializerJsonNotStrict()
    {
        var deserialized = TestObjects.GetSampleJsonNotStrict();

        var serializedResult = (string)deserialized.GetSerializer();

        var deserializedResult = SerializerFactory
            .GetSerializer<SampleJsonNotStrictClass>()
            .Deserialize(serializedResult);

        Assert.Equal(deserialized, deserializedResult);
    }

    [Fact]
    public void ValidateGetSerializerFromInstanceJson()
    {
        var deserialized = TestObjects.GetSampleJson();

        var deserializedInstance = TestObjects.GetSampleJson();

        var serializedResult = (string)deserialized.GetSerializer();

        var deserializedResult = SerializerFactory
            .GetSerializer(deserializedInstance)
            .Deserialize(serializedResult);

        Assert.Equal(deserialized, deserializedResult);

        Assert.NotEqual(deserializedInstance, deserializedResult);
    }

    [Fact]
    public void ValidateGetSerializerFromInstanceExtensionMethodJson()
    {
        var deserialized = TestObjects.GetSampleJson();

        var deserializedInstance = TestObjects.GetSampleJson();

        var serializedResult = (string)deserialized.GetSerializer();

        var deserializedResult = deserializedInstance.GetSerializer().Deserialize(serializedResult);

        Assert.Equal(deserialized, deserializedResult);

        Assert.NotEqual(deserializedInstance, deserializedResult);
    }

    [Fact]
    public void ValidateInvalidTypeSerialization()
    {
        var deserialized = TestObjects.GetNonSerializable();

        var result = Assert.Throws<InvalidOperationException>(() => deserialized.GetSerializer());

        Assert.Equal(
            "The CrispyWaffle.Serialization.SerializerAttribute attribute was not found in the object of type CrispyWaffle.Tests.Serialization.SampleNonSerializableClass",
            result.Message
        );
    }

    [Fact]
    public void ValidateGetSerializerFromArray()
    {
        var deserialized = new[] { TestObjects.GetSampleXml(), TestObjects.GetSampleXml() };

        var serializedResult = (string)deserialized.GetSerializer();

        var deserializedResult = deserialized.GetSerializer().Deserialize(serializedResult);

        Assert.Equal(deserialized, deserializedResult);
    }

    [Fact]
    public void ValidateGetSerializerFromInherit()
    {
        var deserialized = TestObjects.GetEnumerableJson().ToList();

        var deserializedObject = TestObjects.GetEnumerableJson().ToList();

        var serializedResult = (string)deserialized.GetSerializer();

        var deserializedResult = deserializedObject.GetSerializer().Deserialize(serializedResult);

        Assert.Equal(deserialized, deserializedResult);
    }
}
