namespace CrispyWaffle.Serialization
{
    using System;

    /// <summary>
    /// Attribute for serializer.
    /// </summary>
    /// <seealso cref="Attribute" />

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = true)]
    public sealed class SerializerAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializerAttribute"/> class.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="isStrict">if set to <c>true</c> [is strict].</param>
        public SerializerAttribute(SerializerFormat format = SerializerFormat.Xml, bool isStrict = true)
        {
            Format = format;
            IsStrict = isStrict;
        }

        /// <summary>
        /// Gets the format.
        /// </summary>
        /// <value>The format.</value>
        public SerializerFormat Format { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is strict.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is strict; otherwise, <c>false</c>.
        /// </value>
        public bool IsStrict { get; }
    }
}
