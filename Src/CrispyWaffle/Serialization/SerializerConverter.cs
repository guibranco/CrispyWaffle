namespace CrispyWaffle.Serialization
{
    using Adapters;
    using Composition;
    using Extensions;
    using Log;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// A serializer extension.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    public sealed class SerializerConverter<T> where T : class
    {
        #region Private fields

        /// <summary>
        /// The object.
        /// </summary>
        private T _obj;

        /// <summary>
        /// The formatter.
        /// </summary>
        private readonly ISerializerAdapter _formatter;

        #endregion

        #region Operators

        /// <summary>
        /// XmlDocument casting operator.
        /// </summary>
        /// <param name="instance">The classe.</param>
        /// <returns>The result of the conversion.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the requested operation is invalid.</exception>
        [Pure]
        public static implicit operator XmlDocument(SerializerConverter<T> instance)
        {
            if (!(instance?._formatter is XmlSerializerAdapter))
            {
                return null;
            }

            Stream stream = null;
            var xml = new XmlDocument();
            try
            {
                instance._formatter.Serialize(instance._obj, out stream);
                xml.Load(stream);
            }
            catch (InvalidOperationException e)
            {
                LogConsumer.Handle(e);
            }
            finally
            {
                stream?.Dispose();
            }
            return xml;
        }

        /// <summary>
        /// JObject casting operator.
        /// </summary>
        /// <param name="instance">The classe.</param>
        /// <returns>The result of the conversion.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the requested operation is invalid.</exception>
        [Pure]
        public static implicit operator JToken(SerializerConverter<T> instance)
        {
            if (!(instance._formatter is JsonSerializerAdapter))
            {
                return null;
            }

            TextReader textReader = null;
            try
            {
                instance._formatter.Serialize(instance._obj, out var stream);
                textReader = new StreamReader(stream);

                using (JsonReader jsonReader = new JsonTextReader(textReader))
                {

                    textReader = null;
                    var type = instance._obj.GetType();

                    return typeof(IEnumerable).IsAssignableFrom(type)
                        ? JArray.Load(jsonReader)
                        : (JToken)JObject.Load(jsonReader);
                }
            }
            catch (InvalidOperationException e)
            {
                LogConsumer.Handle(e);
            }
            finally
            {
                textReader?.Dispose();
            }
            return null;
        }

        /// <summary>
        /// Byte[] casting operator.
        /// </summary>
        /// <param name="instance">The classe.</param>
        /// <returns>The result of the conversion.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the requested operation is invalid.</exception>
        [Pure]
        public static implicit operator byte[](SerializerConverter<T> instance)
        {
            if (!(instance._formatter is BinarySerializerAdapter))
            {
                return null;
            }

            Stream stream = null;
            MemoryStream memoryStream = null;
            byte[] bytes = null;

            try
            {
                instance._formatter.Serialize(instance._obj, out stream);

                memoryStream = new MemoryStream();

                stream.CopyTo(memoryStream);

                bytes = memoryStream.ToArray();

            }
            catch (InvalidOperationException e)
            {
                LogConsumer.Handle(e);
            }
            finally
            {
                stream?.Dispose();
                memoryStream?.Dispose();
            }

            return bytes;
        }

        /// <summary>
        /// String casting operator.
        /// </summary>
        /// <param name="instance">The classe.</param>
        /// <returns>The result of the conversion.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the requested operation is invalid.</exception>
        [Pure]
        public static explicit operator string(SerializerConverter<T> instance)
        {
            if (instance._formatter is XmlSerializerAdapter)
            {
                XmlDocument xml = instance;
                var builder = new StringBuilder();
                var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true, NewLineOnAttributes = true };

                using (var xmlWriter = XmlWriter.Create(builder, settings))
                {
                    xml.WriteContentTo(xmlWriter);
                }

                return builder.ToString();

            }
            if (instance._formatter is JsonSerializerAdapter)
            {
                JToken json = instance;
                return json.ToString();
            }
            if (!(instance._formatter is BinarySerializerAdapter))
            {
                throw new InvalidOperationException($"he type {typeof(T).FullName} doesn't allow string explicit conversion");
            }

            byte[] bytes = instance;
            var binary = bytes.ToBinaryString();
            return string.Join(@" ", binary);
        }

        /// <summary>
        /// SerializerExtension casting operator.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <returns>The result of the conversion.</returns>
        [Pure]
        public static implicit operator SerializerConverter<T>(XmlDocument xml)
        {
            var serializer = new SerializerConverter<T>(default, ServiceLocator.Resolve<XmlSerializerAdapter>());
            serializer.Deserialize(xml.InnerXml);
            return serializer;
        }

        /// <summary>
        /// SerializerExtension casting operator.
        /// </summary>
        /// <param name="json">The JSON.</param>
        /// <returns>The result of the conversion.</returns>
        [Pure]
        public static implicit operator SerializerConverter<T>(JObject json)
        {
            var serializer = new SerializerConverter<T>(default, ServiceLocator.Resolve<JsonSerializerAdapter>());
            serializer.Deserialize(json.ToString());
            return serializer;
        }

        /// <summary>
        /// SerializerExtension casting operator.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>The result of the conversion.</returns>
        [Pure]
        public static implicit operator SerializerConverter<T>(byte[] bytes)
        {
            var serializer = new SerializerConverter<T>(default, new BinarySerializerAdapter());
            var stream = new MemoryStream(bytes);
            serializer.Deserialize(stream);
            return serializer;
        }

        /// <summary>
        /// T casting operator.
        /// </summary>
        /// <param name="instance">The classe.</param>
        /// <returns>The result of the conversion.</returns>
        [Pure]
        public static implicit operator T(SerializerConverter<T> instance)
        {
            return instance._obj;
        }

        #endregion

        #region ~Ctor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="formatter">The formatter.</param>
        public SerializerConverter(T obj, ISerializerAdapter formatter)
        {
            _obj = obj;
            _formatter = formatter;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Serializes the given stream.
        /// </summary>
        /// <param name="stream">[out] The stream.</param>
        public void Serialize(out Stream stream)
        {
            _formatter.Serialize(_obj, out stream);
        }

        /// <summary>
        /// Deserialize a stream to a generic type
        /// </summary>
        /// <param name="stream">The serialized object as stream.</param>
        /// <param name="encoding">(Optional)  The encoding to read the stream. If null Encoding.UTF8 will be used.</param>
        /// <returns>A T.</returns>
        [Pure]
        public T DeserializeFromStream(Stream stream, Encoding encoding = null)
        {
            _obj = _formatter.DeserializeFromStream<T>(stream, encoding);
            return _obj;
        }

        /// <summary>
        /// Deserializes the given deserialized.
        /// </summary>
        /// <param name="deserialized">The deserialized.</param>
        /// <returns>A T.</returns>
        public T Deserialize(object deserialized)
        {
            _obj = _formatter.Deserialize<T>(deserialized);
            return _obj;
        }

        /// <summary>
        /// Loads the given file.
        /// </summary>
        /// <param name="file">The file to load.</param>
        /// <returns>A T.</returns>
        [Pure]
        public T Load([Localizable(false)] string file)
        {
            _obj = _formatter.Load<T>(file);
            return _obj;
        }

        /// <summary>
        /// Saves the given file.
        /// </summary>
        /// <param name="file">The file to load.</param>
        public void Save([Localizable(false)] string file)
        {
            _formatter.Save(file, _obj);
        }

        #endregion
    }
}
