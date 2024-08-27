using CrispyWaffle.Attributes;

namespace CrispyWaffle.Composition
{
    /// <summary>
    /// The lifetime of an instance in the service locator.
    /// </summary>
    public enum Lifetime
    {
        /// <summary>
        /// The transient
        /// </summary>
        [HumanReadable("Transient")]
        Transient,

        /// <summary>
        /// The singleton
        /// </summary>
        [HumanReadable("Singleton")]
        Singleton,
    }
}
