using CrispyWaffle.Attributes;

namespace CrispyWaffle.Log
{
    /// <summary>
    /// Bitfield of flags for specifying log level.
    /// </summary>
    [Flags]
    public enum LogLevel
    {
        /// <summary>
        ///
        /// </summary>
        [HumanReadable("Fatal")]
        Fatal = 1,

        /// <summary>
        /// The error level
        /// </summary>
        [HumanReadable("Error")]
        Error = 1 << 1,

        /// <summary>
        /// The warning level
        /// </summary>
        [HumanReadable("Warning")]
        Warning = 1 << 2,

        /// <summary>
        /// The information level
        /// </summary>
        [HumanReadable("Information")]
        Info = 1 << 3,

        /// <summary>
        /// The information detailed level
        /// </summary>
        [HumanReadable("Trace")]
        Trace = 1 << 4,

        /// <summary>
        /// The debug level
        /// </summary>
        [HumanReadable("Debug")]
        Debug = 1 << 5,

        /// <summary>
        /// The production level (Error + Warning + Info)
        /// </summary>
        [HumanReadable("Production")]
        Production = Fatal | Error | Warning | Info,

        /// <summary>
        /// The development level (Production + Info detailed)
        /// </summary>
        [HumanReadable("Development")]
        Development = Production | Trace,

        /// <summary>
        /// All levels together
        /// </summary>
        [HumanReadable("All")]
        All = Development | Debug
    }
}
