namespace CrispyWaffle.Serialization.Adapters;

/// <summary>
/// Defines a serializer adapter that can serialize objects directly to strings.
/// </summary>
/// <seealso cref="ISerializerAdapter"/>
public interface IStringSerializerAdapter : ISerializerAdapter
{
    /// <summary>
    /// Serializes an object of type <typeparamref name="T"/> to its string representation.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="deserialized">The object to serialize.</param>
    /// <returns>The serialized string representation.</returns>
    string SerializeToString<T>(T deserialized)
        where T : class;
}
