namespace CrispyWaffle.Serialization.Adapters
{
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Interface for serializer.
    /// </summary>
    public interface ISerializerAdapter
    {
        /// <summary>
        ///     Deserialize a stream to a generic type
        /// </summary>
        /// <typeparam name="T">
        ///     Generic type parameter
        /// </typeparam>
        /// <param name="stream">
        ///     The serialized object as stream. 
        /// </param>
        /// <param name="encoding">
        ///     (Optional)  The encoding to read the stream. If null Encoding.UTF8 will be used.
        /// </param>
        /// <returns>
        ///     A T.
        /// </returns>
        [Pure]
        T DeserializeFromStream<T>(Stream stream, Encoding encoding = null) where T : class;

        /// <summary>
        ///     Deserializes the serialized object to a generic type
        /// </summary>
        ///
        /// <typeparam name="T">
        ///     Generic type parameter.
        /// </typeparam>
        /// <param name="serialized">
        ///     The serialized.
        /// </param>
        ///
        /// <returns>
        ///     A T.
        /// </returns>
        [Pure]
        T Deserialize<T>(object serialized) where T : class;

        /// <summary>
        ///    Loads the given file and Deserialize its.
        /// </summary>
        ///
        /// <typeparam name="T">
        ///     Generic type parameter.
        /// </typeparam>
        /// <param name="file">
        ///     The file.
        /// </param>
        ///
        /// <returns>
        ///     A T.
        /// </returns>
        [Pure]
        T Load<T>(string file) where T : class;

        /// <summary>
        /// 	Serializes.
        /// </summary>
        ///
        /// <typeparam name="T">
        /// 	Generic type parameter.
        /// </typeparam>
        /// <param name="deserialized">
        /// 	The deserialized.
        /// </param>
        /// <param name="stream">
        /// 	[out] The stream.
        /// </param>
        void Serialize<T>(T deserialized, out Stream stream) where T : class;

        /// <summary>
        ///     Serialize the deserialized Object and Saves the given file.
        /// </summary>
        ///
        /// <typeparam name="T">
        ///     Generic type parameter.
        /// </typeparam>
        /// <param name="file">
        ///     The file.
        /// </param>
        /// <param name="deserialized">
        ///     The deserialized.
        /// </param>
        void Save<T>(string file, T deserialized) where T : class;
    }
}
