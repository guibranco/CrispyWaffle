// ***********************************************************************
// Assembly         : CrispyWaffle.RabbitMQ
// Author           : Guilherme Branco Stracini
// Created          : 09-07-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 03-31-2021
// ***********************************************************************
// <copyright file="Extensions.cs" company="Guilherme Branco Stracini ME">
//     © 2020 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.RabbitMQ.Helpers
{
    /// <summary>
    /// Class Extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the name of the exchange.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>System.String.</returns>
        public static string GetExchangeName<T>()
        where T : class, IQueuing, new()
        {
            var type = typeof(T);
            return type.GetCustomAttributes(typeof(ExchangeNameAttribute), true)
                is ExchangeNameAttribute[] attributes && attributes.Length == 1
                ? attributes[0].ExchangeName
                : type.Name.ToLower().Replace(@" ", @"-");
        }

        /// <summary>
        /// Gets the name of the queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>System.String.</returns>
        public static string GetQueueName<T>()
            where T : class, IQueuing, new()
        {
            var type = typeof(T);

            return type.GetCustomAttributes(typeof(QueueNameAttribute), true)
                is QueueNameAttribute[] attributes && attributes.Length == 1
                ? attributes[0].QueueName
                : type.Name.ToLower().Replace(@" ", @"-");
        }
    }
}
