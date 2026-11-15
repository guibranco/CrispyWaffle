using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrispyWaffle.Log;
using CrispyWaffle.Log.Filters;
using CrispyWaffle.Log.Providers;
using CrispyWaffle.Serialization;
using Xunit;

namespace CrispyWaffle.Tests.Log;

public class LogConsumerConcurrencyTests
{
    [Fact]
    public async Task AddingProvidersAndFiltersWhileLoggingShouldNotThrow()
    {
        // Arrange
        using var start = new ManualResetEventSlim();
        using var done = new CancellationTokenSource();

        var writers = Enumerable
            .Range(0, 4)
            .Select(_ =>
                Task.Run(() =>
                {
                    start.Wait();
                    while (!done.IsCancellationRequested)
                    {
                        LogConsumer.Info("concurrency test");
                        LogConsumer.Trace(new InvalidOperationException("concurrency test"));
                    }
                })
            )
            .ToArray();

        // Act
        start.Set();
        for (var i = 0; i < 50; i++)
        {
            LogConsumer.AddProvider(new NullLogProvider());
            LogConsumer.AddFilter(new CategoryLogFilter(typeof(NullLogProvider)));
            await Task.Yield();
        }

        await done.CancelAsync();

        // Assert: before LogConsumer used copy-on-write arrays this threw
        // "Collection was modified; enumeration operation may not execute."
        await Task.WhenAll(writers);
    }

    /// <summary>
    /// A provider that discards everything, so the test exercises only LogConsumer's own lists.
    /// </summary>
    private sealed class NullLogProvider : ILogProvider
    {
        public void SetLevel(LogLevel level)
        {
            // Intentionally empty: the test only needs providers to enumerate.
        }

        public void Fatal(string category, string message)
        {
            // Intentionally empty: the test only needs providers to enumerate.
        }

        public void Error(string category, string message)
        {
            // Intentionally empty: the test only needs providers to enumerate.
        }

        public void Warning(string category, string message)
        {
            // Intentionally empty: the test only needs providers to enumerate.
        }

        public void Info(string category, string message)
        {
            // Intentionally empty: the test only needs providers to enumerate.
        }

        public void Trace(string category, string message)
        {
            // Intentionally empty: the test only needs providers to enumerate.
        }

        public void Trace(string category, string message, Exception exception)
        {
            // Intentionally empty: the test only needs providers to enumerate.
        }

        public void Trace(string category, Exception exception)
        {
            // Intentionally empty: the test only needs providers to enumerate.
        }

        public void Debug(string category, string message)
        {
            // Intentionally empty: the test only needs providers to enumerate.
        }

        public void Debug(string category, string content, string identifier)
        {
            // Intentionally empty: the test only needs providers to enumerate.
        }

        public void Debug<T>(
            string category,
            T content,
            string identifier,
            SerializerFormat customFormat
        )
            where T : class, new()
        {
            // Intentionally empty: the test only needs providers to enumerate.
        }
    }
}
