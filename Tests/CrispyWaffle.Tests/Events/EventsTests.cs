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
using CrispyWaffle.Events;
using System;
using Xunit;
using Xunit.Abstractions;

namespace CrispyWaffle.Tests.Events
{
    /// <summary>
    /// Class EventsTests.
    /// </summary>
    [Collection("Logged collection")]
    public class EventsTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventsTests" /> class.
        /// </summary>
        /// <param name="testOutputHelper">The test output helper.</param>
        /// <param name="fixture">The fixture.</param>
        public EventsTests(ITestOutputHelper testOutputHelper, BootstrapFixture fixture) =>
            fixture.SetLogProvider(testOutputHelper);

        /// <summary>
        /// Defines the test method ValidateRaiseEvent.
        /// </summary>
        [Fact]
        public void ValidateRaiseEvent()
        {
            var @event = new TestObjects.TestDoneEvent(Guid.NewGuid(), @"Sample test");
            EventsConsumer.Raise(@event);
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
