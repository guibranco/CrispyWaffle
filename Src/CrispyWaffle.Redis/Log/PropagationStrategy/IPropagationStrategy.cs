using StackExchange.Redis;
using System.Threading;
using System.Threading.Tasks;

namespace CrispyWaffle.Redis.Log.PropagationStrategy
{
    /// <summary>
    /// The propagation strategy interface
    /// </summary>
    public interface IPropagationStrategy
    {
        /// <summary>
        /// Propagates the specified message using specific strategy to the publisher.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="queuePrefix">The queue prefix.</param>
        /// <param name="publisher">The publisher.</param>
        void Propagate(string message, string queuePrefix, ISubscriber publisher);

        /// <summary>
        /// Propagates the asynchronous.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="queuePrefix">The queue prefix.</param>
        /// <param name="publisher">The publisher.</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        Task PropagateAsync(string message, string queuePrefix, ISubscriber publisher, CancellationToken cancellationToken);
    }
}
