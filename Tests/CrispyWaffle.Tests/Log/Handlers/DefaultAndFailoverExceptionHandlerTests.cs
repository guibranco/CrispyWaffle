using System;
using System.Collections.Generic;
using System.Linq;
using CrispyWaffle.Composition;
using CrispyWaffle.Extensions;
using CrispyWaffle.Log;
using CrispyWaffle.Log.Adapters;
using CrispyWaffle.Log.Handlers;
using CrispyWaffle.Log.Providers;
using CrispyWaffle.Telemetry;
using NSubstitute;
using Xunit;

namespace CrispyWaffle.Tests.Log.Handlers;

public sealed class DefaultAndFailoverExceptionHandlerTests
{
    [Fact]
    public void ToQueueIncludesOutermostAndInnerExceptionTypes()
    {
        var exception = new InvalidOperationException("outer", new ArgumentException("inner"));

        _ = exception.ToQueue(out var types);

        Assert.Equal([typeof(InvalidOperationException), typeof(ArgumentException)], types);
    }

    [Fact]
    public void DefaultExceptionHandlerTracksOutermostAndInnerExceptionTypes()
    {
        var client = Substitute.For<ITelemetryClient>();
        TelemetryAnalytics.AddClient(client);
        var handler = new DefaultExceptionHandler();

        handler.Handle(new InvalidOperationException("outer", new ArgumentException("inner")));
        handler.Handle(new InvalidOperationException("no inner"));

        client.Received(2).TrackException(typeof(InvalidOperationException));
        client.Received(1).TrackException(typeof(ArgumentException));
    }

    [Fact]
    public void FailoverExceptionHandlerDoesNotDuplicateTextFileLogProviderOnRepeatedCalls()
    {
        var recordedErrors = new List<string>();
        var adapter = Substitute.For<ITextFileLogAdapter>();
        adapter
            .When(a => a.CategorizedError(Arg.Any<string>(), Arg.Any<string>()))
            .Do(ci => recordedErrors.Add(ci.ArgAt<string>(1)));

        ServiceLocator.Register<ITextFileLogAdapter>(() => adapter, Lifetime.Singleton);
        LogConsumer.SetHandler(ServiceLocator.Resolve<DefaultExceptionHandler>());

        FailoverExceptionHandler.Handle(new InvalidOperationException("first-failover"));
        FailoverExceptionHandler.Handle(new InvalidOperationException("second-failover"));
        FailoverExceptionHandler.Handle(new InvalidOperationException("third-failover"));

        Assert.Equal(1, recordedErrors.Count(m => m.Contains("first-failover", StringComparison.Ordinal)));
        Assert.Equal(1, recordedErrors.Count(m => m.Contains("second-failover", StringComparison.Ordinal)));
        Assert.Equal(1, recordedErrors.Count(m => m.Contains("third-failover", StringComparison.Ordinal)));
    }
}
