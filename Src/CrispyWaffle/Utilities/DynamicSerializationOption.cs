namespace CrispyWaffle.Utilities;

/// <summary>
/// Specifies the serialization formatting options for dynamic serialization.
/// </summary>
/// <remarks>
/// This enum defines various options for controlling the format of serialized keys during dynamic serialization.
/// The options include no modification, lowercase, uppercase, or camel case formatting.
/// </remarks>
public enum DynamicSerializationOption
{
    /// <summary>
    /// No formatting is applied to the serialized keys. The original case is preserved.
    /// </summary>
    None,

    /// <summary>
    /// Converts the serialized keys to lowercase.
    /// </summary>
    Lowercase,

    /// <summary>
    /// Converts the serialized keys to uppercase.
    /// </summary>
    Uppercase,

    /// <summary>
    /// Converts the serialized keys to camel case (e.g., "myVariableName").
    /// </summary>
    Camelcase,
}
