using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CrispyWaffle.BackgroundJobs.Abstractions;
using System.Text.Json;

namespace CrispyWaffle.BackgroundJobs.Core
{
    public class BackgroundWorker : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly BackgroundJobQueue? _queue;
        private readonly IJobStore? _store;
        private readonly IJobHandlerRegistry _registry;
        private readonly JobOptions _options;
        private readonly ILogger<BackgroundWorker> _logger;

        public BackgroundWorker(IServiceProvider provider, JobOptions options, ILogger<BackgroundWorker> logger)
        {
            _provider = provider;
            _options = options;
            _logger = logger;

            // These may or may not be registered depending on configuration; resolve lazily per scope
            _queue = provider.GetService<BackgroundJobQueue>();
            _store = provider.GetService<IJobStore>();
            _registry = provider.GetRequiredService<IJobHandlerRegistry>();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundWorker starting with {Workers} workers", _options.WorkerCount);

            var tasks = new Task[_options.WorkerCount];
            for (int i = 0; i < _options.WorkerCount; i++) tasks[i] = Task.Run(() => WorkerLoopAsync(i, stoppingToken), stoppingToken);

            return Task.WhenAll(tasks);
        }

        private async Task WorkerLoopAsync(int workerId, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker {WorkerId} started", workerId);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    JobEntity? job = null;

                    if (_store != null)
                    {
                        job = await _store.FetchNextAsync(stoppingToken);
                        if (job == null)
                        {
                            await Task.Delay(_options.PollingInterval, stoppingToken);
                            continue;
                        }
                    }
                    else if (_queue != null)
                    {
                        job = await _queue.DequeueAsync(stoppingToken);
                        if (job == null)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(200), stoppingToken);
                            continue;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No job queue nor store configured. Worker sleeping.");
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                        continue;
                    }

                    if (job == null) continue;

                    // If job has ScheduledAt in future, re-enqueue / re-schedule.
                    if (job.ScheduledAt.HasValue && job.ScheduledAt.Value > DateTimeOffset.UtcNow)
                    {
                        // For persistence, mark back to pending with same ScheduledAt; for in-memory, just re-enqueue.
                        if (_store != null)
                        {
                            // mark as Pending again (store.FetchNextAsync should have returned only due jobs, this is defensive)
                            await _store.MarkRetryAsync(job.Id, job.ScheduledAt, job.Attempt, stoppingToken);
                        }
                        else
                        {
                            _queue!.Enqueue(job);
                        }

                        continue;
                    }

                    // Process job with scope so handlers can have scoped services.
                    using var scope = _provider.CreateScope();
                    var scopedProvider = scope.ServiceProvider;

                    var success = false;
                    try
                    {
                        success = await ProcessJobAsync(job, scopedProvider, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        // If shutting down, put the job back to pending (or leave DB in Processing if store does locking; here we mark retry)
                        if (_store != null)
                        {
                            var next = DateTimeOffset.UtcNow.AddSeconds(5);
                            await _store.MarkRetryAsync(job.Id, next, job.Attempt, stoppingToken);
                        }
                        else
                        {
                            _queue!.Enqueue(job);
                        }
                        break; // exit loop to allow graceful shutdown
                    }

                    if (!success)
                    {
                        _logger.LogWarning("Job {JobId} failed after processing attempt {Attempt}", job.Id, job.Attempt);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Worker loop encountered an unexpected error");
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }

            _logger.LogInformation("Worker {WorkerId} stopping", workerId);
        }

