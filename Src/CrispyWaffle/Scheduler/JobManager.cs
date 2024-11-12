using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace CrispyWaffle.Scheduler;

/// <summary>
/// Manages scheduled jobs by adding, starting, and stopping them.
/// This class uses a timer to periodically check for jobs that need to be executed based on their cron expressions.
/// It implements the <see cref="IJobManager"/> interface, allowing jobs to be scheduled and triggered based on defined cron expressions.
/// </summary>
/// <seealso cref="IJobManager"/>
public class JobManager : IJobManager
{
    /// <summary>
    /// The timer that triggers the job execution at regular intervals.
    /// The default interval is set to 30 seconds.
    /// </summary>
    private readonly Timer _timer = new(30000) { AutoReset = true };

    /// <summary>
    /// The list of job runners that will execute the jobs at the specified time.
    /// </summary>
    private readonly List<IJobRunner> _jobRunners = new();

    /// <summary>
    /// The last time the jobs were run. Used to prevent duplicate executions within the same minute.
    /// </summary>
    private DateTime _lastRun = DateTime.Now;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobManager"/> class.
    /// The constructor sets up the timer and subscribes to its <see cref="Timer.Elapsed"/> event.
    /// </summary>
    public JobManager() => _timer.Elapsed += TimerOnElapsed;

    /// <summary>
    /// Event handler for the timer's <see cref="Timer.Elapsed"/> event.
    /// This method checks whether the current minute is different from the last run minute.
    /// If the minute has changed, it triggers the execution of all scheduled jobs.
    /// </summary>
    /// <param name="sender">The source of the event (the timer).</param>
    /// <param name="e">The event data containing the time when the event was triggered.</param>
    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        // Prevent running jobs multiple times within the same minute
        if (_lastRun.Minute == DateTime.Now.Minute)
        {
            return;
        }

        _lastRun = DateTime.Now;

        // Execute all scheduled jobs
        foreach (var jobRunner in _jobRunners)
        {
            jobRunner.Execute(DateTime.Now);
        }
    }

    /// <summary>
    /// Adds a job to the scheduler with a specified cron expression and action.
    /// The cron expression defines the schedule for the job, and the action represents the code to be executed.
    /// </summary>
    /// <param name="cronExpression">A string representing the cron expression that defines the schedule for the job.
    /// It follows the standard cron syntax for minute, hour, day, month, and weekday.</param>
    /// <param name="action">A <see cref="ThreadStart"/> delegate that contains the code to be executed when the job is triggered.</param>
    /// <exception cref="System.NotImplementedException">Thrown if the method is not implemented.</exception>
    public void AddJob(string cronExpression, ThreadStart action) =>
        _jobRunners.Add(new JobRunner(cronExpression, action));

    /// <summary>
    /// Starts the job manager, enabling the periodic execution of scheduled jobs.
    /// This method starts the timer, which triggers job execution at regular intervals (e.g., every 30 seconds).
    /// </summary>
    /// <exception cref="System.NotImplementedException">Thrown if the method is not implemented.</exception>
    public void Start() => _timer.Start();

    /// <summary>
    /// Stops the job manager, halting the periodic execution of scheduled jobs.
    /// This method stops the timer, preventing further job executions until <see cref="Start"/> is called again.
    /// </summary>
    /// <exception cref="System.NotImplementedException">Thrown if the method is not implemented.</exception>
    public void Stop() => _timer.Stop();
}
