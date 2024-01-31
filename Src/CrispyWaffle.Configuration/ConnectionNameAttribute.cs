using System;

namespace CrispyWaffle.Configuration
{
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
