// ***********************************************************************
// Assembly         : CrispyWaffle.RabbitMQ
// Author           : Guilherme Branco Stracini
// Created          : 09-28-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 03-31-2021
// ***********************************************************************
// <copyright file="MessageReceivedArgs.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace CrispyWaffle.RabbitMQ.Helpers
{
    /// <summary>
    /// Class MessageReceivedArgs.
    /// </summary>
    public class MessageReceivedArgs
    {
        /// <summary>
        /// Gets or sets the name of the queue.
        /// </summary>
        /// <value>The name of the queue.</value>
        public string QueueName { get; set; }

        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        /// <value>The body.</value>
        public string Body { get; set; }
    }
}
