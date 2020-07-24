namespace CrispyWaffle.Log
{
    using Attributes;
    using System;

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
        FATAL = 1,

        /// <summary>
        /// The error level
        /// </summary>

        [HumanReadable("Error")]
        ERROR = 1 << 1,

        /// <summary>
        /// The warning level
        /// </summary>

        [HumanReadable("Warning")]
        WARNING = 1 << 2,

        /// <summary>
        /// The information level
        /// </summary>

        [HumanReadable("Information")]
        INFO = 1 << 3,


        /// <summary>
        /// The information detailed level
        /// </summary>
        [HumanReadable("Trace")]
        TRACE = 1 << 4,

        /// <summary>
        /// The debug level
        /// </summary>

        [HumanReadable("Debug")]
        DEBUG = 1 << 5,

        /// <summary>
        /// The production level (Error + Warning + Info)
        /// </summary>

        [HumanReadable("Production")]
        PRODUCTION = FATAL | ERROR | WARNING | INFO,

        /// <summary>
        /// The development level (Production + Info detailed)
        /// </summary>
        [HumanReadable("Development")]
        DEVELOPMENT = PRODUCTION | TRACE,

        /// <summary>
        /// All levels together
        /// </summary>
        [HumanReadable("All")]
        ALL = DEVELOPMENT | DEBUG
    }
}
