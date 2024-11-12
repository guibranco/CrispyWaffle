using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrispyWaffle.Serialization.NewtonsoftJson;

/// <summary>
/// A custom <see cref="JsonConverter"/> for handling the serialization and deserialization of the <see cref="NotNullObserver"/> type.
/// </summary>
/// <remarks>
/// This converter ensures that a <see cref="NotNullObserver"/> object is not deserialized with null, empty, or whitespace-only string values,
/// and it throws a <see cref="NotNullObserverException"/> when such invalid values are encountered.
/// </remarks>
public sealed class NotNullObserverConverter : JsonConverter
{
    /// <summary>
    /// Serializes a <see cref="NotNullObserver"/> object to JSON.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> used to write the JSON data.</param>
    /// <param name="value">The object being serialized.</param>
    /// <param name="serializer">The <see cref="JsonSerializer"/> used to perform the serialization.</param>
    /// <remarks>
    /// This method calls the default serialization mechanism for writing a <see cref="NotNullObserver"/> object to JSON.
    /// </remarks>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
        serializer.Serialize(writer, value);

    /// <summary>
    /// Deserializes JSON into a <see cref="NotNullObserver"/> object, ensuring that invalid values (null, empty, or whitespace strings)
    /// are not allowed.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> used to read the JSON data.</param>
    /// <param name="objectType">The type of the object to be deserialized. This should be <see cref="NotNullObserver"/>.</param>
    /// <param name="existingValue">The existing value of the object being deserialized, or <c>null</c> if none.</param>
    /// <param name="serializer">The <see cref="JsonSerializer"/> used to perform the deserialization.</param>
    /// <returns>A deserialized <see cref="NotNullObserver"/> object.</returns>
    /// <exception cref="NotNullObserverException">
    /// Thrown when the JSON value is invalid, such as when the value is null, empty, or a whitespace-only string,
    /// or when the value is an empty array or object.
    /// </exception>
    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer
    )
    {
        var type = reader.TokenType;

        if (type == JsonToken.Null)
        {
            return null;
        }

        var value = JToken.Load(reader);

        if (type == JsonToken.String && string.IsNullOrWhiteSpace(value.Value<string>()))
        {
            return null;
        }

        if (!value.HasValues && (type == JsonToken.StartArray || type == JsonToken.StartObject))
        {
            return null;
        }

        throw new NotNullObserverException(type, reader.Value ?? value.ToString(), reader.Path);
    }

    /// <summary>
    /// Determines whether this converter can convert the specified type.
    /// </summary>
    /// <param name="objectType">The type to check for compatibility.</param>
    /// <returns><c>true</c> if this converter can handle the specified type; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This converter is designed specifically for the <see cref="NotNullObserver"/> type.
    /// </remarks>
    public override bool CanConvert(Type objectType) => objectType == typeof(NotNullObserver);
}