using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CrispyWaffle.Serialization
{
    public class YamlSerializer
    {
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;

        public YamlSerializer()
        {
            _serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            _deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        }

        public string Serialize<T>(T obj) => _serializer.Serialize(obj);
        public T Deserialize<T>(string yaml) => _deserializer.Deserialize<T>(yaml);
    }
}