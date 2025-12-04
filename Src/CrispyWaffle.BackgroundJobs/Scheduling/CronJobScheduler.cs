using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CrispyWaffle.BackgroundJobs.Scheduling
{
    /// <summary>
    /// CronJobScheduler is a lightweight helper that triggers an action on a CRON expression.
    /// Note: to support CRON syntax you'd typically add a dependency such as NCrontab or Cronos.
    /// This is a stub showing how to wire a hosted service cron runner.
    /// </summary>
    public abstract class CronJobService : BackgroundService
    {
        private readonly ILogger _logger;

        protected CronJobService(ILogger logger)
        {
            _logger = logger;
        }

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

        protected abstract Task ExecuteOnce(CancellationToken stoppingToken);
    }
}