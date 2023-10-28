// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 09-05-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="SerializerFactoryTests.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Linq;
using CrispyWaffle.Serialization;
using Xunit;

namespace CrispyWaffle.Tests.Serialization
{
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

        //[Fact]
        //public void ValidateSerializerJsonStrict()
        //{
        //    var deserialized = TestObjects.GetSampleJson();

        //    var serializedResult = (string)deserialized.GetSerializer();

        //    var deserializedResult = SerializerFactory.GetSerializer<SampleJsonClass>().Deserialize(serializedResult);

        //    Assert.Equal(deserialized, deserializedResult);
        //}

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

        //[Fact]
        //public void ValidateGetSerializerFromInstanceJson()
        //{
        //    var deserialized = TestObjects.GetSampleJson();

        //    var deserializedInstance = TestObjects.GetSampleJson();

        //    var serializedResult = (string)deserialized.GetSerializer();

        //    // ReSharper disable once InvokeAsExtensionMethod
        //    var deserializedResult = SerializerFactory.GetSerializer(deserializedInstance).Deserialize(serializedResult);

        //    Assert.Equal(deserialized, deserializedResult);

        //    Assert.NotEqual(deserializedInstance, deserializedResult);
        //}

        //[Fact]
        //public void ValidateGetSerializerFromInstanceExtensionMethodJson()
        //{
        //    var deserialized = TestObjects.GetSampleJson();

        //    var deserializedInstance = TestObjects.GetSampleJson();

        //    var serializedResult = (string)deserialized.GetSerializer();

        //    var deserializedResult = deserializedInstance.GetSerializer().Deserialize(serializedResult);

        //    Assert.Equal(deserialized, deserializedResult);

        //    Assert.NotEqual(deserializedInstance, deserializedResult);

        //}

        [Fact]
        public void ValidateInvalidTypeSerialization()
        {
            var deserialized = TestObjects.GetNonSerializable();

            var result = Assert.Throws<InvalidOperationException>(
                () => deserialized.GetSerializer()
            );

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

            var deserializedResult = deserializedObject
                .GetSerializer()
                .Deserialize(serializedResult);

            Assert.Equal(deserialized, deserializedResult);
        }

        //[Fact]
        //public void ChangeName()
        //{
        //    const string serialized = "{\r\n  \"Id\": \"cabc8de0-b57a-44e2-9401-f8b7a46565f3\",\r\n  \"Date\": \"2020-09-05T23:24:57.6493849-03:00\",\r\n  \"ListStrong\": [\r\n    {\r\n      \"CorrelationId\": \"00000000-0000-0000-0000-000000000000\",\r\n      \"SomeText\": \"2020-09-05T23:24:57.6498435-03:00\",\r\n      \"Date\": \"2020-09-05T23:24:57.6497889-03:00\"\r\n    },\r\n    {\r\n      \"CorrelationId\": \"3bd67cac-f408-4a86-9d23-1217d93778d7\",\r\n      \"SomeText\": \"2020-09-05T23:24:57.6501691-03:00\",\r\n      \"Date\": \"2020-09-05T23:24:57.6501681-03:00\"\r\n    },\r\n    {\r\n      \"CorrelationId\": \"fd2d1745-c7af-45b2-b6ea-0b1e6f658555\",\r\n      \"SomeText\": \"2020-09-05T23:24:57.6501719-03:00\",\r\n      \"Date\": \"2020-09-05T23:24:57.6501718-03:00\"\r\n    }\r\n  ]\r\n}";


        //}
    }
}
