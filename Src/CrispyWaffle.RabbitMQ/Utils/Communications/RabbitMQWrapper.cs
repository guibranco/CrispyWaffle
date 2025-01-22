using System;
using System.Text;
using CrispyWaffle.Log;
using CrispyWaffle.RabbitMQ.Helpers;
using CrispyWaffle.Serialization;

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
    /// <remarks>
    /// This method establishes a connection to RabbitMQ, declares an exchange if necessary,
    /// serializes the provided item to JSON, and sends the message to the exchange.
    /// </remarks>
    public void SendToExchange<T>(T item, string exchangeDeclareType = null)
        where T : class, IQueuing, new()
    {
        var exchangeName = Helpers.Extensions.GetExchangeName<T>();

        using (var connection = _connector.ConnectionFactory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // Declare the exchange if the type is provided
            if (!string.IsNullOrWhiteSpace(exchangeDeclareType))
            {
                channel.ExchangeDeclare(exchangeName, exchangeDeclareType, true, false, null);
            }

            // Serialize the item and prepare the message
            var json = (string)item.GetSerializer();
            var body = Encoding.UTF8.GetBytes(json);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            // Publish the message to the exchange
            channel.BasicPublish(exchangeName, string.Empty, false, properties, body);

            LogConsumer.Trace("Sent to exchange {0} the item {1}", exchangeName, json);
        }
    }

    /// <summary>
    /// Sends a serialized message to a RabbitMQ queue.
    /// </summary>
    /// <typeparam name="T">The type of the item being sent. It must implement <see cref="IQueuing"/> and provide a serializer.</typeparam>
    /// <param name="item">The item to send. It will be serialized to JSON before being sent.</param>
    /// <param name="queueDeclare">If <c>true</c>, the queue will be declared before sending the message. Default is <c>true</c>.</param>
    /// <remarks>
    /// This method establishes a connection to RabbitMQ, declares a queue if necessary,
    /// serializes the provided item to JSON, and sends the message to the queue.
    /// </remarks>
    public void SendToQueue<T>(T item, bool queueDeclare = true)
        where T : class, IQueuing, new()
    {
        var queueName = Helpers.Extensions.GetQueueName<T>();

        using (var connection = _connector.ConnectionFactory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // Declare the queue if specified
            if (queueDeclare)
            {
                channel.QueueDeclare(queueName, true, false, false, null);
            }

            // Serialize the item and prepare the message
            var json = (string)item.GetSerializer();
            var body = Encoding.UTF8.GetBytes(json);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            // Publish the message to the queue
            channel.BasicPublish(string.Empty, queueName, false, properties, body);

            LogConsumer.Trace("Sent to queue {0} the item: {1}", queueName, json);
        }
    }
}
