using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CrispyWaffle.Serialization.SystemTextJson;

namespace CrispyWaffle.Serialization.Adapters
{
    /// <summary>
    /// A serializer json.
    /// </summary>
    /// <seealso cref="ISerializerAdapter"/>
    public sealed class JsonSerializerAdapter : BaseSerializerAdapter
    {
        /// <summary>
        /// The settings.
        /// </summary>
        private readonly JsonSerializerOptions _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializerAdapter"/> class.
        /// </summary>
        public JsonSerializerAdapter()
        {
            _settings = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new NotNullObserverConverter() },
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializerAdapter"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public JsonSerializerAdapter(JsonSerializerOptions settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Deserialize a stream to a generic type.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="stream">The serialized object as stream.</param>
        /// <param name="encoding">
        /// (Optional) The encoding to read the stream. If null Encoding.UTF8 will be used.
        /// </param>
        /// <returns>A T.</returns>
        /// <exception cref="NotNullObserverException">
        /// Throws when a field marked as NotNullObserver finds a value.
        /// </exception>
        /// <exception cref="ArgumentNullException">Throws when a stream is null.</exception>
        public override T DeserializeFromStream<T>(Stream stream, Encoding encoding = null)
            where T : class
        {
            if (stream == null)
            {
                throw new ArgumentNullException();
            }

            try
            {
                using (var reader = new StreamReader(stream, encoding ?? Encoding.UTF8))
                {
                    var content = reader.ReadToEnd();
                    return JsonSerializer.Deserialize<T>(content, _settings);
                }
            }
            catch (NotNullObserverException e)
            {
                throw new NotNullObserverException(typeof(T), e);
            }
        }

        /// <summary>
        /// Deserializes the serialized object to a generic type.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="serialized">The serialized.</param>
        /// <returns>A T.</returns>
        /// <exception cref="NotNullObserverException">
        /// Throws when a field marked as NotNullObserver finds a value.
        /// </exception>
        public override T Deserialize<T>(object serialized)
            where T : class
        {
            try
            {
                return JsonSerializer.Deserialize<T>((string)serialized, _settings);
            }
            catch (NotNullObserverException e)
            {
                throw new NotNullObserverException(typeof(T), e);
            }
        }

        /// <summary>
        /// Serializes.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="deserialized">The deserialized.</param>
        /// <param name="stream">[in,out] The stream.</param>
        public override void Serialize<T>(T deserialized, out Stream stream)
            where T : class
        {
            stream = new MemoryStream();
            if (deserialized == null)
            {
                return;
            }

            var writerOptions = new JsonWriterOptions { Indented = _settings.WriteIndented, };

            using (var jsonWriter = new Utf8JsonWriter(stream, writerOptions))
            {
                JsonSerializer.Serialize(jsonWriter, deserialized, _settings);
            }

            stream.Seek(0, SeekOrigin.Begin);
        }
    }
}
