using CrispyWaffle.Attributes;

namespace CrispyWaffle.Serialization
{
    /// <summary>
    ///     Values that represent SerializerFormat.
    /// </summary>
    public enum SerializerFormat
    {
        /// <summary>
        ///     An enum constant representing the none option.
        /// </summary>
        [HumanReadable("None")]
        None = 0,

        /// <summary>
        ///     An enum constant representing the JSON option.
        /// </summary>
        [HumanReadable("JSON")]
        Json,

        /// <summary>
        ///     An enum constant representing the XML option.
        /// </summary>
        [HumanReadable("XML")]
        Xml
    }
}
