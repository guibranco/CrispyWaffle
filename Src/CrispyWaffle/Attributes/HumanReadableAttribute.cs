﻿using System;
using System.ComponentModel;

namespace CrispyWaffle.Attributes;

/// <summary>
/// This attribute is used to show a human-readable text of the description of the field.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="HumanReadableAttribute"/> class.
/// </remarks>
/// <param name="value">The value.</param>
[AttributeUsage(AttributeTargets.Field)]
[Localizable(true)]
public sealed class HumanReadableAttribute([Localizable(true)] string value) : Attribute
{
    /// <summary>
    /// Gets the string value.
    /// </summary>
    /// <value>
    /// The string value.
    /// </value>
    public string StringValue { get; } = value;
}
