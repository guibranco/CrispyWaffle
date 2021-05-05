namespace CrispyWaffle.Composition
{
    using System;

    /// <summary>
    /// A dependency resolver interface
    /// </summary>
    public interface IDependencyResolver
    {
        /// <summary>
        /// Resolves a dependency for the specified type.
        /// </summary>
        /// <param name="type">The type where the dependency origins.</param>
        /// <param name="order">The order of this argument in the origins argument list.</param>
        /// <returns></returns>
        object Resolve(Type type, int order);
    }
}
