﻿using System;
using System.Threading;
using CrispyWaffle.Composition;
using CrispyWaffle.Log.Providers;

namespace CrispyWaffle.Log.Handlers
{
    /// <summary>
    /// The null exception handler.
    /// </summary>
    /// <seealso cref="IExceptionHandler" />
    public sealed class NullExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// Logs a exception as ERROR level.
        /// Exception is logged generally with Message, StackTrace and Type.FullName, and it's inner exception until no one more is available,
        /// but this behavior depends on the Adapter implementation.
        /// </summary>
        /// <param name="exception">The exception to be logged.</param>
        /// <remarks>Requires LogLevel.ERROR flag.</remarks>
        public void Handle(Exception exception) { }

        /// <summary>
        /// Cast <seealso cref="UnhandledExceptionEventArgs.ExceptionObject" /> as Exception
        /// and then call <seealso cref="IExceptionHandler.Handle(Exception)" />.
        /// This is the default behavior, each implementation can have it own behavior!
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="args">A instance of <seealso cref="UnhandledExceptionEventArgs" /></param>
        /// <remarks>Requires LogLevel.ERROR flag.</remarks>
        public void Handle(object sender, UnhandledExceptionEventArgs args) { }

        /// <summary>
        /// Handles the specified sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ThreadExceptionEventArgs" /> instance containing the event data.</param>
        public void Handle(object sender, ThreadExceptionEventArgs args) { }

        /// <summary>
        /// Adds the log provider.
        /// </summary>
        /// <typeparam name="TLogProvider">The type of the i log provider.</typeparam>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public ILogProvider AddLogProvider<TLogProvider>(ExceptionLogType type)
            where TLogProvider : ILogProvider => ServiceLocator.Resolve<TLogProvider>();
    }
}
