using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CrispyWaffle.Extensions;
using CrispyWaffle.Log;
using CrispyWaffle.Log.Adapters;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CrispyWaffle.Tests.Log.Adapters;

[Collection("Sequential")]
public class RollingTextFileLogAdapterTests
{
    [Theory]
    [InlineData(LogLevel.Production, false)]
    [InlineData(LogLevel.All, true)]
    public void CategorizedDebugRespectsLogLevelTest(LogLevel level, bool shouldLog)
    {
        var folderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(folderPath);

        try
        {
            using (var adapter = new RollingTextFileLogAdapter(
                folderPath,
                "categorizedDebug",
                100,
                (Unit.KByte, 100),
                LogFileType.JSON
            ))
            {
                adapter.SetLevel(level);
                adapter.CategorizedDebug("category", "debug message", "identifier");
            }

            var file = Assert.Single(Directory.GetFiles(folderPath));
            var messages = JArray.Parse(File.ReadAllText(file));
            var debugMessages = messages.Where(message =>
                message["Message"].Value<string>() == "debug message"
            );

            if (shouldLog)
            {
                var message = Assert.Single(debugMessages);
                Assert.Equal("Debug", message["Level"].Value<string>());
                Assert.Equal("category", message["Category"].Value<string>());
                Assert.Equal("identifier", message["MessageIdentifier"].Value<string>());
            }
            else
            {
                Assert.Empty(debugMessages);
            }
        }
        finally
        {
            Directory.Delete(folderPath, true);
        }
    }

    [Fact]
    public void SetLevelLogsPreviousAndNewLevelsTest()
    {
        var folderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(folderPath);

        try
        {
            using (var adapter = new RollingTextFileLogAdapter(
                folderPath,
                "setLevel",
                100,
                (Unit.KByte, 100),
                LogFileType.JSON
            ))
            {
                adapter.SetLevel(LogLevel.All);
            }

            var file = Assert.Single(Directory.GetFiles(folderPath));
            var message = Assert.Single(JArray.Parse(File.ReadAllText(file)));
            Assert.Equal("Warning", message["Level"].Value<string>());
            Assert.Equal(
                $"Level updated from {LogLevel.Production.GetHumanReadableValue()} to {LogLevel.All.GetHumanReadableValue()}.",
                message["Message"].Value<string>()
            );
        }
        finally
        {
            Directory.Delete(folderPath, true);
        }
    }

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

        Assert.Equal(10, files.Count);

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

    private static string GetFileNameRegex(string fileNameSeed, LogFileType fileType) =>
        $"LogFile-{fileNameSeed}-\\[[0-9]+\\].{fileType.GetInternalValue()}";

    private static void Clean(List<string> files) => files.ForEach(File.Delete);
}
