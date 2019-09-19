namespace CrispyWaffle.Attributes
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Class InternalValueAttribute. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class InternalValueAttribute : Attribute
    {
        /// <summary>
        /// Gets the internal value.
        /// </summary>
        /// <value>The internal value.</value>
        public string InternalValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalValueAttribute" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public InternalValueAttribute([Localizable(false)]string value)
        {
            InternalValue = value;
        }
    }
}
