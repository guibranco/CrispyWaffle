using System;

namespace CrispyWaffle.HttpClient.Serialization;

/// <summary>
/// Defines the JSON serializer used by <see cref="HttpClientWrapper"/> for request and response bodies.
/// Implementations must be thread-safe, because the registered instance is shared as a singleton.
/// </summary>
public interface IJsonSerializer
{
    /// <summary>
    /// Serializes a value to its JSON representation.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The JSON string that represents <paramref name="value"/>.</returns>
    string Serialize<T>(T value);

    /// <summary>
    /// Deserializes a JSON string into an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON into.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized instance of <typeparamref name="T"/>.</returns>
    T? Deserialize<T>(string json);

    /// <summary>
    /// Deserializes a JSON string into an instance of the given type.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="type">The type to deserialize the JSON into.</param>
    /// <returns>The deserialized instance of <paramref name="type"/>.</returns>
    object? Deserialize(string json, Type type);
}
