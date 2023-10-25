// ***********************************************************************
// Assembly         : CrispyWaffle
// Author           : Guilherme Branco Stracini
// Created          : 09-05-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="JobManager.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace CrispyWaffle.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Timers;
    using Timer = System.Timers.Timer;

    /// <summary>
    /// Class JobManager.
    /// Implements the <see cref="CrispyWaffle.Scheduler.IJobManager" />
    /// </summary>
    /// <seealso cref="CrispyWaffle.Scheduler.IJobManager" />
    public class JobManager : IJobManager
    {
        /// <summary>
        /// The timer
        /// </summary>
        private readonly Timer _timer = new(30000)
{
    AutoReset = true
};

        /// <summary>
        /// The job runners
        /// </summary>
        private readonly List<IJobRunner> _jobRunners = new List<IJobRunner>();

        private DateTime _lastRun = DateTime.Now;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobManager"/> class.
        /// </summary>
        public JobManager() => _timer.Elapsed += TimerOnElapsed;

        /// <summary>
        /// Timers the on elapsed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs" /> instance containing the event data.</param>
        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (_lastRun.Minute == DateTime.Now.Minute)
            {
                return;
            }

            _lastRun = DateTime.Now;

            foreach (var jobRunner in _jobRunners)
            {
                jobRunner.Execute(DateTime.Now);
            }
        }

        #region Implementation of IJobManager

        /// <summary>
        /// Adds the job.
        /// </summary>
        /// <param name="cronExpression">The cron expression.</param>
        /// <param name="action">The action.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void AddJob(string cronExpression, ThreadStart action) =>
            _jobRunners.Add(new JobRunner(cronExpression, action));

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Start() => _timer.Start();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Stop() => _timer.Stop();

        #endregion
    }
}
