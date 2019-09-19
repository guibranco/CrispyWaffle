namespace CrispyWaffle.Attributes
{
    using System;
    using System.ComponentModel;

    /// <summary>
    ///     <para>This attribute is used to show a human readable text of the description of the field</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Localizable(true)]
    public sealed class HumanReadableAttribute : Attribute
    {
        /// <summary>
        /// Gets the string value.
        /// </summary>
        /// <value>
        /// The string value.
        /// </value>
        public string StringValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HumanReadableAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public HumanReadableAttribute([Localizable(true)]string value)
        {
            StringValue = value;
        }
    }
}
