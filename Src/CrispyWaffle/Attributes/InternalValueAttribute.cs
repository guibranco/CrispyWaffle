using System;
using System.ComponentModel;

namespace CrispyWaffle.Attributes;

/// <summary>
/// Class InternalValueAttribute. This class cannot be inherited.
/// </summary>
/// <seealso cref="Attribute" />
/// <remarks>
/// Initializes a new instance of the <see cref="InternalValueAttribute" /> class.
/// </remarks>
/// <param name="value">The value.</param>
[AttributeUsage(AttributeTargets.Field)]
public sealed class InternalValueAttribute([Localizable(false)] string value) : Attribute
{
    /// <summary>
    /// Gets the internal value.
    /// </summary>
    /// <value>The internal value.</value>
    public string InternalValue { get; } = value;
}
