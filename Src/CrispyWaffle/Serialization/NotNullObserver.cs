namespace CrispyWaffle.Serialization
{

    /// <summary>
    /// The not null observer class.
    /// This class is used when a type of a property is always null, so using this we can track wherever the value of the property is not null and architect the strongly type for that property
    /// </summary>
    /// <remarks>
    /// Only works for CrispyWaffle.Serialization.Adapter.JsonSerializerAdapter!
    /// </remarks>
    public class NotNullObserver
    { }
}
