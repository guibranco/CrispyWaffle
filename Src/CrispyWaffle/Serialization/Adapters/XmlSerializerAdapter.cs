using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrispyWaffle.Serialization.Adapters;

/// <summary>
/// A serializer xml.
/// </summary>
/// <seealso cref="ISerializerAdapter" />
public sealed class XmlSerializerAdapter : BaseSerializerAdapter
{
    /// <summary>
    /// Deserialize a stream to a generic type.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
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
    /// Serializes an object of type <typeparamref name="T"/> into an XML format and outputs it to a stream.
    /// </summary>
    /// <typeparam name="T">The type of the object to be serialized, which must be a class.</typeparam>
    /// <param name="deserialized">The object to be serialized.</param>
    /// <param name="stream">An output parameter that will contain the stream with the serialized XML data.</param>
    /// <remarks>
    /// This method uses the <see cref="XmlSerializer"/> class to convert the provided object into XML format.
    /// It creates a new <see cref="MemoryStream"/> to hold the serialized data and configures the XML writer settings for proper formatting.
    /// The method also initializes an <see cref="XmlSerializerNamespaces"/> instance to handle XML namespaces, ensuring that no namespace is added to the serialized output.
    /// After serialization, the stream's position is reset to the beginning, allowing for immediate reading of the serialized data.
    /// This method does not throw any exceptions, but it assumes that the provided object is serializable.
    /// </remarks>
    public override void Serialize<T>(T deserialized, out Stream stream)
        where T : class
    {
        stream = new MemoryStream();

        var ns = new XmlSerializerNamespaces();
        ns.Add(string.Empty, string.Empty);

        var xmlConfig = new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false,
        };
        var xmlStream = XmlWriter.Create(stream, xmlConfig);

        var serializer = new XmlSerializer(deserialized.GetType());

        serializer.Serialize(xmlStream, deserialized, ns);

        xmlStream.Close();

        stream.Seek(0, SeekOrigin.Begin);
    }
}
