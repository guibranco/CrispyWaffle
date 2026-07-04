using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrispyWaffle.Log;
using CrispyWaffle.RabbitMQ.Helpers;
using CrispyWaffle.Serialization;
using RabbitMQ.Client;

namespace CrispyWaffle.RabbitMQ.Utils.Communications;

/// <summary>
/// Provides methods for sending messages to RabbitMQ exchanges and queues.
/// The <see cref="RabbitMQWrapper"/> class uses a <see cref="RabbitMQConnector"/> to establish
/// a connection with RabbitMQ and send serialized messages to a specified exchange or queue.
/// </summary>
public class RabbitMQWrapper
{
    /// <summary>
    /// The RabbitMQ connector used to establish connections.
    /// </summary>
    private readonly RabbitMQConnector _connector;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQWrapper"/> class.
    /// </summary>
    /// <param name="connector">The <see cref="RabbitMQConnector"/> instance used to establish RabbitMQ connections.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="connector"/> is <c>null</c>.</exception>
    public RabbitMQWrapper(RabbitMQConnector connector) =>
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));

    /// <summary>
    /// Sends a serialized message to a RabbitMQ exchange.
    /// </summary>
    /// <typeparam name="T">The type of the item being sent. It must implement <see cref="IQueuing"/> and provide a serializer.</typeparam>
    /// <param name="item">The item to send. It will be serialized to JSON before being sent.</param>
    /// <param name="exchangeDeclareType">The type of exchange to declare. If <c>null</c>, the exchange will not be declared.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to allow cancellation of the operation.</param>
    /// <remarks>
    /// This method establishes a connection to RabbitMQ, declares an exchange if necessary,
    /// serializes the provided item to JSON, and sends the message to the exchange.
    /// </remarks>
    public async Task SendToExchangeAsync<T>(
        T item,
        string exchangeDeclareType = null,
        CancellationToken cancellationToken = default
    )
        where T : class, IQueuing, new()
    {
        var exchangeName = Helpers.Extensions.GetExchangeName<T>();

        var connection = await _connector
            .ConnectionFactory.CreateConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var connectionDisposable = connection.ConfigureAwait(false);

        var channel = await connection
            .CreateChannelAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        await using var channelDisposable = channel.ConfigureAwait(false);

        // Declare the exchange if the type is provided
        if (!string.IsNullOrWhiteSpace(exchangeDeclareType))
        {
            await channel
                .ExchangeDeclareAsync(
                    exchangeName,
                    exchangeDeclareType,
                    true,
                    false,
                    cancellationToken: cancellationToken
                )
                .ConfigureAwait(false);
        }

        // Serialize the item and prepare the message
        var json = (string)item.GetSerializer();
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties { Persistent = true };

        // Publish the message to the exchange
        await channel
            .BasicPublishAsync(
                exchangeName,
                string.Empty,
                false,
                properties,
                body,
                cancellationToken
            )
            .ConfigureAwait(false);

        LogConsumer.Trace("Sent to exchange {0} the item {1}", exchangeName, json);
    }

    /// <summary>
    /// Sends a serialized message to a RabbitMQ queue.
    /// </summary>
    /// <typeparam name="T">The type of the item being sent. It must implement <see cref="IQueuing"/> and provide a serializer.</typeparam>
    /// <param name="item">The item to send. It will be serialized to JSON before being sent.</param>
    /// <param name="queueDeclare">If <c>true</c>, the queue will be declared before sending the message. Default is <c>true</c>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to allow cancellation of the operation.</param>
    /// <remarks>
    /// This method establishes a connection to RabbitMQ, declares a queue if necessary,
    /// serializes the provided item to JSON, and sends the message to the queue.
    /// </remarks>
    public async Task SendToQueueAsync<T>(
        T item,
        bool queueDeclare = true,
        CancellationToken cancellationToken = default
    )
        where T : class, IQueuing, new()
    {
        var queueName = Helpers.Extensions.GetQueueName<T>();

        var connection = await _connector
            .ConnectionFactory.CreateConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var connectionDisposable = connection.ConfigureAwait(false);

        var channel = await connection
            .CreateChannelAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        await using var channelDisposable = channel.ConfigureAwait(false);

        // Declare the queue if specified
        if (queueDeclare)
        {
            await channel
                .QueueDeclareAsync(
                    queueName,
                    true,
                    false,
                    false,
                    cancellationToken: cancellationToken
                )
                .ConfigureAwait(false);
        }

        // Serialize the item and prepare the message
        var json = (string)item.GetSerializer();
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties { Persistent = true };

        // Publish the message to the queue
        await channel
            .BasicPublishAsync(string.Empty, queueName, false, properties, body, cancellationToken)
            .ConfigureAwait(false);

        LogConsumer.Trace("Sent to queue {0} the item: {1}", queueName, json);
    }
}
