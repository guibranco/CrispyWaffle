using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrispyWaffle.Serialization.Adapters
{
    /// <summary>
    /// A serializer xml.
    /// </summary>
    /// <seealso cref="ISerializerAdapter" />
    public sealed class XmlSerializerAdapter : BaseSerializerAdapter
    {
        /// <summary>
        /// Deserialize a stream to a generic type
        /// </summary>
        /// <typeparam name="T">Generic type parameter</typeparam>
        /// <param name="stream">The serialized object as stream.</param>
        /// <param name="encoding">(Optional)  The encoding to read the stream. If null Encoding.UTF8 will be used.</param>
        /// <returns>A T.</returns>
        [Pure]
        public override T DeserializeFromStream<T>(Stream stream, Encoding encoding = null)
            where T : class
        {
            return new XmlSerializer(typeof(T)).Deserialize(
                    new StreamReader(stream, encoding ?? Encoding.UTF8)
                ) as T;
        }

        /// <summary>
        /// Deserializes.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="serialized">The serialized.</param>
        /// <returns>A T.</returns>
        [Pure]
        public override T Deserialize<T>(object serialized)
            where T : class
        {
            var document = serialized as XmlDocument;
            return new XmlSerializer(typeof(T)).Deserialize(
                    new StringReader(document?.OuterXml ?? (string)serialized)
                ) as T;
        }

        /// <summary>
        /// Serializes.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="deserialized">The deserialized.</param>
        /// <param name="stream">[out] The stream.</param>
        public override void Serialize<T>(T deserialized, out Stream stream)
            where T : class
        {
            stream = new MemoryStream();

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            var xmlConfig = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            };
            var xmlStream = XmlWriter.Create(stream, xmlConfig);

            var serializer = new XmlSerializer(deserialized.GetType());

            serializer.Serialize(xmlStream, deserialized, ns);

            xmlStream.Close();

            stream.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Serialize the deserialized Object and Saves the given file.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="file">The file.</param>
        /// <param name="deserialized">The deserialized.</param>
        /// <exception cref="LocalFileNotFoundException">null - null</exception>
        public override void Save<T>(string file, T deserialized)
            where T : class
        {
            Stream stream = null;
            try
            {
                if (string.IsNullOrWhiteSpace(file))
                {
                    throw new LocalFileNotFoundException(null, null);
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
