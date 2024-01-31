using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrispyWaffle.Serialization.NewtonsoftJson
{
    /// <summary>
    /// The not null observer converter class.
    /// This class notifies when a usual null property receives a value.
    /// </summary>
    /// <seealso cref="JsonConverter" />
    public sealed class NotNullObserverConverter : JsonConverter
    {
        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
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
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NotNullObserver);
        }
    }
}
