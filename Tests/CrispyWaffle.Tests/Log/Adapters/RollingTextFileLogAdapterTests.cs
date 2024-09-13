using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CrispyWaffle.Extensions;
using CrispyWaffle.Log.Adapters;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CrispyWaffle.Tests.Log.Adapters;

[Collection("Sequential")]
public class RollingTextFileLogAdapterTests
    /// <summary>
    /// Tests that logs are saved to a text file correctly.
    /// </summary>
    /// <remarks>
    /// This test method verifies that the <see cref="RollingTextFileLogAdapter"/> correctly logs 
    /// information messages to a text file. It initializes the log adapter with a specified 
    /// file name seed and size limits, then generates a message consisting of repeated characters 
    /// and logs this message 100 times. After logging, it disposes of the adapter and checks 
    /// the generated log files to ensure that they contain entries. The method uses regular 
    /// expressions to match the log file names based on the specified seed and cleans up any 
    /// log files created during the test execution. This ensures that the logging functionality 
    /// works as intended and that the log files are properly populated.
    /// </remarks>
    /// <exception cref="ApplicationException">Thrown when an application-specific error occurs.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an argument is outside the allowable range.</exception>
    /// <exception cref="AccessViolationException">Thrown when there is an attempt to read or write protected memory.</exception>
{
    [Fact]
    public void BasicSaveLogsToTextFileTest()
    {
        var fileNameSeed = "basicTextLogs";
        var adapter = new RollingTextFileLogAdapter(
            AppDomain.CurrentDomain.BaseDirectory,
            fileNameSeed,
            100,
            (Unit.KByte, 10)
        );
        var message = new string(Enumerable.Repeat('0', 100).ToArray());

        for (int i = 0; i < 100; i++)
        {
            adapter.Info(message);
        }

        adapter.Dispose();

        var regexFileName = new Regex(GetFileNameRegex(fileNameSeed, LogFileType.Text));
        var files = Directory
            .GetFiles(
                AppDomain.CurrentDomain.BaseDirectory,
                $"*.{LogFileType.Text.GetInternalValue()}"
            )
            .Where(x => regexFileName.IsMatch(x))
            .ToList();

        foreach (var file in files)
        {
            Assert.True(File.ReadLines(file).Any());
        }

        Clean(files);
        /// <summary>
        /// Tests that exceptions are logged to a text file correctly.
        /// </summary>
        /// <remarks>
        /// This test method verifies that when an exception is traced using the
        /// <see cref="RollingTextFileLogAdapter"/>, the expected exception message
        /// is logged to a text file. The test creates a nested exception structure
        /// and traces it multiple times (100 times in this case). After tracing,
        /// it checks the generated log files to ensure that the exception message
        /// appears the expected number of times. The method uses regular expressions
        /// to match the log file names and to count occurrences of the exception
        /// message in the log files. Finally, it cleans up any log files created
        /// during the test execution.
        /// </remarks>
        /// <exception cref="ApplicationException">Thrown when an application-specific error occurs.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an argument is outside the allowable range.</exception>
        /// <exception cref="AccessViolationException">Thrown when there is an attempt to read or write protected memory.</exception>
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage",
        "CA2201:Do not raise reserved exception types",
        Justification = "Testing."
    )]
    public void ExceptionLogsToTextFileTest()
    {
        var fileNameSeed = "exceptionTextLogs";
        var adapter = new RollingTextFileLogAdapter(
            AppDomain.CurrentDomain.BaseDirectory,
            fileNameSeed,
            100,
            (Unit.KByte, 10)
        );
        var exception = new ApplicationException(
            "Main",
            new ArgumentOutOfRangeException(
                "AOOR Exception",
                new AccessViolationException("AW Exception")
            )
        );

        adapter.SetLevel(CrispyWaffle.Log.LogLevel.Development);

        for (int i = 0; i < 100; i++)
        {
            adapter.Trace(exception);
        }

        adapter.Dispose();

        var regexFileName = new Regex(GetFileNameRegex(fileNameSeed, LogFileType.Text));
        var files = Directory
            .GetFiles(
                AppDomain.CurrentDomain.BaseDirectory,
                $"*.{LogFileType.Text.GetInternalValue()}"
            )
            .Where(x => regexFileName.IsMatch(x))
            .ToList();
        var exMsgCount = 0;

        foreach (var file in files)
        {
            exMsgCount += Regex
                .Matches(
                    File.ReadAllText(file),
                    $"\\[MainException\\]\\[{exception.GetType()}\\]: {exception.Message}"
                )
                .Count;
        }

        Assert.True(exMsgCount == 100);

        Clean(files);
    }

    [Fact]
    public void BasicSaveLogsToJsonFileTest()
    {
        var fileNameSeed = "basicJsonLogs";
        var adapter = new RollingTextFileLogAdapter(
            AppDomain.CurrentDomain.BaseDirectory,
            fileNameSeed,
            100,
            (Unit.KByte, 10),
            LogFileType.JSON
        );
        var message = new string(Enumerable.Repeat('0', 1000).ToArray());

        for (int i = 0; i < 100; i++)
        {
            adapter.Info(message);
        }

        adapter.Dispose();

        var regexFileName = new Regex(GetFileNameRegex(fileNameSeed, LogFileType.JSON));
        var files = Directory
            .GetFiles(
                AppDomain.CurrentDomain.BaseDirectory,
                $"*.{LogFileType.JSON.GetInternalValue()}"
            )
            .Where(x => regexFileName.IsMatch(x))
            .ToList();

        foreach (var file in files)
        {
            Assert.True(JArray.Parse(File.ReadAllText(file)) is not null);
        }

        Clean(files);
    }

    /// <summary>
    /// Tests the maximum message constraint for the RollingTextFileLogAdapter.
    /// </summary>
    /// <remarks>
    /// This test verifies that the RollingTextFileLogAdapter correctly limits the number of log files created
    /// when logging messages. It initializes the adapter with a specified file name seed and configuration,
    /// then logs a predefined message multiple times. After logging, it checks that the number of log files
    /// generated does not exceed the expected limit. The test also ensures that each log file contains valid JSON
    /// data by attempting to parse the contents of each file. Finally, any created log files are cleaned up
    /// after the test execution.
    /// </remarks>
    /// <exception cref="System.IO.IOException">
    /// Thrown when there is an issue accessing the file system, such as when reading or writing log files.
    /// </exception>
    [Fact]
    public void MaxMessageConstraintTest()
    {
        var fileNameSeed = "maxMessageLogs";
        var adapter = new RollingTextFileLogAdapter(
            AppDomain.CurrentDomain.BaseDirectory,
            fileNameSeed,
            100,
            (Unit.MByte, 1),
            LogFileType.JSON
        );
        adapter.SetLevel(CrispyWaffle.Log.LogLevel.Debug);
        var message = "Message";

        for (int i = 0; i < 1000; i++)
        {
            adapter.Debug(message);
        }

        adapter.Dispose();

        var regexFileName = new Regex(GetFileNameRegex(fileNameSeed, LogFileType.JSON));
        var files = Directory
            .GetFiles(
                AppDomain.CurrentDomain.BaseDirectory,
                $"*.{LogFileType.JSON.GetInternalValue()}"
            )
            .Where(x => regexFileName.IsMatch(x))
            .ToList();

        Assert.True(files.Count == 10);

        foreach (var file in files)
        {
            Assert.True(JArray.Parse(File.ReadAllText(file)) is not null);
        }

        Clean(files);
    }

    [Fact]
    public void MaxSizeConstraintTest()
    {
        var fileNameSeed = "maxSizeLogs";
        var adapter = new RollingTextFileLogAdapter(
            AppDomain.CurrentDomain.BaseDirectory,
            fileNameSeed,
            100000,
            (Unit.KByte, 1),
            LogFileType.JSON
        );
        var message = new string(Enumerable.Repeat('0', 990).ToArray());

        for (int i = 0; i < 50; i++)
        {
            adapter.Info(message);
        }

        adapter.Dispose();

        var regexFileName = new Regex(GetFileNameRegex(fileNameSeed, LogFileType.JSON));
        var files = Directory
            .GetFiles(
                AppDomain.CurrentDomain.BaseDirectory,
                $"*.{LogFileType.JSON.GetInternalValue()}"
            )
            .Where(x => regexFileName.IsMatch(x))
            .ToList();

        Assert.True(files.Count == 50);

        foreach (var file in files)
        {
            Assert.True(JArray.Parse(File.ReadAllText(file)) is not null);
        }

        Clean(files);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage",
        "xUnit1031:Do not use blocking task operations in test method",
        Justification = "Testing."
    )]
    public void MultiThreadedTest()
    {
        var fileNameSeed = "multiThreadedLogs";
        var adapter = new RollingTextFileLogAdapter(
            AppDomain.CurrentDomain.BaseDirectory,
            fileNameSeed,
            10,
            (Unit.MByte, 100),
            LogFileType.JSON
        );
        var tasks = new Task[40];

        for (int i = 0; i < 40; i += 4)
        {
            tasks[i] = Task.Run(() => adapter.Info(Guid.NewGuid().ToString()));
            tasks[i + 1] = Task.Run(() => adapter.Warning(Guid.NewGuid().ToString()));
            tasks[i + 2] = Task.Run(() => adapter.Error(Guid.NewGuid().ToString()));
            tasks[i + 3] = Task.Run(() => adapter.Fatal(Guid.NewGuid().ToString()));
        }

        Task.WaitAll(tasks);

        adapter.Dispose();

        var messageSet = new HashSet<string>();
        var regexFileName = new Regex(GetFileNameRegex(fileNameSeed, LogFileType.JSON));
        var files = Directory
            .GetFiles(
                AppDomain.CurrentDomain.BaseDirectory,
                $"*.{LogFileType.JSON.GetInternalValue()}"
            )
            .Where(x => regexFileName.IsMatch(x))
            .ToList();

        Assert.True(files.Count == 4);

        JArray jsonArray;
        foreach (var file in files)
        {
            jsonArray = JArray.Parse(File.ReadAllText(file));

            Assert.True(jsonArray is not null);
            Assert.True(jsonArray.Count == 10);

            foreach (var obj in jsonArray)
            {
                messageSet.Add(obj["Message"].Value<string>());
            }
        }

        Assert.True(messageSet.Count == 40);

        Clean(files);
    }

    private static string GetFileNameRegex(string fileNameSeed, LogFileType fileType)
    {
        return $"LogFile-{fileNameSeed}-\\[[0-9]+\\].{fileType.GetInternalValue()}";
    }

    private static void Clean(List<string> files)
    {
        files.ForEach(File.Delete);
    }
}
