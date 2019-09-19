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
        NONE = 0,

        /// <summary>
        /// 	An enum constant representing the binary option.
        /// </summary>

        [HumanReadable("Binary")]
        BINARY = 1,

        /// <summary>
        /// 	An enum constant representing the JSON option.
        /// </summary>

        [HumanReadable("JSON")]
        JSON = 2,

        /// <summary>
        /// 	An enum constant representing the XML option.
        /// </summary>

        [HumanReadable("XML")]
        XML = 3
    }
}
