// ***********************************************************************
// Assembly         : CrispyWaffle.RabbitMQ
// Author           : Guilherme Branco Stracini
// Created          : 09-07-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 05-05-2021
// ***********************************************************************
// <copyright file="QueueNameAttribute.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.RabbitMQ.Helpers
{
    using System;

    /// <summary>
    /// Class QueueNameAttribute. This class cannot be inherited.
    /// Implements the <see cref="System.Attribute" />
    /// </summary>
    /// <seealso cref="System.Attribute" />
    public sealed class QueueNameAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueueNameAttribute" /> class.
        /// </summary>
        /// <param name="queueName">Name of the queue.</param>
        public QueueNameAttribute(string queueName) => QueueName = queueName;

        /// <summary>
        /// Gets or sets the name of the queue.
        /// </summary>
        /// <value>The name of the queue.</value>
        public string QueueName { get; }
    }
}
