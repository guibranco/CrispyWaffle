using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CrispyWaffle.Redis.Log.PropagationStrategy;

/// <summary>
/// Defines a strategy for propagating messages to a Redis publisher.
/// </summary>
/// <remarks>
/// Implementations of this interface provide specific strategies for message propagation
/// to Redis channels. This interface includes methods for both synchronous and asynchronous
/// propagation, allowing for flexible communication with Redis publishers.
/// </remarks>
public interface IPropagationStrategy
{
    /// <summary>
    /// Propagates the specified message to the Redis publisher synchronously.
    /// </summary>
    /// <param name="message">The message to propagate. This will be published to the Redis channel.</param>
    /// <param name="queuePrefix">The prefix to be used in the Redis queue names. It is combined with the process ID or other identifiers as necessary.</param>
    /// <param name="publisher">The <see cref="ISubscriber"/> instance used to publish the message to Redis.</param>
    /// <remarks>
    /// This method allows for immediate synchronous message propagation to Redis channels.
    /// </remarks>
    void Propagate(string message, string queuePrefix, ISubscriber publisher);

    /// <summary>
    /// Asynchronously propagates the specified message to the Redis publisher.
    /// </summary>
    /// <param name="message">The message to propagate asynchronously. This will be published to the Redis channel.</param>
    /// <param name="queuePrefix">The prefix to be used in the Redis queue names. It is combined with the process ID or other identifiers as necessary.</param>
    /// <param name="publisher">The <see cref="ISubscriber"/> instance used to asynchronously publish the message to Redis.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the propagation operation if needed.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method allows for asynchronous message propagation, enabling non-blocking Redis communication.
    /// The operation can be cancelled via the provided <paramref name="cancellationToken"/>.
    /// </remarks>
    Task PropagateAsync(
        string message,
        string queuePrefix,
        ISubscriber publisher,
        CancellationToken cancellationToken
    );
}
