namespace CrispyWaffle.Serialization
{
    using Attributes;

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
        /// 	An enum constant representing the binary option.
        /// </summary>

        [HumanReadable("Binary")]
        Binary = 1,

        /// <summary>
        /// 	An enum constant representing the JSON option.
        /// </summary>

        [HumanReadable("JSON")]
        Json = 2,

        /// <summary>
        /// 	An enum constant representing the XML option.
        /// </summary>

        [HumanReadable("XML")]
        Xml = 3
    }
}
