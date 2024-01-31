using System;
using System.Linq;
using CrispyWaffle.Serialization;
using Xunit;

namespace CrispyWaffle.Tests.Serialization;

/// <summary>
/// Class SerializerFactoryTests.
/// </summary>
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

        var deserializedResult = SerializerFactory.GetSerializer(deserializedInstance)
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
