using System;
using System.Diagnostics.CodeAnalysis;
using CrispyWaffle.Composition;
using CrispyWaffle.Log;
using CrispyWaffle.TemplateRendering.Engines;
using CrispyWaffle.Tests.Composition;
using Xunit.Abstractions;

namespace CrispyWaffle.Tests.Fixtures;

[ExcludeFromCodeCoverage]
public class LoggingFixture : IDisposable
{
    public LoggingFixture()
    {
        ServiceLocator.Register<TestObjects.SingletonTest>(Lifetime.Singleton);
        ServiceLocator.Register<TestObjects.SingletonWithDependencyTest>(Lifetime.Singleton);

        ServiceLocator.Register<ITemplateRender, MustacheTemplateRender>();
    }

    public void SetLogProvider(ITestOutputHelper testOutputHelper)
    {
        LogConsumer.AddProvider(new TestLogProvider(testOutputHelper)).SetLevel(LogLevel.All);
    }

    private bool _disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
        {
            return;
        }

        if (disposing) { }

        _disposedValue = true;
    }

    public void Dispose()
    {
        Dispose(true);
    }
}
