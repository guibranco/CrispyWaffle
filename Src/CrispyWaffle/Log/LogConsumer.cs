namespace CrispyWaffle.Log
{
    using Composition;
    using Extensions;
    using Filters;
    using Handlers;
    using Providers;
    using Serialization;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// The default log consumer of the application
    /// </summary>
    public static class LogConsumer
    {
        #region Private members

        /// <summary>
        /// The log providers 
        /// </summary>
        private static readonly ICollection<ILogProvider> Providers;

        /// <summary>
        /// The filters
        /// </summary>
        private static readonly ICollection<ILogFilter> Filters;

        /// <summary>
        /// The exception handler
        /// </summary>
        private static IExceptionHandler _handler;

        #endregion

        #region Public properties

        /// <summary>
        /// The storage directory/
        /// </summary>
        public static readonly string StorageDirectory;

        /// <summary>
        /// The debug directory
        /// </summary>
        public static readonly string DebugDirectory;

        #endregion

        #region ~Ctor

        /// <summary>
        /// Initializes the <see cref="LogConsumer"/> class.
        /// </summary>
        static LogConsumer()
        {
            StorageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            DebugDirectory = Path.Combine(StorageDirectory, "Debug");
            Providers = new Collection<ILogProvider>();
            Filters = new Collection<ILogFilter>();
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

        #endregion

        #region Private methods

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <returns></returns>
        private static string GetCategory()
        {
            var stack = new StackTrace();
            var counter = 1;
            while (true)
            {
                var method = stack.GetFrame(counter++).GetMethod();
                if (method == null)
                    return @"CrispyWaffle";
                var ns = method.DeclaringType?.FullName;
                if (string.IsNullOrWhiteSpace(ns))
                    return method.Name;
                if (ns.StartsWith(@"CrispyWaffle.Log") ||
                    ns.StartsWith(@"CrispyWaffle") && ns.EndsWith(@"LogProvider"))
                    continue;
                if (ns.StartsWith(@"CrispyWaffle.", StringExtensions.Comparison))
                    ns = ns.Substring(13);
                return ns;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds the provider.
        /// </summary>
        /// <typeparam name="TILogProvider">The type of the i log provider.</typeparam>
        /// <returns></returns>
        public static ILogProvider AddProvider<TILogProvider>() where TILogProvider : ILogProvider
        {
            var provider = ServiceLocator.Resolve<TILogProvider>();
            Providers.Add(provider);
            return provider;
        }

        /// <summary>
        /// Adds a provider to the providers lists and return the provider 
        /// </summary>
        /// <param name="provider"><see cref="ILogProvider"/></param>
        /// <returns>The <paramref name="provider"/></returns>
        public static ILogProvider AddProvider(ILogProvider provider)
        {
            Providers.Add(provider);
            return provider;
        }

        /// <summary>
        /// Adds the filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public static void AddFilter(ILogFilter filter)
        {
            Filters.Add(filter);
        }

        /// <summary>
        /// Sets the exception handler
        /// </summary>
        /// <param name="handler">A instance of <see cref="IExceptionHandler"/> used to handle the exception</param>
        public static void SetHandler(IExceptionHandler handler)
        {
            _handler = handler;
        }

        /// <summary>
        /// Log the message to an specific log provider with the specified log level.
        /// </summary>
        /// <typeparam name="TLogProvider">The log provider to log the message</typeparam>
        /// <param name="level">The level of message to be logged</param>
        /// <param name="message">The message to be logged</param>
        /// <returns>True if the log provider exists in the providers list, false if not</returns>
        public static bool LogTo<TLogProvider>(LogLevel level, string message) where TLogProvider : ILogProvider
        {
            var type = typeof(TLogProvider);
            var provider = Providers.SingleOrDefault(p => type == p.GetType());
            if (provider == null)
                return false;
            var category = GetCategory();
            if (Filters.Any(f => !f.Filter(type.FullName, level, category, message)))
                return false;
            switch (level)
            {
                case LogLevel.ERROR:
                    provider.Error(category, message);
                    break;
                case LogLevel.WARNING:
                    provider.Warning(category, message);
                    break;
                case LogLevel.INFO:
                    provider.Info(category, message);
                    break;
                case LogLevel.TRACE:
                    provider.Trace(category, message);
                    break;
                case LogLevel.DEBUG:
                    provider.Debug(category, message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
            return true;
        }

        /// <summary>
        /// Debugs the content to a file/attachment with file name/key identifier as debug level
        /// </summary>
        /// <typeparam name="TLogProvider">The log provider to act on</typeparam>
        /// <param name="content">The content to be stored in the file/attachment</param>
        /// <param name="identifier">The file name/identifier of the file/attachment</param>
        /// <returns>True if the log provider exists in the providers list</returns>
        public static bool DebugTo<TLogProvider>(string content, string identifier)
            where TLogProvider : ILogProvider
        {
            var type = typeof(TLogProvider);
            var provider = Providers.SingleOrDefault(p => type == p.GetType());
            if (provider == null)
                return false;
            var category = GetCategory();
            if (Filters.Any(f => !f.Filter(type.FullName, LogLevel.DEBUG, category, content)))
                return false;
            provider.Debug(category, content, identifier);
            return true;
        }

        /// <summary>
        /// Debugs the content to a file/attachment with file name/key identifier as debug level
        /// </summary>
        /// <typeparam name="TLogProvider">The log provider to act on</typeparam>
        /// <typeparam name="T">The object to be stored</typeparam>
        /// <param name="content">The content to be stored in the file/attachment</param>
        /// <param name="identifier">The file name/identifier of the file/attachment</param>
        /// <param name="customFormat">(Optional) the custom serializer format</param>
        /// <returns>True if the log provider exists in the providers list</returns>
        public static bool DebugTo<TLogProvider, T>(
            T content,
            string identifier,
            SerializerFormat customFormat = SerializerFormat.NONE)
            where TLogProvider : ILogProvider
            where T : class, new()
        {
            var type = typeof(TLogProvider);
            var provider = Providers.SingleOrDefault(p => type == p.GetType());
            if (provider == null)
                return false;
            var category = GetCategory();
            if (Filters.Any(f => !f.Filter(type.FullName, LogLevel.DEBUG, category, string.Empty)))
                return false;
            provider.Debug(category, content, identifier, customFormat);
            return true;
        }

        /// <summary>
        /// Logs the message with debug log level
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message)
        {
            var category = GetCategory();
            foreach (var provider in Providers.Where(p => Filters.All(f => f.Filter(p.GetType().FullName, LogLevel.DEBUG, category, message))))
                provider.Debug(category, message);
        }

        /// <summary>
        /// Logs the message as formatted string with debug log level
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="arguments">The arguments to format the message.</param>
        public static void Debug(string message, params object[] arguments)
        {
            Debug(string.Format(message, arguments));
        }

        /// <summary>
        /// Logs the message as a file/attachment with a file name/identifier with debug level
        /// </summary>
        /// <param name="content">The content to be stored</param>
        /// <param name="identifier">The file name of the content. This can be a filename, a key, a identifier. Depends upon each implementation</param>
        public static void Debug(string content, [Localizable(false)] string identifier)
        {
            var category = GetCategory();
            foreach (var provider in Providers.Where(p => Filters.All(f => f.Filter(p.GetType().FullName, LogLevel.DEBUG, category, content))))
                provider.Debug(category, content, identifier);
        }

        /// <summary>
        /// Logs the message as a file/attachment with a file name/identifier with debug level using a custom serializer or default.
        /// </summary>
        /// <typeparam name="T">any class that can be serialized to the <paramref name="customFormat"/> serializer format</typeparam>
        /// <param name="content">The object to be serialized</param>
        /// <param name="identifier">The filename/attachment identifier (file name or key)</param>
        /// <param name="customFormat">(Optional) the custom serializer format</param>
        public static void Debug<T>(T content, [Localizable(false)] string identifier, SerializerFormat customFormat = SerializerFormat.NONE)
            where T : class, new()
        {
            var category = GetCategory();
            foreach (var provider in Providers.Where(p => Filters.All(f => f.Filter(p.GetType().FullName, LogLevel.DEBUG, category, string.Empty))))
                provider.Debug(category, content, identifier, customFormat);
        }

        /// <summary>
        /// Logs the message with tracing log level
        /// </summary>
        /// <param name="message">The message to be logged</param>
        public static void Trace(string message)
        {
            var category = GetCategory();
            foreach (var provider in Providers.Where(p => Filters.All(f => f.Filter(p.GetType().FullName, LogLevel.TRACE, category, message))))
                provider.Trace(category, message);
        }

        /// <summary>
        /// Logs the message as formatted string with trace log level
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="arguments">The arguments to format the message.</param>
        public static void Trace(string message, params object[] arguments)
        {
            Trace(string.Format(message, arguments));
        }

        /// <summary>
        /// Logs the message with info log level
        /// </summary>
        /// <param name="message">The message to be logged</param>
        public static void Info(string message)
        {
            var category = GetCategory();
            foreach (var provider in Providers.Where(p => Filters.All(f => f.Filter(p.GetType().FullName, LogLevel.INFO, category, message))))
                provider.Info(category, message);
        }

        /// <summary>
        /// Logs the message as formatted string with info log level
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="arguments">The arguments to format the message.</param>
        public static void Info(string message, params object[] arguments)
        {
            Info(string.Format(message, arguments));
        }

        /// <summary>
        /// Logs the message with warning log level
        /// </summary>
        /// <param name="message">The message to be logged</param>
        public static void Warning(string message)
        {
            var category = GetCategory();
            foreach (var provider in Providers.Where(p => Filters.All(f => f.Filter(p.GetType().FullName, LogLevel.WARNING, category, message))))
                provider.Warning(category, message);
        }

        /// <summary>
        /// Logs the message as formatted string with warning log level
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="arguments">The arguments to format the message.</param>
        public static void Warning(string message, params object[] arguments)
        {
            Warning(string.Format(message, arguments));
        }

        /// <summary>
        /// Logs the message with error log level
        /// </summary>
        /// <param name="message">The message to be logged</param>
        public static void Error(string message)
        {
            var category = GetCategory();
            foreach (var provider in Providers.Where(p => Filters.All(f => f.Filter(p.GetType().FullName, LogLevel.ERROR, category, message))))
                provider.Error(category, message);
        }

        /// <summary>
        /// Logs the message as formatted string with error log level
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="arguments">The arguments to format the message.</param>
        public static void Error(string message, params object[] arguments)
        {
            Error(string.Format(message, arguments));
        }

        /// <summary>
        /// Handle the exception with the exception handler set in the 
        /// </summary>
        /// <param name="exception">The exception to be handled</param>
        public static void Handle(Exception exception)
        {
            _handler?.Handle(exception);
        }

        /// <summary>
        /// Handle an unhandled exception
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="args"><see cref="UnhandledExceptionEventArgs"/></param>
        public static void Handle(object sender, UnhandledExceptionEventArgs args)
        {
            _handler?.Handle(sender, args);
        }

        /// <summary>
        /// Handles the specified sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ThreadExceptionEventArgs"/> instance containing the event data.</param>
        public static void Handle(object sender, ThreadExceptionEventArgs args)
        {
            _handler?.Handle(sender, args);
        }

        #endregion

    }
}
