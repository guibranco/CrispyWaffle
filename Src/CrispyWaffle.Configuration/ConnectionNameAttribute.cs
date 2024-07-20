using System;

namespace CrispyWaffle.Configuration
{
    /// <summary>
    /// Class ConnectionNameAttribute. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ConnectionNameAttribute : Attribute
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionNameAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ConnectionNameAttribute(string name)
        {
            Name = name;
        }
    }
}
