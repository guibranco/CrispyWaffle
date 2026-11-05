using System;
using System.Text.Json;

namespace CrispyWaffle.HttpClient.Serialization;

/// <summary>
/// The default <see cref="IJsonSerializer"/> implementation, based on <see cref="JsonSerializer"/>.
/// </summary>
public class SystemTextJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextJsonSerializer"/> class.
    /// </summary>
    /// <param name="options">
    /// The serializer options to use. When <c>null</c>, the serializer uses camelCase property names,
    /// case-insensitive property matching and leaves <c>null</c> values out when writing.
    /// </param>
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

    /// <inheritdoc />
    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, _options);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Returns the default value of <typeparamref name="T"/> when <paramref name="json"/> is <c>null</c>, empty or white space.
    /// </remarks>
    public T? Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;
        return JsonSerializer.Deserialize<T>(json, _options);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Returns <c>null</c> when <paramref name="json"/> is <c>null</c>, empty or white space.
    /// </remarks>
    public object? Deserialize(string json, Type type)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        return JsonSerializer.Deserialize(json, type, _options);
    }
}
