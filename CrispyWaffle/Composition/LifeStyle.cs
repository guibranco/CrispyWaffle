namespace CrispyWaffle.Composition
{
    using Attributes;

    /// <summary>
    /// The life style of an instance in the service locator
    /// </summary>
    public enum LifeStyle
    {
        /// <summary>
        /// The transient
        /// </summary>
        [HumanReadable("Transient")]
        TRANSIENT,
        /// <summary>
        /// The singleton
        /// </summary>
        [HumanReadable("Singleton")]
        SINGLETON,

        /// <summary>
        /// The scoped
        /// </summary>
        [HumanReadable("Scoped")]
        SCOPED
    }
}
