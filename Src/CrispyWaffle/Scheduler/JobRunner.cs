// ***********************************************************************
// Assembly         : CrispyWaffle
// Author           : Guilherme Branco Stracini
// Created          : 09-05-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="JobRunner.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.Scheduler
{
    using System;
    using System.Threading;

    /// <summary>
    /// Class JobRunner.
    /// Implements the <see cref="CrispyWaffle.Scheduler.IJobRunner" />
    /// </summary>
    /// <seealso cref="CrispyWaffle.Scheduler.IJobRunner" />
    public class JobRunner : IJobRunner
    {
        /// <summary>
        /// The synchronize root
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// The scheduler
        /// </summary>
        private readonly IScheduler _scheduler;

        /// <summary>
        /// The thread start
        /// </summary>
        private readonly ThreadStart _threadStart;

        /// <summary>
        /// The thread
        /// </summary>
        private Thread _thread;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobRunner"/> class.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <param name="threadStart">The thread start.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentNullException">threadStart</exception>
        public JobRunner(string schedule, ThreadStart threadStart)
        {
            if (string.IsNullOrWhiteSpace(schedule))
            {
                throw new ArgumentNullException(schedule);
            }

            _scheduler = new CronScheduler(schedule);
            _threadStart = threadStart ?? throw new ArgumentNullException(nameof(threadStart));
            _thread = new Thread(threadStart);
        }

        /// <summary>
        /// Executes the internal.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        private void ExecuteInternal(DateTime dateTime)
        {
            var status = _thread.ThreadState;

            if (!_scheduler.IsTime(dateTime))
            {
                return;
            }

            if (status == ThreadState.Running || status == ThreadState.WaitSleepJoin)
            {
                return;
            }

            _thread = new Thread(_threadStart);
            _thread.Start();
        }

        #region Implementation of IJobRunner

        /// <summary>
        /// Executes the specified date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        public void Execute(DateTime dateTime)
        {
            lock (_syncRoot)
            {
                ExecuteInternal(dateTime);
            }
        }

        #endregion
    }
}
