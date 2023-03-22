// ***********************************************************************
// Assembly         : CrispyWaffle.Configuration
// Author           : Guilherme Branco Stracini
// Created          : 09-03-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-03-2020
// ***********************************************************************
// <copyright file="ConnectionNameAttribute.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.Configuration
{
    using System;

    /// <summary>
    /// Connection name attribute class. This class cannot be inherited.
    /// </summary>

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ConnectionNameAttribute : Attribute
    {

        /// <summary>
        /// The connection name
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the order of the connection in the constructor parameters, index based on zero.
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; set; }

        /// <summary>
        /// Initializes a new instance o ConnectionNameAttribute class
        /// </summary>
        /// <param name="name">The name.</param>
        public ConnectionNameAttribute(string name)
        {
            Name = name;
        }
    }
}
