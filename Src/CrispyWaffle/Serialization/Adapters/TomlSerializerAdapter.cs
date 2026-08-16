using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Tomlyn;

namespace CrispyWaffle.Serialization.Adapters;

/// <summary>
/// A TOML serializer adapter backed by Tomlyn.
/// </summary>
/// <seealso cref="ISerializerAdapter" />
/// <seealso cref="IStringSerializerAdapter" />
public sealed class TomlSerializerAdapter : BaseSerializerAdapter, IStringSerializerAdapter
{
    private static readonly TomlSerializerOptions SerializerOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

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
            return TomlSerializer.Deserialize<T>(reader, SerializerOptions);
        }
    }

    /// <summary>
    /// Deserializes a TOML string to a generic type.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="serialized">The TOML serialized representation.</param>
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

        if (serialized is not string toml)
        {
            throw new ArgumentException("Serialized TOML must be a string.", nameof(serialized));
        }

        return TomlSerializer.Deserialize<T>(toml, SerializerOptions);
    }

    /// <summary>
    /// Serializes an object of type <typeparamref name="T"/> into TOML and outputs it to a stream.
    /// </summary>
    /// <typeparam name="T">The type of the object to be serialized.</typeparam>
    /// <param name="deserialized">The object to serialize.</param>
    /// <param name="stream">An output stream containing the serialized TOML.</param>
    public override void Serialize<T>(T deserialized, out Stream stream)
        where T : class
    {
        stream = new MemoryStream();
        var toml = SerializeToString(deserialized);
        if (toml.Length == 0)
        {
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(toml);
        stream.Write(bytes, 0, bytes.Length);
        stream.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    /// Serializes an object of type <typeparamref name="T"/> into a TOML string.
    /// </summary>
    /// <typeparam name="T">The type of the object to be serialized.</typeparam>
    /// <param name="deserialized">The object to serialize.</param>
    /// <returns>The TOML string representation, or an empty string when the object is null.</returns>
    public string SerializeToString<T>(T deserialized)
        where T : class =>
        deserialized == null ? string.Empty : TomlSerializer.Serialize(deserialized, SerializerOptions);
}
