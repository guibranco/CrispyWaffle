namespace CrispyWaffle.Serialization.Adapters
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// A serializer xml.
    /// </summary>
    /// <seealso cref="ISerializerAdapter" />
    public sealed class XmlSerializerAdapter : ISerializerAdapter
    {
        #region Implemetation of ISerializerAdapter

        /// <summary>
        /// Deserialize a stream to a generic type
        /// </summary>
        /// <typeparam name="T">Generic type parameter</typeparam>
        /// <param name="stream">The serialized object as stream.</param>
        /// <param name="encoding">(Optional)  The encoding to read the stream. If null Encoding.UTF8 will be used.</param>
        /// <returns>A T.</returns>
        [Pure]
        public T DeserializeFromStream<T>(Stream stream, Encoding encoding = null) where T : class
        {
            return new XmlSerializer(typeof(T)).Deserialize(new StreamReader(stream, encoding ?? Encoding.UTF8)) as T;
        }

        /// <summary>
        /// Deserializes.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="serialized">The serialized.</param>
        /// <returns>A T.</returns>
        [Pure]
        public T Deserialize<T>(object serialized) where T : class
        {
            var document = serialized as XmlDocument;
            return new XmlSerializer(typeof(T)).Deserialize(new StringReader(document?.OuterXml ?? (string)serialized)) as T;
        }

        /// <summary>
        /// Loads the given file and Deserialize its.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="file">The file.</param>
        /// <returns>A T.</returns>
        /// <exception cref="ArgumentNullException">file - Supply a valid filename</exception>
        /// <exception cref="LocalFileNotFoundException">Thrown when an Arquivo Nao Encontrado error condition occurs.</exception>
        [Pure]
        public T Load<T>(string file) where T : class
        {
            var fileName = Path.GetFileName(file);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(nameof(file), "Supply a valid filename");
            }

            if (!File.Exists(file))
            {
                throw new LocalFileNotFoundException(file, Path.GetDirectoryName(Path.GetFullPath(file)));
            }

            using (var sr = new StreamReader(file, Encoding.UTF8))
            {
                var serialized = sr.ReadToEnd();

                return Deserialize<T>(serialized);
            }
        }

        /// <summary>
        /// Serializes.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="deserialized">The deserialized.</param>
        /// <param name="stream">[out] The stream.</param>
        [Pure]
        public void Serialize<T>(T deserialized, out Stream stream) where T : class
        {
            stream = new MemoryStream();

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            var xmlConfig = new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8, OmitXmlDeclaration = false };
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
        public void Save<T>(string file, T deserialized) where T : class
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

                using (var fileStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None))
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

        #endregion
    }
}
