using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using CrispyWaffle.Composition;
using CrispyWaffle.Log.Providers;

namespace CrispyWaffle.Log.Handlers
{
    /// <summary>
    /// A static class responsible for handling exceptions with a failover mechanism.
    /// It logs the original exception and handles additional errors that may occur during
    /// logging by writing a detailed error report to a file.
    /// </summary>
    public static class FailoverExceptionHandler
    {
        /// <summary>
        /// Handles the specified exception by attempting to log it and handle any
        /// errors that occur during the logging process. If an error occurs, a
        /// detailed error report is written to a log file.
        /// </summary>
        /// <param name="exception">The exception to be handled.</param>
        /// <remarks>
        /// This method first attempts to resolve the default exception handler and
        /// add a log provider. If any errors occur during the logging process,
        /// the details of both the original exception and the error that occurred
        /// are written to a file in the format "fatal-yyyyMMddHHmmss.log".
        /// </remarks>
        public static void Handle(Exception exception)
        {
            try
            {
                // Resolves the default exception handler and sets up the log provider
                ServiceLocator
                    .Resolve<DefaultExceptionHandler>()
                    .AddLogProvider<TextFileLogProvider>(ExceptionLogType.Full);

                // Handles the exception via the log consumer
                LogConsumer.Handle(exception);
            }
            catch (Exception ex)
            {
                // In case of an error, log the details of both the original and current exceptions
                var builder = new StringBuilder()
                    .AppendFormat("Original exception: {0}\r\n", GetMessage(exception))
                    .AppendFormat("Current exception: {0}", GetMessage(ex));

                // Write the exception details to a log file
                File.WriteAllText(
                    $@"fatal-{DateTime.Now:yyyyMMddHHmmss}.log",
                    builder.ToString(),
                    Encoding.UTF8
                );
            }
        }

        /// <summary>
        /// Constructs a detailed message for the specified exception, including its message,
        /// stack trace, and any inner exceptions. If the exception is a
        /// <see cref="ReflectionTypeLoadException"/>, the loader exceptions are included.
        /// </summary>
        /// <param name="ex">The exception whose message is to be constructed.</param>
        /// <returns>A string representing the detailed message of the exception.</returns>
        private static string GetMessage(Exception ex)
        {
            var builder = new StringBuilder();

            while (ex != null)
            {
                builder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "Message: {0}\r\nStackTrace: {1}\r\n",
                    ex.Message,
                    ex.StackTrace
                );

                // If the exception is a ReflectionTypeLoadException, include the loader exceptions
                if (ex is ReflectionTypeLoadException reflectionException)
                {
                    builder.Append(GetMessages(reflectionException.LoaderExceptions));
                }

                // Traverse the inner exception chain
                ex = ex.InnerException;
            }

            return builder.ToString();
        }

        /// <summary>
        /// Constructs a detailed message for an array of exceptions, including their
        /// messages, stack traces, and any inner exceptions.
        /// </summary>
        /// <param name="exceptions">The array of exceptions whose messages are to be constructed.</param>
        /// <returns>A string representing the detailed messages of the exceptions.</returns>
        private static string GetMessages(Exception[] exceptions)
        {
            var builder = new StringBuilder();

            var counter = 0;

            foreach (var exception in exceptions)
            {
                builder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "#{0} exception: {1}\r\n",
                    counter++,
                    GetMessage(exception)
                );
            }

            return builder.ToString();
        }
    }
}
