using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrispyWaffle.Serialization.SystemTextJson
{
    /// <summary>
    /// The not null observer converter class.
    /// This class notifies when a usual null property receives a value.
    /// </summary>
    public sealed class NotNullObserverConverter : JsonConverter<NotNullObserver>
    {
        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read from.</param>
        /// <param name="typeToConvert">Type of the object.</param>
        /// <param name="options">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override NotNullObserver Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var type = reader.TokenType;

            if (type == JsonTokenType.Null)
            {
                return null;
            }

            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(root.GetString()))
                {
                    return null;
                }

                if (!root.EnumerateObject().Any() && (root.ValueKind == JsonValueKind.Array || root.ValueKind == JsonValueKind.Object))
                {
                    return null;
                }

                throw new NotNullObserverException(type, root.ToString());
            }
        }

        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="options">The calling serializer.</param>
        public override void Write(Utf8JsonWriter writer, NotNullObserver value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
