using System;
using System.Text.Json;

namespace CrispyWaffle.HttpClient.Serialization
{
    public class SystemTextJsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerOptions _options;

        public SystemTextJsonSerializer(JsonSerializerOptions? options = null)
        {
            _options =
                options
                ?? new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System
                        .Text
                        .Json
                        .Serialization
                        .JsonIgnoreCondition
                        .WhenWritingNull,
                };
        }

        public string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, _options);
        }

        public T? Deserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;
            return JsonSerializer.Deserialize<T>(json, _options);
        }

        public object? Deserialize(string json, Type type)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;
            return JsonSerializer.Deserialize(json, type, _options);
        }
    }
}
