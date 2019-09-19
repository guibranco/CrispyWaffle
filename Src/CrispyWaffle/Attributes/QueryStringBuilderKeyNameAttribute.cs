namespace CrispyWaffle.Attributes
{
    using System;

    /// <summary>
    /// The query string key name attribute
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class QueryStringBuilderKeyNameAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the key.
        /// </summary>
        /// <value>
        /// The name of the key.
        /// </value>
        public string KeyName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStringBuilderKeyNameAttribute"/> class.
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        public QueryStringBuilderKeyNameAttribute(string keyName)
        {
            KeyName = keyName;
        }
    }
}
