using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrispyWaffle.Log.Providers;

namespace CrispyWaffle.Extensions;

/// <summary>
/// Provides extension methods for handling and processing exceptions.
/// </summary>
/// <remarks>
/// This static class contains extension methods that facilitate the conversion of exceptions
/// into a queue of exceptions and allows the retrieval of detailed exception messages,
/// along with the option to log additional information using external providers.
/// </remarks>
public static class ExceptionExtensions
{
    /// <summary>
    /// Converts the exception and its inner exceptions into a queue and outputs the types of the inner exceptions.
    /// </summary>
    /// <param name="exception">The initial exception to be processed.</param>
    /// <param name="types">A list that will contain the types of all inner exceptions, if any.</param>
    /// <returns>A queue containing the exception and all of its inner exceptions in reverse order.</returns>
    /// <remarks>
    /// This method starts with the provided exception and iterates through its inner exceptions,
    /// enqueuing each exception into a queue. The types of the inner exceptions are also recorded
    /// in the provided list. The final queue is reversed to maintain the order from the outermost
    /// exception to the innermost exception.
    /// </remarks>
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
    /// Retrieves detailed messages from a queue of exceptions, including optional error logging with additional providers.
    /// </summary>
    /// <param name="exceptions">The queue of exceptions to process.</param>
    /// <param name="category">The category under which the error should be logged (e.g., "Database", "API").</param>
    /// <param name="additionalProviders">A collection of <see cref="ILogProvider"/> instances used for logging additional error details.</param>
    /// <returns>A formatted string containing the messages for each exception in the queue.</returns>
    /// <remarks>
    /// This method dequeues each exception from the queue, appends its message, type, and stack trace
    /// to a StringBuilder, and logs the message through the provided additional providers.
    /// It also adds a rethrow marker with a counter to indicate the position of the exception
    /// in the original exception chain.
    /// </remarks>
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