        private async Task<bool> ProcessJobAsync(JobEntity job, IServiceProvider scopedProvider, CancellationToken cancellationToken)
        {
            try
            {
                if (!_registry.TryGet(job.HandlerName, out var handlerType, out var dataType))
                {
                    var msg = $"No handler registered for '{job.HandlerName}'";
                    _logger.LogError(msg);
                    if (_store != null) await _store.MarkFailedAsync(job.Id, msg, cancellationToken);
                    return false;
                }

                // Deserialize payload into dataType
                object? data = null;
                if (!string.IsNullOrWhiteSpace(job.Payload))
                {
                    data = JsonSerializer.Deserialize(job.Payload, dataType ?? typeof(object));
                }

                // Resolve handler instance
                var handler = scopedProvider.GetService(handlerType!);
                if (handler == null)
                {
                    var msg = $"Handler type {handlerType} not resolved from DI";
                    _logger.LogError(msg);
                    if (_store != null) await _store.MarkFailedAsync(job.Id, msg, cancellationToken);
                    return false;
                }

                // Invoke handler using reflection and await the Task<JobResult>
                var result = await InvokeHandleAsync(handler, data, cancellationToken);

                if (result.Success)
                {
                    if (_store != null) await _store.MarkCompletedAsync(job.Id, cancellationToken);
                    return true;
                }

                // handle retry
                job.Attempt++;
                var shouldRetry = result.Retry && job.Attempt < job.MaxAttempt;
                if (shouldRetry)
                {
                    var backoffSeconds = ComputeBackoffSeconds(job.Attempt);
                    var nextAttempt = DateTimeOffset.UtcNow.AddSeconds(backoffSeconds);
                    _logger.LogInformation("Scheduling retry for job {JobId} after {Delay}s (attempt {Attempt}/{MaxAttempt})", job.Id, backoffSeconds, job.Attempt, job.MaxAttempt);

                    if (_store != null)
                    {
                        await _store.MarkRetryAsync(job.Id, nextAttempt, job.Attempt, cancellationToken);
                    }
                    else
                    {
                        job.ScheduledAt = nextAttempt;
                        _queue!.Enqueue(job);
                    }
                }
                else
                {
                    // exhaust
                    var err = result.ErrorMessage ?? "Job failed";
                    if (_store != null) await _store.MarkFailedAsync(job.Id, err, cancellationToken);
                    else _logger.LogError("Job {JobId} failed and will not be retried: {Error}", job.Id, err);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when processing job {JobId}", job.Id);
                if (_store != null)
                {
                    // schedule retry with backoff
                    job.Attempt++;
                    var nextAttempt = DateTimeOffset.UtcNow.AddSeconds(ComputeBackoffSeconds(job.Attempt));
                    await _store.MarkRetryAsync(job.Id, nextAttempt, job.Attempt, cancellationToken);
                }
                else
                {
                    job.Attempt++;
                    job.ScheduledAt = DateTimeOffset.UtcNow.AddSeconds(ComputeBackoffSeconds(job.Attempt));
                    _queue!.Enqueue(job);
                }

                return false;
            }
        }

        private static int ComputeBackoffSeconds(int attempt)
        {
            // exponential backoff with cap
            var seconds = (int)Math.Pow(2, Math.Min(attempt, 10));
            return Math.Min(seconds, 300);
        }

        private static async Task<JobResult> InvokeHandleAsync(object handler, object? data, CancellationToken cancellationToken)
        {
            // Find HandleAsync method (TData, CancellationToken)
            var method = handler.GetType().GetMethod("HandleAsync");
            if (method == null) throw new InvalidOperationException("Handler does not implement HandleAsync");

            var parameters = method.GetParameters();
            object? invokeArg1 = null;
            object? invokeArg2 = null;
            if (parameters.Length == 2)
            {
                invokeArg1 = data;
                invokeArg2 = cancellationToken;
            }
            else if (parameters.Length == 1)
            {
                invokeArg1 = data ?? cancellationToken;
                if (invokeArg1 is CancellationToken)
                {
                    invokeArg2 = null;
                }
            }

            var taskObj = method.Invoke(handler, invokeArg2 == null ? new[] { invokeArg1 } : new[] { invokeArg1, invokeArg2 });
            if (taskObj == null) throw new InvalidOperationException("Handler returned null instead of Task<JobResult>");

            // support Task<JobResult> and Task
            if (taskObj is Task<JobResult> typed)
            {
                return await typed;
            }

            if (taskObj is Task task)
            {
                await task;
                // attempt to read Result property (in case of Task<T>)
                var resultProperty = task.GetType().GetProperty("Result");
                if (resultProperty != null)
                {
                    var res = resultProperty.GetValue(task);
                    if (res is JobResult jr) return jr;
                }

                // else assume success
                return JobResult.Ok();
            }

            throw new InvalidOperationException("Handler.HandleAsync returned unsupported type");
        }
    }
}
