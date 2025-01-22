using System.Threading;
using System.Threading.Tasks;
using CrispyWaffle.Infrastructure;
using StackExchange.Redis;

namespace CrispyWaffle.Redis.Log.PropagationStrategy;

/// <summary>
/// Represents a process-based propagation strategy for publishing messages to Redis.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IPropagationStrategy"/> interface and defines how messages
/// are propagated to a Redis publisher. It uses the process ID as part of the queue naming convention
/// to uniquely identify messages for the current process.
/// </remarks>
/// <seealso cref="IPropagationStrategy"/>
public sealed class ProcessPropagation : IPropagationStrategy
{
    /// <summary>
    /// Propagates the specified message to the Redis publisher using the process-based strategy.
    /// </summary>
    /// <param name="message">The message to propagate. This will be published to the Redis channel.</param>
    /// <param name="queuePrefix">The prefix to be used in the Redis queue names. It is combined with the process ID to form the final queue name.</param>
    /// <param name="publisher">The <see cref="ISubscriber"/> responsible for publishing the message to Redis.</param>
    /// <remarks>
    /// This method will first publish the process ID to the Redis channel formatted as
    /// "<paramref name="queuePrefix"/>-queues". Then, it will publish the message to the
    /// channel "<paramref name="queuePrefix"/>-<see cref="EnvironmentHelper.ProcessId"/>", where
    /// <see cref="EnvironmentHelper.ProcessId"/> is the unique process ID for the current process.
    /// </remarks>
    public void Propagate(string message, string queuePrefix, ISubscriber publisher)
    {
        publisher.Publish($"{queuePrefix}-queues", EnvironmentHelper.ProcessId);
        publisher.Publish($"{queuePrefix}-{EnvironmentHelper.ProcessId}", message);
    }

    /// <summary>
    /// Asynchronously propagates the specified message to the Redis publisher using the process-based strategy.
    /// </summary>
    /// <param name="message">The message to propagate. This will be published to the Redis channel asynchronously.</param>
    /// <param name="queuePrefix">The prefix to be used in the Redis queue names. It is combined with the process ID to form the final queue name.</param>
    /// <param name="publisher">The <see cref="ISubscriber"/> responsible for publishing the message to Redis asynchronously.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method will first asynchronously publish the process ID to the Redis channel formatted as
    /// "<paramref name="queuePrefix"/>-queues". Then, it will asynchronously publish the message to the
    /// channel "<paramref name="queuePrefix"/>-<see cref="EnvironmentHelper.ProcessId"/>, where
    /// <see cref="EnvironmentHelper.ProcessId"/> is the unique process ID for the current process.
    /// </remarks>
    public async Task PropagateAsync(
        string message,
        string queuePrefix,
        ISubscriber publisher,
        CancellationToken cancellationToken
    )
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        await publisher
            .PublishAsync($"{queuePrefix}-queues", EnvironmentHelper.ProcessId)
            .ConfigureAwait(false);

        await publisher
            .PublishAsync($"{queuePrefix}-{EnvironmentHelper.ProcessId}", message)
            .ConfigureAwait(false);
    }
}
