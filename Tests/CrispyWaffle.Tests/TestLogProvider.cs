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
        _testOutputHelper.WriteLine("Set level: {0}", level.GetHumanReadableValue());

    public void Fatal(string category, string message) =>
        _testOutputHelper.WriteLine("Fatal: {0} - {1}", category, message);

    public void Error(string category, string message) =>
        _testOutputHelper.WriteLine("Error: {0} - {1}", category, message);

    public void Warning(string category, string message) =>
        _testOutputHelper.WriteLine("Warning: {0} - {1}", category, message);

    public void Info(string category, string message) =>
        System.Diagnostics.Trace.WriteLine($"Info: {category} - {message}");

    public void Trace(string category, string message) =>
        System.Diagnostics.Trace.WriteLine($"Trace: {category} - {message}");

    public void Trace(string category, string message, Exception exception)
    {
        _testOutputHelper.WriteLine("Trace: {0} - {1}", category, message);
        WriteExceptionDetails("Trace", category, exception);
    }

    public void Trace(string category, Exception exception) =>
        WriteExceptionDetails("Trace", category, exception);

    public void Debug(string category, string message) =>
        _testOutputHelper.WriteLine("Debug: {0} - {1}", category, message);

    public void Debug(string category, string content, string identifier)
    {
        _testOutputHelper.WriteLine("Error: {0} - {1}", category, identifier);
        _testOutputHelper.WriteLine(content);
    }

    public void Debug<T>(
        string category,
        T content,
        string identifier,
        SerializerFormat customFormat
    )
        where T : class, new()
    {
        _testOutputHelper.WriteLine("Error: {0} - {1}", category, identifier);
        _testOutputHelper.WriteLine((string)content.GetCustomSerializer(customFormat));
    }

    private void WriteExceptionDetails(string level, string category, Exception exception)
    {
        do
        {
            _testOutputHelper.WriteLine("{0}: {1} - {2}", level, category, exception.Message);
            _testOutputHelper.WriteLine(
                "{0}: {1} - {2}",
                level,
                category,
                exception.GetType().FullName
            );
            _testOutputHelper.WriteLine("{0}: {1} - {2}", level, category, exception.StackTrace);

            exception = exception.InnerException;
        } while (exception != null);
    }
}
