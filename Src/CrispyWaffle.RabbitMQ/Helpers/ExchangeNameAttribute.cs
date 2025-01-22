using System;

namespace CrispyWaffle.RabbitMQ.Helpers;

/// <summary>
/// Class ExchangeNameAttribute. This class cannot be inherited.
/// Implements the <see cref="Attribute" />.
/// </summary>
/// <seealso cref="Attribute" />.
[AttributeUsage(AttributeTargets.Class)]
public sealed class ExchangeNameAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExchangeNameAttribute" /> class.
    /// </summary>
    /// <param name="exchangeName">Name of the exchange.</param>
    public ExchangeNameAttribute(string exchangeName) => ExchangeName = exchangeName;

    /// <summary>
    /// Gets the name of the exchange.
    /// </summary>
    /// <value>The name of the exchange.</value>
    public string ExchangeName { get; }
}
