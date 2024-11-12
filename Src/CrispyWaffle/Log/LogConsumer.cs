using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CrispyWaffle.Composition;
using CrispyWaffle.Log.Filters;
using CrispyWaffle.Log.Handlers;
using CrispyWaffle.Log.Providers;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Log
{
    /// <summary>
    /// Provides functionality for logging messages to various log providers and applying filters.
    /// </summary>
    public static class LogConsumer
    {
        /// <summary>
        /// The providers.
        /// </summary>
        private static readonly Collection<ILogProvider> _providers;

        /// <summary>
        /// The filters.
        /// </summary>
        private static readonly Collection<ILogFilter> _filters;

        /// <summary>
        /// The handler.
        /// </summary>
        private static IExceptionHandler _handler;

        /// <summary>
        /// Gets the directory where log files are stored.
        /// </summary>
        public static readonly string StorageDirectory;

        /// <summary>
        /// Gets the directory where debug log files are stored.
        /// </summary>
        public static readonly string DebugDirectory;

        /// <summary>
        /// Initializes static members of the <see cref="LogConsumer"/> class.
        /// </summary>
        static LogConsumer()
        {
            StorageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            DebugDirectory = Path.Combine(StorageDirectory, "Debug");
            _providers = new Collection<ILogProvider>();
            _filters = new Collection<ILogFilter>();
            try
            {
                _handler = ServiceLocator.Resolve<IExceptionHandler>();
            }
            catch (Exception e)
            {
                _handler = ServiceLocator.Resolve<DefaultExceptionHandler>();
                _handler.Handle(e);
            }
        }

        /// <summary>
        /// Determines the category for the current context based on the calling method's namespace.
        /// </summary>
        /// <returns>The category as a string.</returns>
        private static string GetCategory()
        {
            var stack = new StackTrace();
            var counter = 1;
            while (true)
            {
                var method = stack.GetFrame(counter++)?.GetMethod();

                if (method == null)
                {
                    return "CrispyWaffle";
                }

                if (GetNamespace(method, out var category))
                {
                    return category;
                }
            }
        }

        /// <summary>
        /// Retrieves the namespace for the specified method.
        /// </summary>
        /// <param name="method">The method to evaluate.</param>
        /// <param name="category">The resulting category based on the namespace.</param>
        /// <returns>True if a namespace is found; otherwise, false.</returns>
        private static bool GetNamespace(MethodBase method, out string category)
        {
            category = string.Empty;
            var ns = method.DeclaringType?.FullName;

            if (string.IsNullOrWhiteSpace(ns))
            {
                category = method.Name;
                return true;
            }

            if (
                ns.StartsWith("CrispyWaffle.Log", StringComparison.OrdinalIgnoreCase)
                || (
                    ns.StartsWith("CrispyWaffle", StringComparison.OrdinalIgnoreCase)
                    && ns.EndsWith("LogProvider", StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                return false;
            }

            if (ns.StartsWith("CrispyWaffle.", StringComparison.InvariantCultureIgnoreCase))
            {
                ns = ns.Substring(13);
            }

            category = ns;
            return true;
        }

        /// <summary>
        /// Adds a new log provider to the consumer.
        /// </summary>
        /// <typeparam name="TLogProvider">The type of the log provider.</typeparam>
        /// <returns>The instance of the added log provider.</returns>
        public static ILogProvider AddProvider<TLogProvider>()
            where TLogProvider : ILogProvider
        {
            var provider = ServiceLocator.Resolve<TLogProvider>();
            _providers.Add(provider);
            return provider;
        }

        /// <summary>
        /// Adds a specified log provider to the consumer.
        /// </summary>
        /// <param name="provider">The log provider to add.</param>
        /// <returns>The instance of the added log provider.</returns>
        public static ILogProvider AddProvider(ILogProvider provider)
        {
            _providers.Add(provider);
            return provider;
        }

        /// <summary>
        /// Adds a log filter to the consumer.
        /// </summary>
        /// <param name="filter">The log filter to add.</param>
        public static void AddFilter(ILogFilter filter)
        {
            _filters.Add(filter);
        }

        /// <summary>
        /// Sets the exception handler for logging operations.
        /// </summary>
        /// <param name="handler">The exception handler to set.</param>
        public static void SetHandler(IExceptionHandler handler)
        {
            _handler = handler;
        }

        /// <summary>
        /// Logs a message to the specified log provider.
        /// </summary>
        /// <typeparam name="TLogProvider">The type of the log provider.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="message">The message to log.</param>
        /// <returns>True if the message was logged; otherwise, false.</returns>
        public static bool LogTo<TLogProvider>(LogLevel level, string message)
            where TLogProvider : ILogProvider
        {
            var type = typeof(TLogProvider);

            var provider = _providers.SingleOrDefault(p => type == p.GetType());

            if (provider == null)
            {
                return false;
            }

            var category = GetCategory();

            if (_filters.Any(f => !f.Filter(type.FullName, level, category, message)))
            {
                return false;
            }

            LogToInternal(level, message, provider, category);

            return true;
        }

        /// <summary>
        /// Internal method for logging messages to a provider.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="provider">The log provider.</param>
        /// <param name="category">The category of the log.</param>
        /// <exception cref="ArgumentOutOfRangeException">level - null.</exception>
        private static void LogToInternal(
            LogLevel level,
            string message,
            ILogProvider provider,
            string category
        )
        {
            switch (level)
            {
                case LogLevel.Fatal:
                    provider.Fatal(category, message);
                    break;
                case LogLevel.Error:
                    provider.Error(category, message);
                    break;
                case LogLevel.Warning:
                    provider.Warning(category, message);
                    break;
                case LogLevel.Info:
                    provider.Info(category, message);
                    break;
                case LogLevel.Trace:
                    provider.Trace(category, message);
                    break;
                case LogLevel.Debug:
                    provider.Debug(category, message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        /// <summary>
        /// Debugs to specific log provider.
        /// </summary>
        /// <typeparam name="TLogProvider">The type of the t log provider.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="identifier">The identifier.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise.</returns>
        public static bool DebugTo<TLogProvider>(string content, string identifier)
            where TLogProvider : ILogProvider
        {
            var type = typeof(TLogProvider);

            var provider = _providers.SingleOrDefault(p => type == p.GetType());

            if (provider == null)
            {
                return false;
            }

            var category = GetCategory();

            if (_filters.Any(f => !f.Filter(type.FullName, LogLevel.Debug, category, content)))
            {
                return false;
            }

            provider.Debug(category, content, identifier);

            return true;
        }

        /// <summary>
        /// Debugs to specif log provider.
        /// </summary>
        /// <typeparam name="TLogProvider">The type of the t log provider.</typeparam>
        /// <typeparam name="T">The type of the file to be persisted.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="customFormat">The custom format.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise.</returns>
        public static bool DebugTo<TLogProvider, T>(
            T content,
            string identifier,
            SerializerFormat customFormat = SerializerFormat.None
        )
            where TLogProvider : ILogProvider
            where T : class, new()
        {
            var type = typeof(TLogProvider);

            var provider = _providers.SingleOrDefault(p => type == p.GetType());

            if (provider == null)
            {
                return false;
            }

            var category = GetCategory();

            if (_filters.Any(f => !f.Filter(type.FullName, LogLevel.Debug, category, string.Empty)))
            {
                return false;
            }

            provider.Debug(category, content, identifier, customFormat);

            return true;
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Debug(string message)
        {
            var category = GetCategory();

            foreach (
                var provider in _providers.Where(p =>
                    _filters.All(f =>
                        f.Filter(p.GetType().FullName, LogLevel.Debug, category, message)
                    )
                )
            )
            {
                provider.Debug(category, message);
            }
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="arguments">The arguments.</param>
        public static void Debug(string message, params object[] arguments)
        {
            Debug(string.Format(CultureInfo.InvariantCulture, message, arguments));
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="identifier">The identifier.</param>
        public static void Debug(string content, [Localizable(false)] string identifier)
        {
            var category = GetCategory();

            foreach (
                var provider in _providers.Where(p =>
                    _filters.All(f =>
                        f.Filter(p.GetType().FullName, LogLevel.Debug, category, content)
                    )
                )
            )
            {
                provider.Debug(category, content, identifier);
            }
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <typeparam name="T">The type of the content.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="customFormat">The custom format.</param>
        public static void Debug<T>(
            T content,
            [Localizable(false)] string identifier,
            SerializerFormat customFormat = SerializerFormat.None
        )
            where T : class, new()
        {
            var category = GetCategory();

            foreach (
                var provider in _providers.Where(p =>
                    _filters.All(f =>
                        f.Filter(p.GetType().FullName, LogLevel.Debug, category, string.Empty)
                    )
                )
            )
            {
                provider.Debug(category, content, identifier, customFormat);
            }
        }

        /// <summary>
        /// Logs a trace message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Trace(string message)
        {
            var category = GetCategory();

            foreach (
                var provider in _providers.Where(p =>
                    _filters.All(f =>
                        f.Filter(p.GetType().FullName, LogLevel.Trace, category, message)
                    )
                )
            )
            {
                provider.Trace(category, message);
            }
        }

        /// <summary>
        /// Logs a trace message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="arguments">The arguments.</param>
        public static void Trace(string message, params object[] arguments)
        {
            Trace(string.Format(CultureInfo.InvariantCulture, message, arguments));
        }

        /// <summary>
        /// Logs a trace message with an exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        public static void Trace(Exception exception, string message)
        {
            var category = GetCategory();

            foreach (
                var provider in _providers.Where(p =>
                    _filters.All(f =>
                        f.Filter(p.GetType().FullName, LogLevel.Trace, category, message)
                    )
                )
            )
            {
                provider.Trace(category, message, exception);
            }
        }

        /// <summary>
        /// Logs a trace message with an exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="arguments">The arguments.</param>
        public static void Trace(Exception exception, string message, params object[] arguments)
        {
            Trace(exception, string.Format(CultureInfo.InvariantCulture, message, arguments));
        }

        /// <summary>
        /// Logs a trace message with an exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public static void Trace(Exception exception)
        {
            var category = GetCategory();

            foreach (
                var provider in _providers.Where(p =>
                    _filters.All(f =>
                        f.Filter(p.GetType().FullName, LogLevel.Trace, category, exception.Message)
                    )
                )
            )
            {
                provider.Trace(category, exception);
            }
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Info(string message)
        {
            var category = GetCategory();

            foreach (
                var provider in _providers.Where(p =>
                    _filters.All(f =>
                        f.Filter(p.GetType().FullName, LogLevel.Info, category, message)
                    )
                )
            )
            {
                provider.Info(category, message);
            }
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="arguments">The arguments.</param>
        public static void Info(string message, params object[] arguments)
        {
            Info(string.Format(CultureInfo.InvariantCulture, message, arguments));
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Warning(string message)
        {
            var category = GetCategory();

            foreach (
                var provider in _providers.Where(p =>
                    _filters.All(f =>
                        f.Filter(p.GetType().FullName, LogLevel.Warning, category, message)
                    )
                )
            )
            {
                provider.Warning(category, message);
            }
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="arguments">The arguments.</param>
        public static void Warning(string message, params object[] arguments)
        {
            Warning(string.Format(CultureInfo.InvariantCulture, message, arguments));
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Error(string message)
        {
            var category = GetCategory();

            foreach (
                var provider in _providers.Where(p =>
                    _filters.All(f =>
                        f.Filter(p.GetType().FullName, LogLevel.Error, category, message)
                    )
                )
            )
            {
                provider.Error(category, message);
            }
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="arguments">The arguments.</param>
        public static void Error(string message, params object[] arguments)
        {
            Error(string.Format(CultureInfo.InvariantCulture, message, arguments));
        }

        /// <summary>
        /// Logs a fatal error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Fatal(string message)
        {
            var category = GetCategory();

            foreach (
                var provider in _providers.Where(p =>
                    _filters.All(f =>
                        f.Filter(p.GetType().FullName, LogLevel.Fatal, category, message)
                    )
                )
            )
            {
                provider.Fatal(category, message);
            }
        }

        /// <summary>
        /// Logs a fatal error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="arguments">The arguments.</param>
        public static void Fatal(string message, params object[] arguments)
        {
            Fatal(string.Format(CultureInfo.InvariantCulture, message, arguments));
        }

        /// <summary>
        /// Handles an exception by passing it to the exception handler.
        /// </summary>
        /// <param name="exception">The exception to handle.</param>
        public static void Handle(Exception exception)
        {
            _handler?.Handle(exception);
        }

        /// <summary>
        /// Handles an exception by passing it to the exception handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        public static void Handle(object sender, UnhandledExceptionEventArgs args)
        {
            _handler?.Handle(sender, args);
        }

        /// <summary>
        /// Handles an exception by passing it to the exception handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ThreadExceptionEventArgs"/> instance containing the event data.</param>
        public static void Handle(object sender, ThreadExceptionEventArgs args)
        {
            _handler?.Handle(sender, args);
        }
    }
}
