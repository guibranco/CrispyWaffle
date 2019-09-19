namespace CrispyWaffle.Composition
{
    /// <summary>
    /// Interface for bootstrap application
    /// </summary>
    public interface IBootstrapper
    {
        /// <summary>
        /// Register the services used by a application.
        /// </summary>
        void RegisterServices();
    }
}
