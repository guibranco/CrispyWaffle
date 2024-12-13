using System;

namespace CrispyWaffle.Composition;

/// <summary>
/// An interface for a dependency resolver that resolves dependencies based on the type and order of resolution.
/// </summary>
public interface IDependencyResolver
{
    /// <summary>
    /// Resolves a dependency for the specified type and order.
    /// </summary>
    /// <param name="type">The type of the dependency to resolve. This represents the type of the object that is being requested.</param>
    /// <param name="order">The order in which the dependency should be resolved, typically used to prioritize dependencies when multiple dependencies exist for the same type.</param>
    /// <returns>An instance of the resolved dependency.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the dependency cannot be resolved.</exception>
    object Resolve(Type type, int order);
}
