// ***********************************************************************
// Assembly         : CrispyWaffle
// Author           : Guilherme Branco Stracini
// Created          : 09-05-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="IJobManager.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Threading;

namespace CrispyWaffle.Scheduler
{
    /// <summary>
    /// Interface IJobManager
    /// </summary>
    public interface IJobManager
    {
        /// <summary>
        /// Adds the job.
        /// </summary>
        /// <param name="cronExpression">The cron expression.</param>
        /// <param name="action">The action.</param>
        void AddJob(string cronExpression, ThreadStart action);

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();
    }
}
