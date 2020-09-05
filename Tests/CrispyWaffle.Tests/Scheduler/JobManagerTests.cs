// ***********************************************************************
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
using CrispyWaffle.Scheduler;
using System.Threading;
using Xunit;

namespace CrispyWaffle.Tests.Scheduler
{
    /// <summary>
    /// Class JobManagerTests.
    /// </summary>
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

            Thread.Sleep(59 * 1000);

            manager.Stop();

            Assert.Equal(5, sampler.Counter);

        }
    }
}
