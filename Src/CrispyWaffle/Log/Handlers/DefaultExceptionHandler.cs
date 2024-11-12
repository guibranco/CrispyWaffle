using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CrispyWaffle.Composition;
using CrispyWaffle.Extensions;
using CrispyWaffle.Log.Providers;
using CrispyWaffle.Telemetry;

namespace CrispyWaffle.Log.Handlers
{
    /// <summary>
    /// A default exception handler that logs exception details using the available log providers.
    /// It supports logging to multiple providers and handles both full exception details and summarized messages.
    /// </summary>
    /// <seealso cref="IExceptionHandler" />
    public class DefaultExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// The collection of additional log providers to handle exception logging.
        /// Each provider is associated with a specific type of logging (e.g., message or full details).
        /// </summary>
        private static readonly ICollection<
            Tuple<ILogProvider, ExceptionLogType>
        > _additionalProviders = new List<Tuple<ILogProvider, ExceptionLogType>>();

        /// <summary>
        /// Determines the category to use for logging based on the current call stack.
        /// The category is extracted from the namespace of the calling method.
        /// </summary>
        /// <returns>A string representing the log category.</returns>
        private static string GetCategory()
        {
            var stack = new StackTrace();
            var counter = 1;

            while (true)
            {
                var method = stack.GetFrame(counter++).GetMethod();

                if (method == null)
                {
                    return @"CrispyWaffle";
                }

                if (GetNamespace(method, out var category))
                {
                    return category;
                }
            }
        }

        /// <summary>
        /// Retrieves the namespace of the specified method, which is used to determine the log category.
        /// </summary>
        /// <param name="method">The method to inspect.</param>
        /// <param name="category">The resulting log category extracted from the namespace.</param>
        /// <returns><c>true</c> if a valid namespace was found; otherwise, <c>false</c>.</returns>
        private static bool GetNamespace(MethodBase method, out string category)
        {
            category = string.Empty;
            var ns = method.DeclaringType?.FullName;

            if (string.IsNullOrWhiteSpace(ns))
            {
                category = method.Name;
                return true;
            }

            if (ns.StartsWith(@"CrispyWaffle.Log", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (ns.StartsWith(@"CrispyWaffle.", StringComparison.InvariantCultureIgnoreCase))
            {
                ns = ns.Substring(13);
            }

            category = ns;
            return true;
        }

        /// <summary>
        /// Handles the exception by logging it to all available log providers.
        /// It processes the exception, tracks telemetry, and formats the log messages accordingly.
        /// </summary>
        /// <param name="exception">The exception to be logged.</param>
        private static void HandleInternal(Exception exception)
        {
            var category = GetCategory();
            var exceptions = exception.ToQueue(out var types);

            // Track exception telemetry
            foreach (var type in types)
            {
                TelemetryAnalytics.TrackException(type);
            }

            var messages = exceptions.GetMessages(
                category,
                _additionalProviders
                    .Where(p => p.Item2 == ExceptionLogType.Message)
                    .Select(p => p.Item1)
                    .ToList()
            );

            // Log to providers that handle full exception details
            foreach (
                var additionalProvider in _additionalProviders.Where(p =>
                    p.Item2 == ExceptionLogType.Full
                )
            )
            {
                additionalProvider.Item1.Error(category, messages);
            }
        }

        /// <summary>
        /// Logs an exception at the <see cref="LogLevel.Error"/> level.
        /// The exception is logged with its message, stack trace, type name, and inner exceptions (if any).
        /// The behavior of this method depends on the implementation of the log adapter.
        /// </summary>
        /// <param name="exception">The exception to be logged.</param>
        /// <remarks>Requires <see cref="LogLevel.Error"/> to be enabled for logging.</remarks>
        public void Handle(Exception exception) => HandleInternal(exception);

        /// <summary>
        /// Handles an unhandled exception event by casting the exception from <see cref="UnhandledExceptionEventArgs.ExceptionObject"/>
        /// and calling <see cref="Handle(Exception)"/> to log the exception.
        /// This is the default behavior, and custom behavior can be implemented by overriding this method.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The event arguments containing the exception.</param>
        /// <remarks>Requires <see cref="LogLevel.Error"/> to be enabled for logging.</remarks>
        public void Handle(object sender, UnhandledExceptionEventArgs args) =>
            HandleInternal((Exception)args.ExceptionObject);

        /// <summary>
        /// Handles a thread exception by extracting the exception from the event arguments and logging it.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ThreadExceptionEventArgs"/> containing the exception data.</param>
        public void Handle(object sender, ThreadExceptionEventArgs args) =>
            HandleInternal(args.Exception);

        /// <summary>
        /// Adds a log provider to the exception handler. The provider will be used to log exception details
        /// based on the specified <see cref="ExceptionLogType"/> (e.g., message or full exception).
        /// </summary>
        /// <typeparam name="TLogProvider">The type of the log provider to be added.</typeparam>
        /// <param name="type">The log type indicating whether the provider logs messages, full exceptions, or other details.</param>
        /// <returns>An instance of the <see cref="ILogProvider"/> that was added.</returns>
        public ILogProvider AddLogProvider<TLogProvider>(ExceptionLogType type)
            where TLogProvider : ILogProvider
        {
            var provider = ServiceLocator.Resolve<TLogProvider>();
            _additionalProviders.Add(new Tuple<ILogProvider, ExceptionLogType>(provider, type));
            return provider;
        }

        /// <summary>
        /// Attempts to add a <see cref="ConsoleLogProvider"/> to the exception handler if a console is available.
        /// The provider will log exception messages to the console.
        /// </summary>
        public static void TryAddConsoleLogProvider()
        {
            try
            {
                bool consoleAvailable;

                using (var stream = Console.OpenStandardInput(1))
                {
                    consoleAvailable = stream != Stream.Null;
                }

                if (!consoleAvailable)
                {
                    return;
                }

                var instance = ServiceLocator.TryResolve<ConsoleLogProvider>();

                if (instance != null)
                {
                    _additionalProviders.Add(
                        new Tuple<ILogProvider, ExceptionLogType>(
                            instance,
                            ExceptionLogType.Message
                        )
                    );
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Attempts to add a <see cref="TextFileLogProvider"/> to the exception handler.
        /// The provider will log full exception details to a text file.
        /// </summary>
        public static void TryAddTextFileLogProvider()
        {
            try
            {
                var instance = ServiceLocator.TryResolve<TextFileLogProvider>();

                if (instance != null)
                {
                    _additionalProviders.Add(
                        new Tuple<ILogProvider, ExceptionLogType>(instance, ExceptionLogType.Full)
                    );
                }
            }
            catch (Exception) { }
        }
    }
}
