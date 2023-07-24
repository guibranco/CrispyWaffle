namespace CrispyWaffle.Log.Handlers
{
    using Composition;
    using Providers;
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// The failover exception handler
    /// </summary>
    public static class FailoverExceptionHandler
    {
        /// <summary>
        /// Handles the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public static void Handle(Exception exception)
        {
            try
            {
                //Tries to log the exception using CrispyWaffle framework behavior
                ServiceLocator
                    .Resolve<DefaultExceptionHandler>()
                    .AddLogProvider<EventLogProvider>(ExceptionLogType.Full);

                LogConsumer.Handle(exception);
            }
            catch (Exception ex)
            {
                var builder = new StringBuilder()
                    .AppendFormat("Original exception: {0}\r\n", GetMessage(exception))
                    .AppendFormat("Current exception: {0}", GetMessage(ex));

                System.IO.File.WriteAllText(
                    $@"fatal-{DateTime.Now:yyyyMMddHHmmss}.log",
                    builder.ToString(),
                    Encoding.UTF8
                );
            }
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns>System.String.</returns>
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

                if (ex is ReflectionTypeLoadException reflectionException)
                {
                    builder.Append(GetMessages(reflectionException.LoaderExceptions));
                }

                ex = ex.InnerException;
            }

            return builder.ToString();
        }

        /// <summary>
        /// Gets the messages.
        /// </summary>
        /// <param name="exceptions">The exceptions.</param>
        /// <returns>System.String.</returns>
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
