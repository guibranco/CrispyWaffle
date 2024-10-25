using System;
using Xunit;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Tests.Serialization
{
    public class YamlSerializerTests
    {
        private readonly YamlSerializer _serializer;

        public YamlSerializerTests()
        {
            _serializer = new YamlSerializer();
        }

        [Fact]
        public void Serialize_ShouldReturnYamlString()
        {
            var obj = new { Name = "Test", Value = 123 };
            var yaml = _serializer.Serialize(obj);
            Assert.Contains("name: Test", yaml);
            Assert.Contains("value: 123", yaml);
        }

        [Fact]
        public void Deserialize_ShouldReturnObject()
        {
            var yaml = "name: Test\nvalue: 123";
            var obj = _serializer.Deserialize<dynamic>(yaml);
            Assert.Equal("Test", (string)obj.Name);
            Assert.Equal(123, (int)obj.Value);
        }
    }
}
