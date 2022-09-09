// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 07-29-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="EventsTests.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.Tests.Events
{
    using CrispyWaffle.Events;
    using System;
    using CrispyWaffle.Log;
    using Xunit;

    /// <summary>
    /// Class EventsTests.
    /// </summary>
    public class EventsTests
    {
        /// <summary>
        /// Defines the test method ValidateRaiseEvent.
        /// </summary>
        [Fact]
        public void ValidateRaiseEvent()
        {
            var @event = new TestObjects.TestDoneEvent(Guid.NewGuid(), @"Sample test");
            EventsConsumer.Raise(@event);
            Assert.Equal("Sample test",@event.Text);
        }

        /// <summary>
        /// Defines the test method ValidateRaiseEventWithException.
        /// </summary>
        [Fact]
        public void ValidateRaiseEventWithException()
        {
            var @event = new TestObjects.ExceptionEvent(Guid.NewGuid());
            var result = Assert.Throws<NotImplementedException>(() => EventsConsumer.Raise(@event));
            Assert.Equal(@event.Identifier.ToString(), result.Message);
        }
    }
}
