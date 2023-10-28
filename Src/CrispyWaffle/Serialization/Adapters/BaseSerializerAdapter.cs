using System;
using System.IO;
using System.Text;

namespace CrispyWaffle.Serialization.Adapters;

/// <summary>
/// Class BaseSerializerAdapter.
/// Implements the <see cref="ISerializerAdapter" />.
/// </summary>
/// <seealso cref="ISerializerAdapter" />
public abstract class BaseSerializerAdapter : ISerializerAdapter
{
    /// <summary>
    /// Deserialize a stream to a generic type
    /// </summary>
    /// <typeparam name="T">Generic type parameter</typeparam>
    /// <param name="stream">The serialized object as stream.</param>
    /// <param name="encoding">(Optional)  The encoding to read the stream. If null Encoding.UTF8 will be used.</param>
    /// <returns>A T.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented in the base class. Use the derived one.</exception>
    public virtual T DeserializeFromStream<T>(Stream stream, Encoding encoding = null)
        where T : class => throw new NotImplementedException();

    /// <summary>
    /// Deserializes the serialized object to a generic type
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="serialized">The serialized.</param>
    /// <returns>A T.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented in the base class. Use the derived one.</exception>
    public virtual T Deserialize<T>(object serialized)
        where T : class => throw new NotImplementedException();

    /// <summary>
    /// Loads the given file and Deserialize its.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="file">The file.</param>
    /// <returns>A T.</returns>
    /// <exception cref="ArgumentNullException">Throws when the file parameter is null or whitespace.</exception>
    /// <exception cref="LocalFileNotFoundException">Throws when the file does not exist.</exception>
    public virtual T Load<T>(string file)
        where T : class
    {
        var fileName = Path.GetFileName(file);
        if (string.IsNullOrWhiteSpace(fileName))
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

        using (var sr = new StreamReader(file, Encoding.UTF8))
        {
            var serialized = sr.ReadToEnd();

            return Deserialize<T>(serialized);
        }
    }

    /// <summary>
    /// Serializes.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="deserialized">The deserialized.</param>
    /// <param name="stream">[out] The stream.</param>
    /// <exception cref="NotImplementedException">This method is not implemented in the base class. Use the derived one.</exception>
    public virtual void Serialize<T>(T deserialized, out Stream stream)
        where T : class => throw new NotImplementedException();

    /// <summary>
    /// Serialize the deserialized Object and Saves the given file.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="file">The file.</param>
    /// <param name="deserialized">The deserialized.</param>
    /// <exception cref="System.ArgumentNullException">file - Supply a valid filename</exception>
    public virtual void Save<T>(string file, T deserialized)
        where T : class
    {
        Stream stream = null;
        try
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                throw new ArgumentNullException(nameof(file), "Supply a valid filename");
            }

            if (File.Exists(file))
            {
                File.Delete(file);
            }

            using (
                var fileStream = new FileStream(
                    file,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None
                )
            )
            {
                Serialize(deserialized, out stream);

                stream.CopyTo(fileStream);
            }
        }
        finally
        {
            stream?.Dispose();
        }
    }
}
