namespace CrispyWaffle.Utilities
{
    using System;

    /// <summary>
    /// The interface for the resettable lazy classes.
    /// </summary>
    public interface ILazyResettable
    {
        /// <summary>
        /// Resets this instance.
        /// </summary>
        void Reset();
        /// <summary>
        /// Loads this instance.
        /// </summary>
        void Load();
        /// <summary>
        /// Gets the type of the declaring.
        /// </summary>
        /// <value>
        /// The type of the declaring.
        /// </value>
        Type DeclaringType { get; }
        /// <summary>
        /// Stats this instance
        /// </summary>
        ResetLazyStats Stats();
    }
}
