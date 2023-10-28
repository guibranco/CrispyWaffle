using System;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace CrispyWaffle.Serialization.Adapters
{
    /// <summary>
    /// A serializer json.
    /// </summary>
    /// <seealso cref="ISerializerAdapter" />
    public sealed class JsonSerializerAdapter : BaseSerializerAdapter
    {
        /// <summary>
        /// The settings
        /// </summary>
        private readonly JsonSerializerSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializerAdapter"/> class.
        /// </summary>
        public JsonSerializerAdapter()
        {
            _settings = new JsonSerializerSettings
            {
                Converters = { new NotNullObserverConverter() },
                Culture = CultureInfo.GetCultureInfo("pt-br"),
                MissingMemberHandling = MissingMemberHandling.Error,
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializerAdapter"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public JsonSerializerAdapter(JsonSerializerSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Deserialize a stream to a generic type
        /// </summary>
        /// <typeparam name="T">Generic type parameter</typeparam>
        /// <param name="stream">The serialized object as stream.</param>
        /// <param name="encoding">(Optional)  The encoding to read the stream. If null Encoding.UTF8 will be used.</param>
        /// <returns>
        /// A T.
        /// </returns>
        /// <exception cref="NotNullObserverException"></exception>
        public override T DeserializeFromStream<T>(Stream stream, Encoding encoding = null)
            where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(
                    new StreamReader(stream, encoding ?? Encoding.UTF8).ReadToEnd(),
                    _settings
                );
            }
            catch (NotNullObserverException e)
            {
                throw new NotNullObserverException(typeof(T), e);
            }
        }

        /// <summary>
        /// Deserializes the serialized object to a generic type
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="serialized">The serialized.</param>
        /// <returns>
        /// A T.
        /// </returns>
        /// <exception cref="NotNullObserverException"></exception>
        public override T Deserialize<T>(object serialized)
            where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>((string)serialized, _settings);
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
            var jsonSerializer = new JsonSerializer();
            stream = new MemoryStream();
            var streamTemp = new MemoryStream();
            var streamWriter = new StreamWriter(streamTemp, Encoding.UTF8);

            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                jsonWriter.Formatting = Formatting.Indented;

                jsonSerializer.Serialize(jsonWriter, deserialized);

                streamWriter.Flush();
                streamTemp.Seek(0, SeekOrigin.Begin);
                streamTemp.CopyTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Serialize the deserialized Object and Saves the given file.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="file">The file.</param>
        /// <param name="deserialized">The deserialized.</param>
        /// <exception cref="ArgumentNullException">file - Supply a valid filename</exception>
        public override void Save<T>(string file, T deserialized)
            where T : class
        {
            Stream stream = null;
            try
            {
                if (string.IsNullOrWhiteSpace(file))
                {
                    throw new ArgumentNullException(nameof(file), "Supply a valid filename");
                }

                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                using (
                    var fileStream = new FileStream(
                        file,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None
                    )
                )
                {
                    Serialize(deserialized, out stream);

                    stream.CopyTo(fileStream);
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }
    }
}
