﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CrispyWaffle.Composition;
using CrispyWaffle.Extensions;
using CrispyWaffle.Log.Handlers;
using CrispyWaffle.Log.Providers;
using ElmahCore;

namespace CrispyWaffle.Elmah;

/// <summary>
/// The Elmah exception handler class.
/// </summary>
/// <seealso cref="IExceptionHandler" />
public sealed class ElmahExceptionHandler : IExceptionHandler
{
    /// <summary>
    /// The additional providers.
    /// </summary>
    private static readonly IList<Tuple<ILogProvider, ExceptionLogType>> _additionalProviders =
        new List<Tuple<ILogProvider, ExceptionLogType>>();

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
            var method = stack.GetFrame(counter++)?.GetMethod();
            if (method == null)
            {
                return "CrispyWaffle";
            }

            var ns = method.DeclaringType?.FullName;
            if (string.IsNullOrWhiteSpace(ns))
            {
                return method.Name;
            }

            if (ns.StartsWith("CrispyWaffle", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (ns.StartsWith("CrispyWaffle.", StringComparison.InvariantCultureIgnoreCase))
            {
                ns = ns.Substring(18);
            }

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

        var exceptions = exception.ToQueue(out _);

        var messages = exceptions.GetMessages(
            category,
            _additionalProviders
                .Where(p => p.Item2 == ExceptionLogType.Message)
                .Select(p => p.Item1)
                .ToList()
        );

        foreach (
            var additionalProvider in _additionalProviders.Where(p =>
                p.Item2 == ExceptionLogType.Full
            )
        )
        {
            additionalProvider.Item1.Error(category, messages);
        }

        ElmahExtensions.RaiseError(exception);
    }

    /// <summary>
    /// Logs an exception as ERROR level.
    /// Exception is logged generally with Message, StackTrace, and Type.FullName and its inner exception until no one more is available, but this behavior depends on the Adapter implementation.
    /// </summary>
    /// <param name="exception">The exception to be logged.</param>
    /// <remarks>Requires LogLevel.ERROR flag.</remarks>
    public void Handle(Exception exception) => HandleInternal(exception);

    /// <summary>
    /// Cast <seealso cref="UnhandledExceptionEventArgs.ExceptionObject" /> as Exception and then call <seealso cref="IExceptionHandler.Handle(Exception)" />.
    /// This is the default behavior; each implementation can have its behavior.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">An instance of <seealso cref="UnhandledExceptionEventArgs" />.</param>
    /// <remarks>Requires LogLevel.ERROR flag.</remarks>
    public void Handle(object sender, UnhandledExceptionEventArgs args) =>
        HandleInternal((Exception)args.ExceptionObject);

    /// <summary>
    /// Handles the specified sender.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="ThreadExceptionEventArgs" /> instance containing the event data.</param>
    public void Handle(object sender, ThreadExceptionEventArgs args) =>
        HandleInternal(args.Exception);

    /// <summary>
    /// Adds the log provider.
    /// </summary>
    /// <typeparam name="TLogProvider">The type of the log provider.</typeparam>
    /// <param name="type">The type.</param>
    /// <returns>ILogProvider.</returns>
    public ILogProvider AddLogProvider<TLogProvider>(ExceptionLogType type)
        where TLogProvider : ILogProvider
    {
        var provider = ServiceLocator.Resolve<TLogProvider>();

        _additionalProviders.Add(new Tuple<ILogProvider, ExceptionLogType>(provider, type));

        return provider;
    }
}
