using CrispyWaffle.Infrastructure;
using StackExchange.Redis;

namespace CrispyWaffle.Redis.Log.PropagationStrategy
{
    /// <summary>
    /// The operation propagation strategy class.
    /// </summary>
    /// <seealso cref="IPropagationStrategy" />
    public sealed class OperationPropagationStrategy : IPropagationStrategy
    {
        #region Implementation of IPropagationStrategy

        /// <summary>
        /// Propagates the specified message using specific strategy to the publisher.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="queuePrefix">The queue prefix.</param>
        /// <param name="publisher">The publisher.</param>
        public void Propagate(string message, string queuePrefix, ISubscriber publisher)
        {
            publisher.Publish($"{queuePrefix}-queues", EnvironmentHelper.Operation);
            publisher.Publish($"{queuePrefix}-{EnvironmentHelper.Operation}", message);
        }

        /// <summary>
        /// propagate as an asynchronous operation.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="queuePrefix">The queue prefix.</param>
        /// <param name="publisher">The publisher.</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
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
                .PublishAsync($"{queuePrefix}-queues", EnvironmentHelper.Operation)
                .ConfigureAwait(false);
            await publisher
                .PublishAsync($"{queuePrefix}-{EnvironmentHelper.Operation}", message)
                .ConfigureAwait(false);
        }

        #endregion
    }
}
