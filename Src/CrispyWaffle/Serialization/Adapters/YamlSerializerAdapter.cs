using System;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CrispyWaffle.Serialization.Adapters;

/// <summary>
/// A YAML serializer adapter backed by YamlDotNet.
/// </summary>
/// <seealso cref="ISerializerAdapter" />
public sealed class YamlSerializerAdapter : BaseSerializerAdapter
{
    /// <summary>
    /// Deserialize a stream to a generic type.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="stream">The serialized object as stream.</param>
    /// <param name="encoding">
    /// (Optional) The encoding to read the stream. If null Encoding.UTF8 will be used.
    /// </param>
    /// <returns>A T.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
    public override T DeserializeFromStream<T>(Stream stream, Encoding encoding = null)
        where T : class
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        using (
            var reader = new StreamReader(
                stream,
                encoding ?? Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 1024,
                leaveOpen: true
            )
        )
        {
            return CreateDeserializer().Deserialize<T>(reader);
        }
    }

    /// <summary>
    /// Deserializes a YAML string to a generic type.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="serialized">The YAML serialized representation.</param>
    /// <returns>A T.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serialized"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="serialized"/> is not a string.</exception>
    public override T Deserialize<T>(object serialized)
        where T : class
    {
        if (serialized == null)
        {
            throw new ArgumentNullException(nameof(serialized));
        }

        if (serialized is not string yaml)
        {
            throw new ArgumentException("Serialized YAML must be a string.", nameof(serialized));
        }

        return CreateDeserializer().Deserialize<T>(yaml);
    }

    /// <summary>
    /// Serializes an object of type <typeparamref name="T"/> into YAML and outputs it to a stream.
    /// </summary>
    /// <typeparam name="T">The type of the object to be serialized.</typeparam>
    /// <param name="deserialized">The object to serialize.</param>
    /// <param name="stream">An output stream containing the serialized YAML.</param>
    public override void Serialize<T>(T deserialized, out Stream stream)
        where T : class
    {
        stream = new MemoryStream();
        if (deserialized == null)
        {
            return;
        }

        var yaml = CreateSerializer().Serialize(deserialized);
        var bytes = Encoding.UTF8.GetBytes(yaml);
        stream.Write(bytes, 0, bytes.Length);
        stream.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    /// Creates the configured YAML serializer.
    /// </summary>
    /// <returns>The configured YAML serializer.</returns>
    private static ISerializer CreateSerializer() =>
        new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

    /// <summary>
    /// Creates the configured YAML deserializer.
    /// </summary>
    /// <returns>The configured YAML deserializer.</returns>
    private static IDeserializer CreateDeserializer() =>
        new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
}
