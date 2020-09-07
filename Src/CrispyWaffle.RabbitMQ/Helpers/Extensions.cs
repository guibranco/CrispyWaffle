// ***********************************************************************
// Assembly         : CrispyWaffle.RabbitMQ
// Author           : Guilherme Branco Stracini
// Created          : 09-07-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-07-2020
// ***********************************************************************
// <copyright file="Extensions.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
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
