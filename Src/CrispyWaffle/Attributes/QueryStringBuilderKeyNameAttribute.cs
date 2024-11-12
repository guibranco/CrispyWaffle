using System;

namespace CrispyWaffle.Attributes;

/// <summary>
/// The query string key name attribute.
/// </summary>
/// <seealso cref="Attribute" />
/// <remarks>
/// Initializes a new instance of the <see cref="QueryStringBuilderKeyNameAttribute"/> class.
/// </remarks>
/// <param name="keyName">Name of the key.</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class QueryStringBuilderKeyNameAttribute(string keyName) : Attribute
{
    /// <summary>
    /// Gets the name of the key.
    /// </summary>
    /// <value>
    /// The name of the key.
    /// </value>
    public string KeyName { get; } = keyName;
}
