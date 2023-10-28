// ***********************************************************************
// Assembly         : CrispyWaffle.RabbitMQ
// Author           : Guilherme Branco Stracini
// Created          : 03-31-2021
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 05-05-2021
// ***********************************************************************
// <copyright file="RabbitMqWrapper.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Text;
using CrispyWaffle.Log;
using CrispyWaffle.RabbitMQ.Helpers;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.RabbitMQ.Utils.Communications
{
    /// <summary>
    /// Class RabbitMQWrapper.
    /// </summary>
    public class RabbitMQWrapper
    {
        /// <summary>
        /// The connector
        /// </summary>
        private readonly RabbitMQConnector _connector;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMQWrapper" /> class.
        /// </summary>
        /// <param name="connector">The connector.</param>
        /// <exception cref="ArgumentNullException">connector</exception>
        public RabbitMQWrapper(RabbitMQConnector connector) =>
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));

        /// <summary>
        /// Sends to exchange.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <param name="exchangeDeclareType">Type of the exchange declare.</param>
        public void SendToExchange<T>(T item, string exchangeDeclareType = null)
            where T : class, IQueuing, new()
        {
            var exchangeName = Helpers.Extensions.GetExchangeName<T>();

            using (var connection = _connector.ConnectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                if (!string.IsNullOrWhiteSpace(exchangeDeclareType))
                {
                    channel.ExchangeDeclare(exchangeName, exchangeDeclareType, true, false, null);
                }

                var json = (string)item.GetSerializer();
                var body = Encoding.UTF8.GetBytes(json);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchangeName, string.Empty, false, properties, body);

                LogConsumer.Trace("Sent to exchange {0} the item {1}", exchangeName, json);
            }
        }

        /// <summary>
        /// Sends to queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <param name="queueDeclare">if set to <c>true</c> [queue declare].</param>
        public void SendToQueue<T>(T item, bool queueDeclare = true)
            where T : class, IQueuing, new()
        {
            var queueName = Helpers.Extensions.GetQueueName<T>();

            using (var connection = _connector.ConnectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                if (queueDeclare)
                {
                    channel.QueueDeclare(queueName, true, false, false, null);
                }

                var json = (string)item.GetSerializer();
                var body = Encoding.UTF8.GetBytes(json);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish("", queueName, false, properties, body);

                LogConsumer.Trace("Sent to queue {0} the item: {1}", queueName, json);
            }
        }
    }
}
