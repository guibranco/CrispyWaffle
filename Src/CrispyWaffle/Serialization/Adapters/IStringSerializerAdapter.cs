namespace CrispyWaffle.Serialization.Adapters;

/// <summary>
/// Defines a serializer adapter that can produce a format-specific string representation.
/// </summary>
/// <remarks>
/// Implementations extend <see cref="ISerializerAdapter"/> with direct string serialization,
/// allowing string-capable formats to participate in the serializer conversion API.
/// </remarks>
/// <seealso cref="ISerializerAdapter"/>
public interface IStringSerializerAdapter : ISerializerAdapter
{
    /// <summary>
    /// Serializes an object of type <typeparamref name="T"/> to its format-specific string representation.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="deserialized">The object to serialize.</param>
    /// <returns>The serialized string representation produced by the adapter.</returns>
    string SerializeToString<T>(T deserialized)
        where T : class;
}
