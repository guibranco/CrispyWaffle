using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CrispyWaffle.Log.Adapters;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CrispyWaffle.Tests.Serialization;

[Collection("Sequential")]
public class RollingTextFileLogAdapterTests
{
    [Fact]
    public void BasicSaveLogsToFileTest()
    {
        var fileNameSeed = "basicLogs";
        var adapter = new RollingTextFileLogAdapter(
            AppDomain.CurrentDomain.BaseDirectory,
            fileNameSeed,
            100,
            (Unit.KByte, 10)
        );
        var message = new string(Enumerable.Repeat('0', 1000).ToArray());

        for (int i = 0; i < 100; i++)
        {
            adapter.Info(message);
        }

        adapter.Dispose();

        var regexFileName = new Regex(GetFileNameRegex(fileNameSeed));
        var files = Directory
            .GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.json")
            .Where(regexFileName.IsMatch)
            .ToList();

        foreach (var file in files)
        {
            Assert.True(File.Exists(file));
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
            (Unit.MByte, 1)
        );
        adapter.SetLevel(Log.LogLevel.Debug);
        var message = "Message";

        for (int i = 0; i < 1000; i++)
        {
            adapter.Debug(message);
        }

        adapter.Dispose();

        var regexFileName = new Regex(GetFileNameRegex(fileNameSeed));
        var files = Directory
            .GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.json")
            .Where(regexFileName.IsMatch)
            .ToList();

        Assert.True(files.Count == 10);

        foreach (var file in files)
        {
            Assert.True(File.Exists(file));
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
            (Unit.KByte, 1)
        );
        var message = new string(Enumerable.Repeat('0', 990).ToArray());

        for (int i = 0; i < 50; i++)
        {
            adapter.Info(message);
        }

        adapter.Dispose();

        var regexFileName = new Regex(GetFileNameRegex(fileNameSeed));
        var files = Directory
            .GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.json")
            .Where(regexFileName.IsMatch)
            .ToList();

        Assert.True(files.Count == 50);

        foreach (var file in files)
        {
            Assert.True(File.Exists(file));
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
            (Unit.MByte, 100)
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
        var regexFileName = new Regex(GetFileNameRegex(fileNameSeed));
        var files = Directory
            .GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.json")
            .Where(regexFileName.IsMatch)
            .ToList();

        Assert.True(files.Count == 4);

        JArray jsonArray;
        foreach (var file in files)
        {
            Assert.True(File.Exists(file));

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

    private static string GetFileNameRegex(string fileNameSeed)
    {
        return $"LogFile-{fileNameSeed}-\\[[0-9]+\\].json";
    }

    private static void Clean(List<string> files)
    {
        files.ForEach(File.Delete);
    }
}
