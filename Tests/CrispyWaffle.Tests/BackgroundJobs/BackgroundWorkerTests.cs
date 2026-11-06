using System;
using System.Threading;
using System.Threading.Tasks;
using CrispyWaffle.BackgroundJobs.Abstractions;
using CrispyWaffle.BackgroundJobs.Core;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CrispyWaffle.Tests.BackgroundJobs;

public class BackgroundWorkerTests
{
    private const string HandlerName = "stub";

    private enum OutcomeKind
    {
        Completed,
        Failed,
        Retry,
    }

    [Fact]
    public async Task SuccessfulHandlerShouldMarkTheJobAsCompleted()
    {
        // Arrange
        var handler = new StubHandler(() => Task.FromResult(JobResult.Ok()));

        // Act
        var outcome = await RunSingleJobAsync(handler, CreateJob());

        // Assert
        outcome.Kind.Should().Be(OutcomeKind.Completed);
        handler.ReceivedData.Should().Be("payload");
    }

    [Fact]
    public async Task RetryableFailureWithAttemptsLeftShouldScheduleARetry()
    {
        // Arrange
        var handler = new StubHandler(() => Task.FromResult(JobResult.Fail("boom", true)));
        var before = DateTimeOffset.UtcNow;

        // Act
        var outcome = await RunSingleJobAsync(handler, CreateJob());
        var after = DateTimeOffset.UtcNow;

        // Assert
        outcome.Kind.Should().Be(OutcomeKind.Retry);
        outcome.AttemptCount.Should().Be(1);

        // The first retry waits two seconds.
        outcome
            .NextAttemptAt.Should()
            .BeOnOrAfter(before.AddSeconds(2))
            .And.BeOnOrBefore(after.AddSeconds(2));
    }

    [Fact]
    public async Task RetryableFailureOnTheLastAttemptShouldMarkTheJobAsFailed()
    {
        // Arrange
        var handler = new StubHandler(() => Task.FromResult(JobResult.Fail("boom", true)));
        var job = CreateJob();
        job.Attempt = 2;
        job.MaxAttempt = 3;

        // Act
        var outcome = await RunSingleJobAsync(handler, job);

        // Assert
        outcome.Kind.Should().Be(OutcomeKind.Failed);
        outcome.Error.Should().Be("boom");
    }

    [Fact]
    public async Task NonRetryableFailureShouldMarkTheJobAsFailed()
    {
        // Arrange
        var handler = new StubHandler(() => Task.FromResult(JobResult.Fail("boom")));

        // Act
        var outcome = await RunSingleJobAsync(handler, CreateJob());

        // Assert
        outcome.Kind.Should().Be(OutcomeKind.Failed);
        outcome.Error.Should().Be("boom");
    }

    [Fact]
    public async Task HandlerReturningNoTaskShouldScheduleARetry()
    {
        // Arrange
        var handler = new StubHandler(() => null);

        // Act
        var outcome = await RunSingleJobAsync(handler, CreateJob());

        // Assert
        outcome.Kind.Should().Be(OutcomeKind.Retry);
        outcome.AttemptCount.Should().Be(1);
    }

    [Fact]
    public async Task UnknownHandlerShouldMarkTheJobAsFailed()
    {
        // Arrange
        var handler = new StubHandler(() => Task.FromResult(JobResult.Ok()));
        var job = CreateJob();
        job.HandlerName = "missing";

        // Act
        var outcome = await RunSingleJobAsync(handler, job);

        // Assert
        outcome.Kind.Should().Be(OutcomeKind.Failed);
        outcome.Error.Should().Be("No handler registered for 'missing'");
        handler.ReceivedData.Should().BeNull();
    }

    private static JobEntity CreateJob() =>
        new() { HandlerName = HandlerName, Payload = "\"payload\"" };

    private static async Task<JobOutcome> RunSingleJobAsync(StubHandler handler, JobEntity job)
    {
        var store = new SingleJobStore(job);
        var registry = new JobHandlerRegistry();
        registry.Register<StubHandler, string>(HandlerName);

        var services = new ServiceCollection();
        services.AddSingleton<IJobStore>(store);
        services.AddSingleton<IJobHandlerRegistry>(registry);
        services.AddSingleton(handler);

        using var provider = services.BuildServiceProvider();
        var options = new JobOptions
        {
            WorkerCount = 1,
            PollingInterval = TimeSpan.FromMilliseconds(10),
        };
        using var worker = new BackgroundWorker(
            provider,
            options,
            NullLogger<BackgroundWorker>.Instance
        );

        await worker.StartAsync(CancellationToken.None);
        try
        {
            return await store.Outcome.WaitAsync(TimeSpan.FromSeconds(30));
        }
        finally
        {
            await worker.StopAsync(CancellationToken.None);
        }
    }

    private sealed record JobOutcome(
        OutcomeKind Kind,
        int AttemptCount = 0,
        DateTimeOffset? NextAttemptAt = null,
        string Error = null
    );

    private sealed class StubHandler : IBackgroundJobHandler<string>
    {
        private readonly Func<Task<JobResult>> _result;

        public StubHandler(Func<Task<JobResult>> result) => _result = result;

        public string ReceivedData { get; private set; }

        public Task<JobResult> HandleAsync(string data, CancellationToken cancellationToken)
        {
            ReceivedData = data;
            return _result();
        }
    }

    /// <summary>
    /// A store that hands out one job and reports what the worker did with it.
    /// </summary>
    private sealed class SingleJobStore : IJobStore
    {
        private readonly TaskCompletionSource<JobOutcome> _outcome = new(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        private JobEntity _job;

        public SingleJobStore(JobEntity job) => _job = job;

        public Task<JobOutcome> Outcome => _outcome.Task;

        public Task SaveAsync(JobEntity job, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<JobEntity> FetchNextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Interlocked.Exchange(ref _job, null));

        public Task MarkCompletedAsync(Guid jobId, CancellationToken cancellationToken = default)
        {
            _outcome.TrySetResult(new JobOutcome(OutcomeKind.Completed));
            return Task.CompletedTask;
        }

        public Task MarkFailedAsync(
            Guid jobId,
            string error,
            CancellationToken cancellationToken = default
        )
        {
            _outcome.TrySetResult(new JobOutcome(OutcomeKind.Failed, Error: error));
            return Task.CompletedTask;
        }

        public Task MarkRetryAsync(
            Guid jobId,
            DateTimeOffset? nextAttemptAt,
            int attemptCount,
            CancellationToken cancellationToken = default
        )
        {
            _outcome.TrySetResult(new JobOutcome(OutcomeKind.Retry, attemptCount, nextAttemptAt));
            return Task.CompletedTask;
        }
    }
}
