// ***********************************************************************
// Assembly         : CrispyWaffle
// Author           : Guilherme Branco Stracini
// Created          : 07-29-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 07-29-2020
// ***********************************************************************
// <copyright file="DefaultExceptionHandler.cs" company="Guilherme Branco Stracini ME">
//     © 2020 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Reflection;

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
        private static readonly ICollection<Tuple<ILogProvider, ExceptionLogType>> AdditionalProviders = AdditionalProviders = new List<Tuple<ILogProvider, ExceptionLogType>>();

        #endregion

        #region Private methods

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <returns>System.String.</returns>
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
        /// Gets the namespace.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="category">The category.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool GetNamespace(MethodBase method, out string category)
        {
            category = string.Empty;
            var ns = method.DeclaringType?.FullName;

            if (string.IsNullOrWhiteSpace(ns))
            {
                category = method.Name;
                return true;
            }

            if (ns.StartsWith(@"CrispyWaffle.Log"))
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
        /// Handles the internal.
        /// </summary>
        /// <param name="exception">The exception.</param>
        private static void HandleInternal(Exception exception)
        {
            var category = GetCategory();

            var exceptions = exception.ToQueue(out var types);

            foreach (var type in types)
            {
                TelemetryAnalytics.TrackException(type);
            }

            var messages = exceptions.GetMessages(category, AdditionalProviders.Where(p => p.Item2 == ExceptionLogType.MESSAGE).Select(p => p.Item1).ToList());

            foreach (var additionalProvider in AdditionalProviders.Where(p => p.Item2 == ExceptionLogType.FULL))
            {
                additionalProvider.Item1.Error(category, messages);
            }
        }

        #endregion

        #region Implementation of IExceptionHandler

        /// <summary>
        /// Logs a exception as ERROR level.
        /// Exception is logged generally with Message, StackTrace and Type.FullName, and it's inner exception until no one more is available,
        /// but this behavior depends on the Adapter implementation.
        /// </summary>
        /// <param name="exception">The exception to be logged.</param>
        /// <remarks>Requires LogLevel.ERROR flag.</remarks>
        public void Handle(Exception exception)
        {
            HandleInternal(exception);
        }


        /// <summary>
        /// Cast <seealso cref="UnhandledExceptionEventArgs.ExceptionObject" /> as Exception
        /// and then call <seealso cref="Handle(Exception)" />.
        /// This is the default behavior, each implementation can have it own behavior!
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="args">A instance of <seealso cref="UnhandledExceptionEventArgs" /></param>
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
        /// <param name="type">The type.</param>
        /// <returns>ILogProvider.</returns>
        public ILogProvider AddLogProvider<TILogProvider>(ExceptionLogType type) where TILogProvider : ILogProvider
        {
            var provider = ServiceLocator.Resolve<TILogProvider>();

            AdditionalProviders.Add(new Tuple<ILogProvider, ExceptionLogType>(provider, type));

            return provider;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Tries the add console log provider.
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
                    AdditionalProviders.Add(new Tuple<ILogProvider, ExceptionLogType>(instance, ExceptionLogType.MESSAGE));
                }
            }
            catch (Exception)
            {
                //ignore handling
            }
        }

        /// <summary>
        /// Tries the add text file log provider.
        /// </summary>
        public static void TryAddTextFileLogProvider()
        {
            try
            {

                var instance = ServiceLocator.TryResolve<TextFileLogProvider>();

                if (instance != null)
                {
                    AdditionalProviders.Add(new Tuple<ILogProvider, ExceptionLogType>(instance, ExceptionLogType.FULL));
                }
            }
            catch (Exception)
            {
                //ignore handling
            }
        }

        #endregion
    }
}
