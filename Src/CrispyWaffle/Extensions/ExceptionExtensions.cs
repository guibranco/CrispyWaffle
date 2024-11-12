using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrispyWaffle.Log.Providers;

namespace CrispyWaffle.Extensions;

/// <summary>
/// The exception extension class.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// To the queue.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="types">The types.</param>
    /// <returns></returns>
    public static Queue<Exception> ToQueue(this Exception exception, out List<Type> types)
    {
        var result = new Queue<Exception>();

        types = new List<Type>();

        var handling = exception;

        result.Enqueue(handling);

        while (handling?.InnerException != null)
        {
            result.Enqueue(handling.InnerException);

            handling = handling.InnerException;

            if (handling != null)
            {
                types.Add(handling.GetType());
            }
        }

        result = new Queue<Exception>(result.Reverse());

        return result;
    }

    /// <summary>
    /// Gets the messages.
    /// </summary>
    /// <param name="exceptions">The exceptions.</param>
    /// <param name="category">The category</param>
    /// <param name="additionalProviders">The additional providers.</param>
    /// <returns></returns>
    public static string GetMessages(
        this Queue<Exception> exceptions,
        string category,
        ICollection<ILogProvider> additionalProviders
    )
    {
        var message = new StringBuilder();

        var counter = 0;

        while (exceptions.Count > 0)
        {
            var current = exceptions.Dequeue();

            if (counter > 0)
            {
                message.Append("Exception rethrow at").Append(@" [").Append(counter).Append(@"]: ");
            }

            message
                .Append(current.Message)
                .AppendFormat(" [{0}]", current.GetType().Name)
                .AppendLine()
                .AppendLine(current.StackTrace);

            counter++;

            foreach (var additionalProvider in additionalProviders)
            {
                additionalProvider.Error(category, current.Message);
            }
        }

        return message.ToString();
    }
}
