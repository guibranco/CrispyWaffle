namespace CrispyWaffle.RabbitMQ.Helpers;

/// <summary>
/// Provides extension methods for retrieving RabbitMQ exchange and queue names based on class metadata.
/// These methods help in dynamically determining the names of exchanges and queues used in RabbitMQ communication
/// by inspecting custom attributes or falling back to default naming conventions.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Retrieves the exchange name associated with the specified class.
    /// This method looks for an <see cref="ExchangeNameAttribute"/> applied to the class and returns the specified exchange name.
    /// If the attribute is not present, it generates a default name based on the class name, converting spaces to hyphens.
    /// </summary>
    /// <typeparam name="T">The type of the class implementing <see cref="IQueuing"/>.</typeparam>
    /// <returns>
    /// A <see cref="string"/> representing the name of the exchange.
    /// If an <see cref="ExchangeNameAttribute"/> is found, its value is returned; otherwise, the class name is used with spaces replaced by hyphens.
    /// </returns>
    public static string GetExchangeName<T>()
        where T : class, IQueuing, new()
    {
        var type = typeof(T);
        return
            type.GetCustomAttributes(typeof(ExchangeNameAttribute), true)
                is ExchangeNameAttribute[] attributes
            && attributes.Length == 1
            ? attributes[0].ExchangeName
            : type.Name.ToLowerInvariant().Replace(@" ", @"-");
    }

    /// <summary>
    /// Retrieves the queue name associated with the specified class.
    /// This method looks for a <see cref="QueueNameAttribute"/> applied to the class and returns the specified queue name.
    /// If the attribute is not present, it generates a default name based on the class name, converting spaces to hyphens.
    /// </summary>
    /// <typeparam name="T">The type of the class implementing <see cref="IQueuing"/>.</typeparam>
    /// <returns>
    /// A <see cref="string"/> representing the name of the queue.
    /// If a <see cref="QueueNameAttribute"/> is found, its value is returned; otherwise, the class name is used with spaces replaced by hyphens.
    /// </returns>
    public static string GetQueueName<T>()
        where T : class, IQueuing, new()
    {
        var type = typeof(T);

        return
            type.GetCustomAttributes(typeof(QueueNameAttribute), true)
                is QueueNameAttribute[] attributes
            && attributes.Length == 1
            ? attributes[0].QueueName
            : type.Name.ToLowerInvariant().Replace(@" ", @"-");
    }
}
