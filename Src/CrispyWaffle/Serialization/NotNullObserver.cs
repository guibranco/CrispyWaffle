namespace CrispyWaffle.Serialization;

/// <summary>
/// Observer class that tracks when a property value becomes non-null.
/// This class is used for properties that are typically null, allowing for the detection and handling of when they are assigned non-null values.
/// It can be used to enforce or manage strongly-typed property logic based on nullability.
/// </summary>
/// <remarks>
/// This class is specifically designed to work with <see cref="Adapters.JsonSerializerAdapter"/>.
/// It will not function correctly outside the context of this adapter.
/// </remarks>
public class NotNullObserver { }
