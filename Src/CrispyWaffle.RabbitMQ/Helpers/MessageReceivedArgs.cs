namespace CrispyWaffle.RabbitMQ.Helpers;

/// <summary>
/// Class MessageReceivedArgs.
/// </summary>
public class MessageReceivedArgs
{
    /// <summary>
    /// Gets or sets the name of the queue.
    /// </summary>
    /// <value>The name of the queue.</value>
    public string QueueName { get; set; }

    /// <summary>
    /// Gets or sets the body.
    /// </summary>
    /// <value>The body.</value>
    public string Body { get; set; }
}
