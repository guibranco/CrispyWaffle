// ***********************************************************************
// Assembly         : CrispyWaffle.RabbitMQ
// Author           : Guilherme Branco Stracini
// Created          : 09-28-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 05-05-2021
// ***********************************************************************
// <copyright file="MessageReceiver.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace CrispyWaffle.RabbitMQ.Helpers
{
    using CrispyWaffle.Log;
    using CrispyWaffle.RabbitMQ.Utils.Communications;
    using global::RabbitMQ.Client;
    using global::RabbitMQ.Client.Events;
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Class MessageReceiver.
    /// </summary>
    public class MessageReceiver
    {
        /// <summary>
        /// The connector
        /// </summary>
        private readonly RabbitMQConnector _connector;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageReceiver" /> class.
        /// </summary>
        /// <param name="connector">The connector.</param>
        /// <exception cref="ArgumentNullException">connector</exception>
        public MessageReceiver(RabbitMQConnector connector) =>
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));

        /// <summary>
        /// Delegate MessageReceivedHandler
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public delegate void MessageReceivedHandler(object sender, MessageReceivedArgs e);

        /// <summary>
        /// Occurs when [message received].
        /// </summary>
        public event MessageReceivedHandler MessageReceived;

        /// <summary>
        /// Receives from queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="autoAck">if set to <c>true</c> [automatic ack].</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public void ReceiveFromQueue<T>(bool autoAck, CancellationToken cancellationToken) where T : class, IQueuing, new()
        {
            var queueName = Extensions.GetQueueName<T>();

            Task.Run(() => DoWork(string.Empty, queueName, autoAck, cancellationToken), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Receives from exchange.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="autoAck">if set to <c>true</c> [automatic ack].</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public void ReceiveFromExchange<T>(bool autoAck, CancellationToken cancellationToken) where T : class, IQueuing, new()
        {
            var exchangeName = Extensions.GetExchangeName<T>();

            Task.Run(() => DoWork(exchangeName, string.Empty, autoAck, cancellationToken), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Does the work.
        /// </summary>
        /// <param name="exchange">The exchange.</param>
        /// <param name="queue">The queue.</param>
        /// <param name="autoAck">if set to <c>true</c> [automatic ack].</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        private void DoWork(string exchange, string queue, bool autoAck, CancellationToken cancellationToken)
        {
            using (var connection = _connector.ConnectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var consumer = new EventingBasicConsumer(channel);

                var queueName = string.IsNullOrWhiteSpace(queue) ? channel.QueueDeclare().QueueName : queue;

                if (!string.IsNullOrWhiteSpace(exchange))
                {
                    channel.QueueBind(queueName, exchange, "");
                }

                consumer.Received += (sender, args) =>
                {
                    if (MessageReceived == null)
                    {
                        return;
                    }

                    LogConsumer.Trace($"Message received from exchange: {exchange} | queue: {queue} | queue name: {queueName}");

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
