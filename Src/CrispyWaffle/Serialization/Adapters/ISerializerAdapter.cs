using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace CrispyWaffle.Serialization.Adapters
{
    /// <summary>
    /// Defines methods for serializing and deserializing objects.
    /// This interface provides functionality to serialize objects to streams and files,
    /// and deserialize them from streams and serialized representations.
    /// </summary>
    public interface ISerializerAdapter
    {
        /// <summary>
        /// Deserializes a stream into a generic object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to deserialize.
        /// </typeparam>
        /// <param name="stream">
        /// The stream containing the serialized object data.
        /// </param>
        /// <param name="encoding">
        /// (Optional) The encoding to read the stream. If null, <see cref="Encoding.UTF8"/> will be used.
        /// </param>
        /// <returns>
        /// The deserialized object of type <typeparamref name="T"/>.
        /// </returns>
        [Pure]
        T DeserializeFromStream<T>(Stream stream, Encoding encoding = null)
            where T : class;

        /// <summary>
        /// Deserializes a serialized object into a generic object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to deserialize.
        /// </typeparam>
        /// <param name="serialized">
        /// The serialized representation of the object.
        /// </param>
        /// <returns>
        /// The deserialized object of type <typeparamref name="T"/>.
        /// </returns>
        [Pure]
        T Deserialize<T>(object serialized)
            where T : class;

        /// <summary>
        /// Loads a file, deserializes its contents, and returns the deserialized object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to deserialize from the file.
        /// </typeparam>
        /// <param name="file">
        /// The file path containing the serialized object.
        /// </param>
        /// <returns>
        /// The deserialized object of type <typeparamref name="T"/>.
        /// </returns>
        [Pure]
        T Load<T>(string file)
            where T : class;

        /// <summary>
        /// Serializes a deserialized object of type <typeparamref name="T"/> into a stream.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to serialize.
        /// </typeparam>
        /// <param name="deserialized">
        /// The deserialized object to serialize.
        /// </param>
        /// <param name="stream">
        /// The output stream that will contain the serialized object data.
        /// </param>
        void Serialize<T>(T deserialized, out Stream stream)
            where T : class;

        /// <summary>
        /// Serializes a deserialized object of type <typeparamref name="T"/> and saves it to the specified file.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to serialize.
        /// </typeparam>
        /// <param name="file">
        /// The file path where the serialized object will be saved.
        /// </param>
        /// <param name="deserialized">
        /// The deserialized object to serialize and save.
        /// </param>
        void Save<T>(string file, T deserialized)
            where T : class;
    }
}
