using System;
using System.IO;
using System.Text;

namespace CrispyWaffle.Serialization.Adapters;

/// <summary>
/// Provides shared validation and stream handling for string-capable serializer adapters.
/// </summary>
internal static class StringSerializerAdapterHelper
{
    /// <summary>
    /// Deserializes text from a stream while keeping the caller-owned stream open.
    /// </summary>
    /// <typeparam name="T">The type to deserialize.</typeparam>
    /// <param name="stream">The readable stream containing serialized text.</param>
    /// <param name="encoding">The encoding used to read the stream, or UTF-8 when null.</param>
    /// <param name="deserialize">The format-specific deserialization function.</param>
    /// <returns>The deserialized instance.</returns>
    public static T DeserializeFromStream<T>(
        Stream stream,
        Encoding encoding,
        Func<TextReader, T> deserialize
    )
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
            return deserialize(reader);
        }
    }

    /// <summary>
    /// Validates a serialized object as text and invokes the format-specific deserializer.
    /// </summary>
    /// <typeparam name="T">The type to deserialize.</typeparam>
    /// <param name="serialized">The serialized value.</param>
    /// <param name="formatName">The format name used in validation messages.</param>
    /// <param name="deserialize">The format-specific deserialization function.</param>
    /// <returns>The deserialized instance.</returns>
    public static T Deserialize<T>(
        object serialized,
        string formatName,
        Func<string, T> deserialize
    )
        where T : class
    {
        if (serialized == null)
        {
            throw new ArgumentNullException(nameof(serialized));
        }

        if (serialized is not string serializedText)
        {
            throw new ArgumentException(
                $"Serialized {formatName} must be a string.",
                nameof(serialized)
            );
        }

        return deserialize(serializedText);
    }

    /// <summary>
    /// Serializes an object to a UTF-8 stream positioned at the beginning.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="deserialized">The object to serialize.</param>
    /// <param name="serialize">The format-specific serialization function.</param>
    /// <param name="stream">The resulting readable stream.</param>
    public static void SerializeToStream<T>(
        T deserialized,
        Func<T, string> serialize,
        out Stream stream
    )
        where T : class
    {
        stream = new MemoryStream();
        var serialized = SerializeToString(deserialized, serialize);
        if (serialized.Length == 0)
        {
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(serialized);
        stream.Write(bytes, 0, bytes.Length);
        stream.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    /// Serializes an object to text, returning an empty string for a null object.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="deserialized">The object to serialize.</param>
    /// <param name="serialize">The format-specific serialization function.</param>
    /// <returns>The serialized text or an empty string when the object is null.</returns>
    public static string SerializeToString<T>(T deserialized, Func<T, string> serialize)
        where T : class => deserialized == null ? string.Empty : serialize(deserialized);
}
