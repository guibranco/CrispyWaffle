using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrispyWaffle.BackgroundJobs.Monitoring;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CrispyWaffle.Tests.BackgroundJobs;

public class MonitoringMiddlewareTests
{
    [Fact]
    public async Task MetricsPathShouldWriteTheCountersAsJson()
    {
        // Arrange
        var metrics = new JobMetrics();
        metrics.Increment("processed");
        metrics.Increment("processed");
        using var body = new MemoryStream();
        var context = CreateContext("/jobs/metrics", body, CancellationToken.None);
        var nextCalled = false;
        var middleware = new MonitoringMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            metrics
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.ContentType.Should().Be("application/json");
        Encoding.UTF8.GetString(body.ToArray()).Should().Be("{\"processed\":2}");
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task MetricsPathShouldNotWriteWhenTheRequestWasAborted()
    {
        // Arrange
        var metrics = new JobMetrics();
        metrics.Increment("processed");
        using var body = new MemoryStream();
        var context = CreateContext("/jobs/metrics", body, new CancellationToken(true));
        var middleware = new MonitoringMiddleware(_ => Task.CompletedTask, metrics);

        // Act
        Func<Task> act = () => middleware.InvokeAsync(context);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        body.Length.Should().Be(0);
    }

    [Fact]
    public async Task OtherPathsShouldCallTheNextMiddleware()
    {
        // Arrange
        using var body = new MemoryStream();
        var context = CreateContext("/health", body, CancellationToken.None);
        var nextCalled = false;
        var middleware = new MonitoringMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            new JobMetrics()
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        body.Length.Should().Be(0);
    }

    private static DefaultHttpContext CreateContext(
        string path,
        Stream body,
        CancellationToken requestAborted
    )
    {
        var context = new DefaultHttpContext { RequestAborted = requestAborted };
        context.Request.Path = path;
        context.Response.Body = body;

        return context;
    }
}
