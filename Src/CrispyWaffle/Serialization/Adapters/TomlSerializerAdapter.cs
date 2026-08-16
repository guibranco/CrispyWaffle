using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Tomlyn;

namespace CrispyWaffle.Serialization.Adapters;

/// <summary>
/// Serializes and deserializes TOML content using Tomlyn.
/// </summary>
/// <remarks>
/// Property names are written and read using a camel-case naming policy. The adapter supports
/// string, stream, and file workflows through the standard Crispy Waffle serialization abstractions.
/// </remarks>
/// <seealso cref="ISerializerAdapter" />
/// <seealso cref="IStringSerializerAdapter" />
public sealed class TomlSerializerAdapter : BaseSerializerAdapter, IStringSerializerAdapter
{
    private static readonly TomlSerializerOptions SerializerOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    /// Deserializes TOML content from a stream into an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize.</typeparam>
    /// <param name="stream">The readable stream containing TOML content.</param>
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
            reader => TomlSerializer.Deserialize<T>(reader, SerializerOptions)
        );

    /// <summary>
    /// Deserializes a TOML string into an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize.</typeparam>
    /// <param name="serialized">The TOML string to deserialize.</param>
    /// <returns>The deserialized instance of <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serialized"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="serialized"/> is not a string.</exception>
    public override T Deserialize<T>(object serialized)
        where T : class =>
        StringSerializerAdapterHelper.Deserialize(
            serialized,
            "TOML",
            toml => TomlSerializer.Deserialize<T>(toml, SerializerOptions)
        );

    /// <summary>
    /// Serializes an object of type <typeparamref name="T"/> to a UTF-8 TOML stream.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="deserialized">The object to serialize.</param>
    /// <param name="stream">
    /// When this method returns, contains a readable stream positioned at the beginning of the serialized TOML content.
    /// </param>
    /// <remarks>
    /// The caller owns the returned stream and is responsible for disposing it. A <see langword="null"/>
    /// object produces an empty stream.
    /// </remarks>
    public override void Serialize<T>(T deserialized, out Stream stream)
        where T : class =>
        StringSerializerAdapterHelper.SerializeToStream(
            deserialized,
            value => TomlSerializer.Serialize(value, SerializerOptions),
            out stream
        );

    /// <summary>
    /// Serializes an object of type <typeparamref name="T"/> to a TOML string.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="deserialized">The object to serialize.</param>
    /// <returns>
    /// The TOML string representation of the object, or <see cref="string.Empty"/> when
    /// <paramref name="deserialized"/> is <see langword="null"/>.
    /// </returns>
    public string SerializeToString<T>(T deserialized)
        where T : class =>
        StringSerializerAdapterHelper.SerializeToString(
            deserialized,
            value => TomlSerializer.Serialize(value, SerializerOptions)
        );
}
