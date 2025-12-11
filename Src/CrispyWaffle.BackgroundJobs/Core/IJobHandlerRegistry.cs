namespace CrispyWaffle.BackgroundJobs.Core
{
    public interface IJobHandlerRegistry
    {
        void Register<THandler, TData>(string handlerName) where THandler : class, CrispyWaffle.BackgroundJobs.Abstractions.IBackgroundJobHandler<TData>;

        bool TryGet(string handlerName, out Type? handlerType, out Type? dataType);
    }
}