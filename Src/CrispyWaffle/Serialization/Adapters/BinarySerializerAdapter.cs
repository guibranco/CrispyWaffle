namespace CrispyWaffle.Serialization.Adapters
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Class BinarySerializerAdapter. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="ISerializerAdapter" />
    public sealed class BinarySerializerAdapter : ISerializerAdapter
    {
        #region Implemetation of ISerializerAdapter

        /// <summary>
        /// Deserialize a stream to a generic type
        /// </summary>
        /// <typeparam name="T">Generic type parameter</typeparam>
        /// <param name="stream">The serialized object as stream.</param>
        /// <param name="encoding">(Optional) Determines the encoding to read the stream (not used in BinarySerializerProvider)</param>
        /// <returns>A T.</returns>
        [Pure]
        public T DeserializeFromStream<T>(Stream stream, Encoding encoding = null) where T : class
        {
            var formatter = new BinaryFormatter();
            var result = formatter.Deserialize(stream);
            stream.Close();
            return (T)result;
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
            return DeserializeFromStream<T>((Stream)serialized);
        }

        /// <summary>
        /// Loads the given file and Deserialize its.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="file">The file.</param>
        /// <returns>A T.</returns>
        /// <exception cref="ArgumentNullException">file - Supply a valid filename</exception>
        /// <exception cref="LocalFileNotFoundException"></exception>
        [Pure]
        public T Load<T>(string file) where T : class
        {
            if (string.IsNullOrWhiteSpace(file))
                throw new ArgumentNullException(nameof(file), "Supply a valid filename");
            if (!File.Exists(file))
                throw new LocalFileNotFoundException(file, Path.GetDirectoryName(Path.GetFullPath(file)));
            var fileName = Path.GetFileName(file);
            var folder = Path.GetDirectoryName(file);
            var are = new AutoResetEvent(false);
            Stream stream;
            while (true)
            {
                try
                {
                    stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None);
                    break;
                }
                catch (IOException)
                {
                    FileSystemWatcher watcher = null;
                    try
                    {
                        watcher = new FileSystemWatcher { Filter = fileName, Path = folder, NotifyFilter = NotifyFilters.Attributes | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size, EnableRaisingEvents = true };
                        watcher.Changed += (o, e) => are.Set();
                        are.WaitOne(new TimeSpan(0, 0, 0, 30));
                    }
                    finally
                    {
                        watcher?.Dispose();
                    }
                }
            }
            return Deserialize<T>(stream);
        }

        /// <summary>
        /// Serializes.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="deserialized">The deserialized.</param>
        /// <param name="stream">[out] The stream.</param>
        public void Serialize<T>(T deserialized, out Stream stream) where T : class
        {
            stream = new MemoryStream();

            using var streamTemp = new MemoryStream();

            IFormatter formatter = new BinaryFormatter();

            formatter.Serialize(streamTemp, deserialized);

            streamTemp.Seek(0, SeekOrigin.Begin);
            streamTemp.CopyTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Serialize the deserialized Object and Saves the given file.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="file">The file.</param>
        /// <param name="deserialized">The deserialized.</param>
        /// <exception cref="ArgumentNullException">file - Supply a valid filename</exception>
        public void Save<T>(string file, T deserialized) where T : class
        {
            Stream stream = null;
            try
            {
                if (string.IsNullOrWhiteSpace(file))
                    throw new ArgumentNullException(nameof(file), "Supply a valid filename");

                if (File.Exists(file))
                    File.Delete(file);

                using var fileStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None);

                Serialize(deserialized, out stream);
                stream.CopyTo(fileStream);
            }
            finally
            {
                stream?.Dispose();
            }
        }

        #endregion
    }
}
