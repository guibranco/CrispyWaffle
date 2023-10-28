using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace CrispyWaffle.Serialization.Adapters
{
    /// <summary>
    /// Class BinarySerializerAdapter. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="ISerializerAdapter" />
    public sealed class BinarySerializerAdapter : BaseSerializerAdapter
    {
        /// <summary>
        /// Deserialize a stream to a generic type.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="stream">The serialized object as stream.</param>
        /// <param name="encoding">(Optional) Determines the encoding to read the stream (not used in BinarySerializerProvider).</param>
        /// <returns>A T.</returns>
        [Pure]
        public override T DeserializeFromStream<T>(Stream stream, Encoding encoding = null)
            where T : class
        {
            var formatter = new BinaryFormatter();
#pragma warning disable S5773
            var result = formatter.Deserialize(stream);
#pragma warning restore S5773
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
        public override T Deserialize<T>(object serialized)
            where T : class
        {
            return DeserializeFromStream<T>((Stream)serialized);
        }

        /// <summary>
        /// Loads the given file and Deserialize its.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="file">The file.</param>
        /// <returns>A T.</returns>
        /// <exception cref="ArgumentNullException">file - Supply a valid filename.</exception>
        /// <exception cref="LocalFileNotFoundException">Throws when the file doesn't exist.</exception>
        [Pure]
        public override T Load<T>(string file)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                throw new ArgumentNullException(nameof(file), "Supply a valid filename");
            }

            if (!File.Exists(file))
            {
                throw new LocalFileNotFoundException(
                    file,
                    Path.GetDirectoryName(Path.GetFullPath(file))
                );
            }

            var fileName = Path.GetFileName(file);
            var folder = Path.GetDirectoryName(file);

            var stream = LoadInternal(file, fileName, folder);

            return Deserialize<T>(stream);
        }

        /// <summary>
        /// Loads the internal.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="folder">The folder.</param>
        /// <returns>Stream.</returns>
        private static Stream LoadInternal(string file, string fileName, string folder)
        {
            var autoResetEvent = new AutoResetEvent(false);

            while (true)
            {
                try
                {
                    return new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None);
                }
                catch (IOException)
                {
                    HandleLoadIoException(fileName, folder, autoResetEvent);
                }
            }
        }

        /// <summary>
        /// Handles the load io exception.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="folder">The folder.</param>
        /// <param name="autoResetEvent">The automatic reset event.</param>
        private static void HandleLoadIoException(
            string fileName,
            string folder,
            AutoResetEvent autoResetEvent
        )
        {
            FileSystemWatcher watcher = null;
            try
            {
                watcher = new FileSystemWatcher
                {
                    Filter = fileName,
                    Path = folder,
                    NotifyFilter =
                        NotifyFilters.Attributes
                        | NotifyFilters.DirectoryName
                        | NotifyFilters.FileName
                        | NotifyFilters.LastWrite
                        | NotifyFilters.Size,
                    EnableRaisingEvents = true
                };
                watcher.Changed += (_, _) => autoResetEvent.Set();
                autoResetEvent.WaitOne(new TimeSpan(0, 0, 0, 30));
            }
            finally
            {
                watcher?.Dispose();
            }
        }

        /// <summary>
        /// Serializes.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="deserialized">The deserialized.</param>
        /// <param name="stream">[out] The stream.</param>
        public override void Serialize<T>(T deserialized, out Stream stream)
            where T : class
        {
            stream = new MemoryStream();

            using (var streamTemp = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();

                formatter.Serialize(streamTemp, deserialized);

                streamTemp.Seek(0, SeekOrigin.Begin);
                streamTemp.CopyTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}
