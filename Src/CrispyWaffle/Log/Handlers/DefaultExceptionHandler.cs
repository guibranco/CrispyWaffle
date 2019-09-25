namespace CrispyWaffle.Log.Handlers
{
    using Composition;
    using Extensions;
    using Providers;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Telemetry;

    /// <summary>
    /// Handle the exception and log it using the available log providers of the log consumer.
    /// </summary>
    /// <seealso cref="IExceptionHandler" />
    public class DefaultExceptionHandler : IExceptionHandler
    {
        #region Private fields 

        /// <summary>
        /// The additional providers
        /// </summary>
        private static readonly ICollection<Tuple<ILogProvider, ExceptionLogType>> AdditionalProviders;

        #endregion

        #region ~Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultExceptionHandler"/> class.
        /// </summary>
        static DefaultExceptionHandler()
        {
            AdditionalProviders = new List<Tuple<ILogProvider, ExceptionLogType>>
            {
                new Tuple<ILogProvider, ExceptionLogType>(ServiceLocator.Resolve<TextFileLogProvider>(),ExceptionLogType.MESSAGE)
            };
            try
            {
                if (Console.OpenStandardInput(1) != Stream.Null)
                    AdditionalProviders.Add(
                        new Tuple<ILogProvider, ExceptionLogType>(ServiceLocator.Resolve<ConsoleLogProvider>(),
                            ExceptionLogType.MESSAGE));
            }
            catch (Exception e)
            {
                LogConsumer.Handle(e);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <returns></returns>
        private string GetCategory()
        {
            var stack = new StackTrace();
            var counter = 1;
            while (true)
            {
				#warning Check IntegracaoService string above
                var method = stack.GetFrame(counter++).GetMethod();
                if (method == null)
                    return @"IntegracaoService";
                var ns = method.DeclaringType?.FullName;
                if (string.IsNullOrWhiteSpace(ns))
                    return method.Name;
                if (ns.StartsWith(@"IntegracaoService.Commons.Log"))
                    continue;
                if (ns.StartsWith(@"IntegracaoService.", StringExtensions.Comparison))
                    ns = ns.Substring(18);
                return ns;
            }
        }

        /// <summary>
        /// Handles the internal.
        /// </summary>
        /// <param name="exception">The exception.</param>
        private void HandleInternal(Exception exception)
        {
            var category = GetCategory();
            var exceptions = exception.ToQueue(out var types);
            foreach (var type in types)
                TelemetryAnalytics.TrackException(type);
            var messages = exceptions.GetMessages(category, AdditionalProviders.Where(p => p.Item2 == ExceptionLogType.MESSAGE).Select(p => p.Item1).ToList());
            foreach (var additionalProvider in AdditionalProviders.Where(p => p.Item2 == ExceptionLogType.FULL))
                additionalProvider.Item1.Error(category, messages);
        }

        #endregion

        #region Implementation of IExceptionHandler

        /// <summary>
        /// Logs a exception as ERROR level.
        /// Exception is logged generally with Message, StackTrace and Type.FullName, and it's inner exception until no one more is available,
        /// but this behavior depends on the Adapter implementation.
        /// </summary>
        /// <param name="exception">The exception to be logged</param>
        /// <remarks>Requires LogLevel.ERROR flag.</remarks>
        public void Handle(Exception exception)
        {
            HandleInternal(exception);
        }

        /// <summary>
        /// Cast <seealso cref="P:System.UnhandledExceptionEventArgs.ExceptionObject" /> as Exception
        /// and then call <seealso cref="M:IntegracaoService.Commons.Log.Handlers.IExceptionHandler.Handle(System.Exception)" />.
        /// This is the default behavior, each implementation can have it own behavior!
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="args">A instance of <seealso cref="T:System.UnhandledExceptionEventArgs" /></param>
        /// <remarks>Requires LogLevel.ERROR flag.</remarks>
        public void Handle(object sender, UnhandledExceptionEventArgs args)
        {
            HandleInternal((Exception)args.ExceptionObject);
        }

        /// <summary>
        /// Handles the specified sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ThreadExceptionEventArgs" /> instance containing the event data.</param>
        public void Handle(object sender, ThreadExceptionEventArgs args)
        {
            HandleInternal(args.Exception);
        }

        /// <summary>
        /// Adds the log provider.
        /// </summary>
        /// <typeparam name="TILogProvider">The type of the i log provider.</typeparam>
        /// <returns></returns>
        public ILogProvider AddLogProvider<TILogProvider>(ExceptionLogType type) where TILogProvider : ILogProvider
        {
            var provider = ServiceLocator.Resolve<TILogProvider>();
            AdditionalProviders.Add(new Tuple<ILogProvider, ExceptionLogType>(provider, type));
            return provider;
        }

        #endregion
    }
}
