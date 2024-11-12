using System;
using System.Threading;

namespace CrispyWaffle.Scheduler
{
    /// <summary>
    /// Represents a job runner that manages the execution of a job based on a specified schedule.
    /// This class checks if the current time matches the scheduled cron expression and executes the associated action in a separate thread.
    /// Implements the <see cref="IJobRunner"/> interface.
    /// </summary>
    /// <seealso cref="IJobRunner"/>
    public class JobRunner : IJobRunner
    {
        /// <summary>
        /// The synchronization object used to ensure thread safety when executing jobs.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// The scheduler responsible for determining when the job should be executed based on a cron expression.
        /// </summary>
        private readonly IScheduler _scheduler;

        /// <summary>
        /// The action to be executed when the job is triggered.
        /// </summary>
        private readonly ThreadStart _threadStart;

        /// <summary>
        /// The thread that runs the job.
        /// </summary>
        private Thread _thread;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobRunner"/> class.
        /// This constructor creates a job runner with a specified cron schedule and action to execute.
        /// </summary>
        /// <param name="schedule">A string representing the cron schedule for the job. It defines the times when the job should be executed.</param>
        /// <param name="threadStart">The action to execute when the job is triggered. It is represented as a <see cref="ThreadStart"/> delegate.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="schedule"/> is null or whitespace, or if the <paramref name="threadStart"/> is null.</exception>
        public JobRunner(string schedule, ThreadStart threadStart)
        {
            if (string.IsNullOrWhiteSpace(schedule))
            {
                throw new ArgumentNullException(schedule, "Schedule cannot be null or empty.");
            }

            _scheduler = new CronScheduler(schedule);
            _threadStart =
                threadStart
                ?? throw new ArgumentNullException(
                    nameof(threadStart),
                    "ThreadStart cannot be null."
                );
            _thread = new Thread(threadStart);
        }

        /// <summary>
        /// Executes the job if the current time matches the schedule.
        /// This method checks whether the job is already running or waiting, and only starts a new thread if the job is not already being executed.
        /// </summary>
        /// <param name="dateTime">The current date and time. The job will be executed only if the time matches the cron schedule.</param>
        private void ExecuteInternal(DateTime dateTime)
        {
            var status = _thread.ThreadState;

            // Check if the job should be executed based on the cron schedule
            if (!_scheduler.IsTime(dateTime))
            {
                return;
            }

            // If the job is already running or waiting, do not start it again
            if (status == ThreadState.Running || status == ThreadState.WaitSleepJoin)
            {
                return;
            }

            // Start a new thread to execute the job
            _thread = new Thread(_threadStart);
            _thread.Start();
        }

        /// <summary>
        /// Executes the job if the current time matches the schedule. This method ensures thread safety by locking the execution.
        /// </summary>
        /// <param name="dateTime">The current date and time. The job will be executed only if the time matches the cron schedule.</param>
        public void Execute(DateTime dateTime)
        {
            lock (_syncRoot)
            {
                ExecuteInternal(dateTime);
            }
        }
    }
}
