namespace CrispyWaffle
{
    using Attributes;

    /// <summary>
    /// Values that represent execution
    /// </summary>

    public enum Execution
    {
        /// <summary>
        /// The none.
        /// </summary>
        [HumanReadable("None")]
        NONE,

        /// <summary>
        /// The schedule task
        /// </summary>
        [HumanReadable("Schedule task")]
        SCHEDULE_TASK,


        /// <summary>
        /// The arguments
        /// </summary>
        [HumanReadable("Command line (arguments)")]
        ARGUMENTS,

        /// <summary>
        /// The parameters.
        /// </summary>
        [HumanReadable("Command line (parameters)")]
        PARAMETERS,
    }
}
