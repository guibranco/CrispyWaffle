using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrispyWaffle.Log;
using CrispyWaffle.RabbitMQ.Utils.Communications;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CrispyWaffle.RabbitMQ.Helpers
{
    /// <summary>
    /// A class that facilitates receiving messages from RabbitMQ queues or exchanges.
    /// </summary>
    /// <remarks>
    /// The <see cref="MessageReceiver"/> class provides methods to receive messages from RabbitMQ queues or exchanges,
    /// handling the message reception process and invoking events when messages are received.
    /// </remarks>
    public class MessageReceiver
    {
        /// <summary>
        /// The RabbitMQ connection connector.
        /// </summary>
        private readonly RabbitMQConnector _connector;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageReceiver"/> class.
        /// </summary>
        /// <param name="connector">The <see cref="RabbitMQConnector"/> to be used for connecting to RabbitMQ.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="connector"/> is null.</exception>
        public MessageReceiver(RabbitMQConnector connector) =>
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));

        /// <summary>
        /// Delegate for handling received messages.
        /// </summary>
        /// <param name="sender">The sender of the message.</param>
        /// <param name="e">The <see cref="MessageReceivedArgs"/> containing details of the received message.</param>
        public delegate void MessageReceivedHandler(object sender, MessageReceivedArgs e);

        /// <summary>
        /// Event triggered when a message is received from the queue or exchange.
        /// </summary>
        public event MessageReceivedHandler MessageReceived;

        /// <summary>
        /// Starts receiving messages from a RabbitMQ queue.
        /// </summary>
        /// <typeparam name="T">The type of message to receive, which should implement <see cref="IQueuing"/>.</typeparam>
        /// <param name="autoAck">If set to <c>true</c>, messages will be acknowledged automatically.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to allow cancellation of the operation.</param>
        /// <remarks>
        /// This method will begin a background task to receive messages from the specified queue.
        /// </remarks>
        public void ReceiveFromQueue<T>(bool autoAck, CancellationToken cancellationToken)
            where T : class, IQueuing, new()
        {
            var queueName = Extensions.GetQueueName<T>();

            Task.Run(
                    () => DoWork(string.Empty, queueName, autoAck, cancellationToken),
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Starts receiving messages from a RabbitMQ exchange.
        /// </summary>
        /// <typeparam name="T">The type of message to receive, which should implement <see cref="IQueuing"/>.</typeparam>
        /// <param name="autoAck">If set to <c>true</c>, messages will be acknowledged automatically.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to allow cancellation of the operation.</param>
        /// <remarks>
        /// This method will begin a background task to receive messages from the specified exchange.
        /// </remarks>
        public void ReceiveFromExchange<T>(bool autoAck, CancellationToken cancellationToken)
            where T : class, IQueuing, new()
        {
            var exchangeName = Extensions.GetExchangeName<T>();

            Task.Run(
                    () => DoWork(exchangeName, string.Empty, autoAck, cancellationToken),
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the work of receiving messages from a specified exchange or queue.
        /// </summary>
        /// <param name="exchange">The name of the exchange (optional).</param>
        /// <param name="queue">The name of the queue (optional).</param>
        /// <param name="autoAck">If set to <c>true</c>, messages will be acknowledged automatically.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to allow cancellation of the operation.</param>
        /// <remarks>
        /// This method connects to RabbitMQ, binds the queue to the exchange (if specified),
        /// and starts listening for messages. It invokes the <see cref="MessageReceived"/> event when a message is received.
        /// </remarks>
        private void DoWork(
            string exchange,
            string queue,
            bool autoAck,
            CancellationToken cancellationToken
        )
        {
            using (var connection = _connector.ConnectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var consumer = new EventingBasicConsumer(channel);

                var queueName = string.IsNullOrWhiteSpace(queue)
                    ? channel.QueueDeclare().QueueName
                    : queue;

                if (!string.IsNullOrWhiteSpace(exchange))
                {
                    channel.QueueBind(queueName, exchange, string.Empty);
                }

                consumer.Received += (_, args) =>
                {
                    if (MessageReceived == null)
                    {
                        return;
                    }

                    LogConsumer.Trace(
                        $"Message received from exchange: {exchange} | queue: {queue} | queue name: {queueName}"
                    );

                    var body = Encoding.UTF8.GetString(args.Body.ToArray());

                    var eventArgs = new MessageReceivedArgs { QueueName = queueName, Body = body };

                    MessageReceived.Invoke(this, eventArgs);
                };

                channel.BasicConsume(queue: queueName, autoAck: autoAck, consumer: consumer);

                cancellationToken.WaitHandle.WaitOne();
            }
        }
    }
}
