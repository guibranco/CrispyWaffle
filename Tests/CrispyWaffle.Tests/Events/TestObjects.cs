// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 07-29-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="TestObjects.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Threading.Tasks;
using CrispyWaffle.Events;
using CrispyWaffle.Log;

namespace CrispyWaffle.Tests.Events;

/// <summary>
/// Class TestObjects.
/// </summary>
internal class TestObjects
{
    /// <summary>
    /// The test done event class.
    /// </summary>
    /// <seealso cref="IEvent" />
    internal sealed class TestDoneEvent : IEvent
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public Guid Identifier { get; }

        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text { get; }

        /// <summary>
        /// Gets the created date t IME.
        /// </summary>
        /// <value>The created date t IME.</value>
        public DateTime CreatedDateTIme { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDoneEvent" /> class.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="text">The text.</param>
        public TestDoneEvent(Guid identifier, string text)
        {
            Identifier = identifier;
            Text = text;
            CreatedDateTIme = DateTime.Now;
        }
    }

    /// <summary>
    /// The log done event class
    /// </summary>
    /// <seealso cref="IEventHandler{TEvent}" />
    public sealed class LogDoneEvent : IEventHandler<TestDoneEvent>, IEventHandlerAsync<TestDoneEvent>
    {
        /// <summary>
        /// Handles the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void Handle(TestDoneEvent args)
        {
            LogConsumer.Info(
                @"Sample done action handled: {0} - {1} - {2:dd/MM/yyyy HH:mm:ss}",
                args.Identifier,
                args.Text,
                args.CreatedDateTIme
            );
        }

        /// <summary>
        /// Handles the specified arguments asynchronously.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public async Task HandleAsync(TestDoneEvent args)
        {
            await Task.Run(() => LogConsumer.Info(
                @"Sample done action handled: {0} - {1} - {2:dd/MM/yyyy HH:mm:ss}",
                args.Identifier,
                args.Text,
                args.CreatedDateTIme));
        }
    }

    ///// <summary>
    ///// The mail done event class.
    ///// </summary>
    ///// <seealso cref="IEventHandler{TestDoneEvent}" />

    //public sealed class MailDoneEvent : IEventHandler<TestDoneEvent>
    //{
    //    #region Implementation of IEventHandler<SampleDoneAction>

    //    /// <summary>
    //    /// Handles the specified arguments.
    //    /// </summary>
    //    /// <param name="args">The arguments.</param>
    //    public void Handle(TestDoneEvent args)
    //    {
    //        NotificationConsumer.Notify(new ExceptionNotification
    //        {
    //            Date = DateTime.Now,
    //            Operation = Operation.NONE,
    //            Messages = new[]
    //                                        {
    //                                            @"Test"
    //                                        }
    //        });
    //    }

    //    #endregion
    //}

    /// <summary>
    /// The exception event class.
    /// </summary>
    /// <seealso cref="IEvent" />
    internal sealed class ExceptionEvent : IEvent
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public Guid Identifier { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEvent" /> class.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        public ExceptionEvent(Guid identifier)
        {
            Identifier = identifier;
        }
    }

    /// <summary>
    /// The throw exception event
    /// </summary>
    /// <seealso cref="IEventHandler{ExceptionEvent}" />
    public sealed class ThrowExceptionEvent : IEventHandler<ExceptionEvent>
    {
        /// <summary>
        /// Handles the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Handle(ExceptionEvent args)
        {
            throw new NotImplementedException(args.Identifier.ToString());
        }
    }
}
