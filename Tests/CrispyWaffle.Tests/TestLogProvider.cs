using System;
using System.Diagnostics.CodeAnalysis;
using CrispyWaffle.Extensions;
using CrispyWaffle.Log;
using CrispyWaffle.Log.Providers;
using CrispyWaffle.Serialization;
using Xunit.Abstractions;

namespace CrispyWaffle.Tests;

[ExcludeFromCodeCoverage]
internal class TestLogProvider : ILogProvider
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TestLogProvider(ITestOutputHelper testOutputHelper) =>
        _testOutputHelper =
            testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

    public void SetLevel(LogLevel level) =>
        SafeWriteLine("Set level: {0}", level.GetHumanReadableValue());

    public void Fatal(string category, string message) =>
        SafeWriteLine("Fatal: {0} - {1}", category, message);

    public void Error(string category, string message) =>
        SafeWriteLine("Error: {0} - {1}", category, message);

    public void Warning(string category, string message) =>
        SafeWriteLine("Warning: {0} - {1}", category, message);

    public void Info(string category, string message) =>
        System.Diagnostics.Trace.WriteLine($"Info: {category} - {message}");

    public void Trace(string category, string message) =>
        System.Diagnostics.Trace.WriteLine($"Trace: {category} - {message}");

    public void Trace(string category, string message, Exception exception)
    {
        SafeWriteLine("Trace: {0} - {1}", category, message);
        WriteExceptionDetails("Trace", category, exception);
    }

    public void Trace(string category, Exception exception) =>
        WriteExceptionDetails("Trace", category, exception);

    public void Debug(string category, string message) =>
        SafeWriteLine("Debug: {0} - {1}", category, message);

    public void Debug(string category, string content, string identifier)
    {
        SafeWriteLine("Error: {0} - {1}", category, identifier);
        SafeWriteLine(content);
    }

    public void Debug<T>(
        string category,
        T content,
        string identifier,
        SerializerFormat customFormat
    )
        where T : class, new()
    {
        SafeWriteLine("Error: {0} - {1}", category, identifier);
        SafeWriteLine((string)content.GetCustomSerializer(customFormat));
    }

    /// <summary>
    /// Writes to the test output, swallowing failures caused by the owning test having
    /// already completed (e.g. this provider is stale, from a prior parallel test run).
    /// </summary>
    private void SafeWriteLine(string message)
    {
        try
        {
            _testOutputHelper.WriteLine(message);
        }
        catch (InvalidOperationException)
        {
            // The test that registered this provider has already finished; ignore.
        }
    }

    /// <summary>
    /// Writes to the test output, swallowing failures caused by the owning test having
    /// already completed (e.g. this provider is stale, from a prior parallel test run).
    /// </summary>
    private void SafeWriteLine(string format, params object[] args)
    {
        try
        {
            _testOutputHelper.WriteLine(format, args);
        }
        catch (InvalidOperationException)
        {
            // The test that registered this provider has already finished; ignore.
        }
    }

    private void WriteExceptionDetails(string level, string category, Exception exception)
    {
        do
        {
            SafeWriteLine("{0}: {1} - {2}", level, category, exception.Message);
            SafeWriteLine("{0}: {1} - {2}", level, category, exception.GetType().FullName);
            SafeWriteLine("{0}: {1} - {2}", level, category, exception.StackTrace);

            exception = exception.InnerException;
        } while (exception != null);
    }
}
