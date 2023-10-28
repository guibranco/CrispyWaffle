﻿// ***********************************************************************
// Assembly         : CrispyWaffle.RabbitMQ
// Author           : Guilherme Branco Stracini
// Created          : 09-28-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 05-05-2021
// ***********************************************************************
// <copyright file="ExchangeNameAttribute.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;

namespace CrispyWaffle.RabbitMQ.Helpers
{
    /// <summary>
    /// Class ExchangeNameAttribute. This class cannot be inherited.
    /// Implements the <see cref="Attribute" />.
    /// </summary>
    /// <seealso cref="Attribute" />.
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ExchangeNameAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeNameAttribute" /> class.
        /// </summary>
        /// <param name="exchangeName">Name of the exchange.</param>
        public ExchangeNameAttribute(string exchangeName) => ExchangeName = exchangeName;

        /// <summary>
        /// Gets the name of the exchange.
        /// </summary>
        /// <value>The name of the exchange.</value>
        public string ExchangeName { get; }
    }
}
