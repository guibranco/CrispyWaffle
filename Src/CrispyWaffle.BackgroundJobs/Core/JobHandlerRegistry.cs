using System.Collections.Concurrent;

namespace CrispyWaffle.BackgroundJobs.Core
{
    public class JobHandlerRegistry : IJobHandlerRegistry
    {
        private readonly ConcurrentDictionary<string, (Type handlerType, Type dataType)> _map = new();

        public void Register<THandler, TData>(string handlerName) where THandler : class, CrispyWaffle.BackgroundJobs.Abstractions.IBackgroundJobHandler<TData>
        {
            if (string.IsNullOrWhiteSpace(handlerName)) throw new ArgumentException("handlerName required", nameof(handlerName));
            _map[handlerName] = (typeof(THandler), typeof(TData));
        }

        public bool TryGet(string handlerName, out Type? handlerType, out Type? dataType)
        {
            if (handlerName != null && _map.TryGetValue(handlerName, out var pair))
            {
                handlerType = pair.handlerType;
                dataType = pair.dataType;
                return true;
            }

            handlerType = null;
            dataType = null;
            return false;
        }
    }
}
