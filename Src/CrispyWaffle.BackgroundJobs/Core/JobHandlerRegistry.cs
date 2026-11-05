using System.Collections.Concurrent;

namespace CrispyWaffle.BackgroundJobs.Core
{
    /// <summary>
    /// The default, thread-safe <see cref="IJobHandlerRegistry"/>.
    /// </summary>
    public class JobHandlerRegistry : IJobHandlerRegistry
    {
        private readonly ConcurrentDictionary<string, (Type handlerType, Type dataType)> _map =
            new();

        /// <inheritdoc />
        /// <exception cref="ArgumentException">Thrown when <paramref name="handlerName"/> is <c>null</c>, empty or white space.</exception>
        public void Register<THandler, TData>(string handlerName)
            where THandler : class,
                CrispyWaffle.BackgroundJobs.Abstractions.IBackgroundJobHandler<TData>
        {
            if (string.IsNullOrWhiteSpace(handlerName))
                throw new ArgumentException("handlerName required", nameof(handlerName));
            _map[handlerName] = (typeof(THandler), typeof(TData));
        }

        /// <inheritdoc />
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
