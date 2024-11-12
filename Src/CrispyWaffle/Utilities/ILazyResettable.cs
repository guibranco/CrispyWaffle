using System;

namespace CrispyWaffle.Utilities
{
    /// <summary>
    /// Represents an interface for classes that support lazy loading and resetting functionality.
    /// </summary>
    /// <remarks>
    /// Implementing classes should provide logic for loading resources lazily and resetting their state when needed.
    /// </remarks>
    public interface ILazyResettable
    {
        /// <summary>
        /// Resets the instance to its initial state.
        /// </summary>
        /// <remarks>
        /// This method should clear any loaded data or state, returning the instance to its default condition.
        /// </remarks>
        void Reset();

        /// <summary>
        /// Loads the instance, typically initializing or fetching resources lazily.
        /// </summary>
        /// <remarks>
        /// This method should ensure that any resources or state are initialized when accessed for the first time.
        /// It might involve delayed loading or computation to optimize performance.
        /// </remarks>
        void Load();

        /// <summary>
        /// Gets the <see cref="Type"/> of the declaring class or object that implements this interface.
        /// </summary>
        /// <value>
        /// The <see cref="Type"/> of the declaring class or object, representing the type of the instance.
        /// </value>
        Type DeclaringType { get; }

        /// <summary>
        /// Retrieves the statistics related to the lazy loading process for the instance.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="ResetLazyStats"/> containing information about the lazy loading and reset operations.
        /// </returns>
        /// <remarks>
        /// This method can provide insights into how many times the instance has been loaded or reset.
        /// </remarks>
        ResetLazyStats Stats();
    }
}
