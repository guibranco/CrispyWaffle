// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 09-05-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="JobRunnerTests.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Threading;
using CrispyWaffle.Scheduler;
using Xunit;

namespace CrispyWaffle.Tests.Scheduler
{
    /// <summary>
    /// Class JobRunnerTests.
    /// </summary>
    [Collection("JobRunner")]
    public class JobRunnerTests
    {
        /// <summary>
        /// Defines the test method ValidateJobRunnerEmptyScheduler.
        /// </summary>
        [Fact]
        public void ValidateJobRunnerEmptyScheduler()
        {
            Assert.Throws<ArgumentNullException>(() => new JobRunner(string.Empty, null));
        }

        /// <summary>
        /// Defines the test method ValidateJobRunner.
        /// </summary>
        [Fact]
        public void ValidateJobRunner()
        {
            var sampler = new TestObjects();

            var runner = new JobRunner("*", () => sampler.Counter++);

            for (var i = 0; i < 10; i++)
            {
                runner.Execute(DateTime.Now);
                Thread.Sleep(500);
            }

            Thread.Sleep(1000);

            Assert.Equal(10, sampler.Counter);
        }

        /// <summary>
        /// Defines the test method ValidateOutOfScheduler.
        /// </summary>
        [Fact]
        public void ValidateOutOfScheduler()
        {
            var sampler = new TestObjects();

            var runner = new JobRunner("*/5", () => sampler.Counter++);

            var date = DateTime.Parse("00:00:00");

            for (var i = 0; i <= 10; i++)
            {
                runner.Execute(date);
                Thread.Sleep(500);
                date = date.AddMinutes(1);
            }

            Thread.Sleep(1000);

            Assert.Equal(3, sampler.Counter);
        }

        /// <summary>
        /// Defines the test method ValidateConcurrency.
        /// </summary>
        [Fact]
        public void ValidateConcurrency()
        {
            const int sleepMilliseconds = 2000;

            var sampler = new TestObjects();

            var runner = new JobRunner(
                "*",
                () =>
                {
                    sampler.Counter++;
                    Thread.Sleep(sleepMilliseconds);
                }
            );

            runner.Execute(DateTime.Now);
            runner.Execute(DateTime.Now);

            Thread.Sleep(sleepMilliseconds * 2);

            runner.Execute(DateTime.Now);
            runner.Execute(DateTime.Now);

            Thread.Sleep(sleepMilliseconds);

            Assert.Equal(2, sampler.Counter);
        }
    }
}
