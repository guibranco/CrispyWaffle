using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CrispyWaffle.BackgroundJobs.Scheduling;

/// <summary>
/// CronJobScheduler is a lightweight helper that triggers an action on a CRON expression.
/// Note: to support CRON syntax you'd typically add a dependency such as NCrontab or Cronos.
/// This is a stub showing how to wire a hosted service cron runner.
/// </summary>
public abstract class CronJobService : BackgroundService
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CronJobService"/> class.
    /// </summary>
    /// <param name="logger">The logger used to report execution errors.</param>
    protected CronJobService(ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Example stub: run every minute
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExecuteOnce(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cron job execution error");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    /// <summary>
    /// Runs the work once. It is called repeatedly, with a one-minute wait after each run.
    /// </summary>
    /// <param name="stoppingToken">A token that is cancelled when the host is stopping.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected abstract Task ExecuteOnce(CancellationToken stoppingToken);
}
