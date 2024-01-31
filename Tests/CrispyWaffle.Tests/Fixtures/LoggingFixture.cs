using System;
using System.Diagnostics.CodeAnalysis;
using CrispyWaffle.Composition;
using CrispyWaffle.Log;
using CrispyWaffle.TemplateRendering.Engines;
using CrispyWaffle.Tests.Composition;
using Xunit.Abstractions;

namespace CrispyWaffle.Tests.Fixtures;

/// <summary>
/// Class LoggingFixture.
/// Implements the <see cref="System.IDisposable" />
/// </summary>
/// <seealso cref="System.IDisposable" />
[ExcludeFromCodeCoverage]
public class LoggingFixture : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingFixture" /> class.
    /// </summary>
    public LoggingFixture()
    {
        ServiceLocator.Register<TestObjects.SingletonTest>(LifeStyle.Singleton);
        ServiceLocator.Register<TestObjects.SingletonWithDependencyTest>(LifeStyle.Singleton);

        ServiceLocator.Register<ITemplateRender, MustacheTemplateRender>();
    }

    /// <summary>
    /// Sets the log provider.
    /// </summary>
    /// <param name="testOutputHelper">The test output helper.</param>
    public void SetLogProvider(ITestOutputHelper testOutputHelper)
    {
        LogConsumer.AddProvider(new TestLogProvider(testOutputHelper)).SetLevel(LogLevel.All);
    }

    /// <summary>
    /// The disposed value
    /// </summary>
    private bool _disposedValue;

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
        {
            return;
        }

        if (disposing)
        {
        }

        _disposedValue = true;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }
}
