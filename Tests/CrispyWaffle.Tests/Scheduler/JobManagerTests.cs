﻿// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 09-05-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="JobManagerTests.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.Tests.Scheduler
{
    using CrispyWaffle.Scheduler;
    using System.Threading;
    using Xunit;

    /// <summary>
    /// Class JobManagerTests.
    /// </summary>
    [Collection("JobManager")]
    public class JobManagerTests
    {
        /// <summary>
        /// Defines the test method ValidateJobManager.
        /// </summary>
        [Fact]
        public void ValidateJobManager()
        {
            var manager = new JobManager();

            var sampler = new TestObjects();

            var syncRoot = new object();

            manager.AddJob("*", () => { lock (syncRoot) { sampler.Counter++; } });
            manager.AddJob("*", () => { lock (syncRoot) { sampler.Counter++; } });
            manager.AddJob("*", () => { lock (syncRoot) { sampler.Counter++; } });
            manager.AddJob("*", () => { lock (syncRoot) { sampler.Counter++; } });
            manager.AddJob("*", () => { lock (syncRoot) { sampler.Counter++; } });

            manager.Start();

            Thread.Sleep(70 * 1000);

            manager.Stop();

            Assert.Equal(5, sampler.Counter);

        }
    }
}
