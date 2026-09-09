using System;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CrispyWaffle.Serialization.Adapters;

/// <summary>
/// Serializes and deserializes YAML content using YamlDotNet.
/// </summary>
/// <remarks>
/// Property names are written and read using the camel-case naming convention. The adapter supports
/// string, stream, and file workflows through the standard Crispy Waffle serialization abstractions.
/// </remarks>
/// <seealso cref="ISerializerAdapter" />
/// <seealso cref="IStringSerializerAdapter" />
public sealed class YamlSerializerAdapter : BaseSerializerAdapter, IStringSerializerAdapter
{
    /// <summary>
    /// Deserializes YAML content from a stream into an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize.</typeparam>
    /// <param name="stream">The readable stream containing YAML content.</param>
    /// <param name="encoding">
    /// The text encoding used to read the stream. When <see langword="null"/>,
    /// <see cref="Encoding.UTF8"/> is used.
    /// </param>
    /// <returns>The deserialized instance of <typeparamref name="T"/>.</returns>
    /// <remarks>The supplied stream remains open after deserialization.</remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is <see langword="null"/>.</exception>
    public override T DeserializeFromStream<T>(Stream stream, Encoding encoding = null)
        where T : class =>
        StringSerializerAdapterHelper.DeserializeFromStream(
            stream,
            encoding,
            reader => CreateDeserializer().Deserialize<T>(reader)
        );

    /// <summary>
    /// Deserializes a YAML string into an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize.</typeparam>
    /// <param name="serialized">The YAML string to deserialize.</param>
    /// <returns>The deserialized instance of <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serialized"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="serialized"/> is not a string.</exception>
    public override T Deserialize<T>(object serialized)
        where T : class =>
        StringSerializerAdapterHelper.Deserialize(
            serialized,
            "YAML",
            yaml => CreateDeserializer().Deserialize<T>(yaml)
        );

    /// <summary>
    /// Serializes an object of type <typeparamref name="T"/> to a UTF-8 YAML stream.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="deserialized">The object to serialize.</param>
    /// <param name="stream">
    /// When this method returns, contains a readable stream positioned at the beginning of the serialized YAML content.
    /// </param>
    /// <remarks>
    /// The caller owns the returned stream and is responsible for disposing it. A <see langword="null"/>
    /// object produces an empty stream.
    /// </remarks>
    public override void Serialize<T>(T deserialized, out Stream stream)
        where T : class =>
        StringSerializerAdapterHelper.SerializeToStream(
            deserialized,
            value => CreateSerializer().Serialize(value),
            out stream
        );

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
            .WithDuplicateKeyChecking()
            .Build();

    /// <summary>
    /// Serializes an object of type <typeparamref name="T"/> to a YAML string.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="deserialized">The object to serialize.</param>
    /// <returns>
    /// The YAML string representation of the object, or <see cref="string.Empty"/> when
    /// <paramref name="deserialized"/> is <see langword="null"/>.
    /// </returns>
    public string SerializeToString<T>(T deserialized)
        where T : class =>
        StringSerializerAdapterHelper.SerializeToString(
            deserialized,
            value => CreateSerializer().Serialize(value)
        );
}
