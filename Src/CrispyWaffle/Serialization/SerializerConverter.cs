using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml;
using CrispyWaffle.Composition;
using CrispyWaffle.Extensions;
using CrispyWaffle.Log;
using CrispyWaffle.Serialization.Adapters;

namespace CrispyWaffle.Serialization
{
    /// <summary>
    /// A serializer extension.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    public sealed class SerializerConverter<T>
        where T : class
    {
        /// <summary>
        /// The object.
        /// </summary>
        private T _obj;

        /// <summary>
        /// The formatter.
        /// </summary>
        private readonly ISerializerAdapter _formatter;

        /// <summary>
        /// XmlDocument casting operator.
        /// </summary>
        /// <param name="instance">The classe.</param>
        /// <returns>The result of the conversion.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the requested operation is invalid.</exception>
        [Pure]
        public static implicit operator XmlDocument(SerializerConverter<T> instance)
        {
            if (instance?._formatter is not XmlSerializerAdapter)
                return null;

            Stream stream = null;
            var xml = new XmlDocument();
            try
            {
#pragma warning disable S2259
                instance._formatter.Serialize(instance._obj, out stream);
#pragma warning restore S2259
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
        public static implicit operator JsonElement(SerializerConverter<T> instance)
        {
            if (instance._formatter is not JsonSerializerAdapter)
            {
                return default;
            }

            try
            {
                instance._formatter.Serialize(instance._obj, out var stream);
                stream.Seek(0, SeekOrigin.Begin);
                using (var doc = JsonDocument.Parse(stream))
                {
                    var type = instance._obj.GetType();

                    if (typeof(IEnumerable).IsAssignableFrom(type))
                        return doc.RootElement.Clone();
                    else
                    {
                        return doc.RootElement.Clone();
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                LogConsumer.Handle(e);
            }

            return default;
        }

        /// <summary>
        /// Byte[] casting operator.
        /// </summary>
        /// <param name="instance">The class.</param>
        /// <returns>The result of the conversion.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the requested operation is invalid.</exception>
        [Pure]
        public static implicit operator byte[](SerializerConverter<T> instance)
        {
            if (instance._formatter is not BinarySerializerAdapter)
                return null;

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
                var settings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Indent = true,
                    NewLineOnAttributes = true
                };

                using (var xmlWriter = XmlWriter.Create(builder, settings))
                {
                    xml.WriteContentTo(xmlWriter);
                }

                return builder.ToString();
            }

            if (instance._formatter is JsonSerializerAdapter)
            {
                JsonElement json = instance;
                return json.ToString();
            }

            if (instance._formatter is not BinarySerializerAdapter)
            {
                throw new InvalidOperationException(
                    $"The type {typeof(T).FullName} doesn't allow string explicit conversion"
                );
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
            var serializer = new SerializerConverter<T>(
                default,
                ServiceLocator.Resolve<XmlSerializerAdapter>()
            );
            serializer.Deserialize(xml.InnerXml);
            return serializer;
        }

        /// <summary>
        /// SerializerExtension casting operator.
        /// </summary>
        /// <param name="json">The JSON.</param>
        /// <returns>The result of the conversion.</returns>
        [Pure]
        public static implicit operator SerializerConverter<T>(JsonElement json)
        {
            var serializer = new SerializerConverter<T>(
                default,
                ServiceLocator.Resolve<JsonSerializerAdapter>()
            );
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
    }
}
