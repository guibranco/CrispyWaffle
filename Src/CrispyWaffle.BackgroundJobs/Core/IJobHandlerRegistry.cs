namespace CrispyWaffle.BackgroundJobs.Core
{
    /// <summary>
    /// Maps handler names to their handler and payload types.
    /// </summary>
    public interface IJobHandlerRegistry
    {
        /// <summary>
        /// Adds or replaces the handler registered under a name.
        /// </summary>
        /// <typeparam name="THandler">The type of the handler.</typeparam>
        /// <typeparam name="TData">The payload type of the handler.</typeparam>
        /// <param name="handlerName">The name to register the handler under.</param>
        void Register<THandler, TData>(string handlerName)
            where THandler : class,
                CrispyWaffle.BackgroundJobs.Abstractions.IBackgroundJobHandler<TData>;

        /// <summary>
        /// Gets the handler and payload types registered under a name.
        /// </summary>
        /// <param name="handlerName">The name the handler was registered under.</param>
        /// <param name="handlerType">When this method returns, the type of the handler, or <c>null</c> when the name is not registered.</param>
        /// <param name="dataType">When this method returns, the payload type of the handler, or <c>null</c> when the name is not registered.</param>
        /// <returns><c>true</c> when a handler is registered under <paramref name="handlerName"/>; otherwise, <c>false</c>.</returns>
        bool TryGet(string handlerName, out Type? handlerType, out Type? dataType);
    }
}
