namespace CrispyWaffle.Tests.Infrastructure
{
    using CrispyWaffle.Infrastructure;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Xunit;

    public class EnvironmentHelperTests
    {
        [Fact]
        public void ValidateEnvironmentHelperProperties()
        {
            EnvironmentHelper.SetApplicationName("CrispyWaffle");
            EnvironmentHelper.SetDisplayApplicationName("Crispy Waffle");

            Assert.Equal("CrispyWaffle", EnvironmentHelper.ApplicationName);
            Assert.Equal("Crispy Waffle", EnvironmentHelper.DisplayApplicationName);

            Assert.NotEmpty(EnvironmentHelper.ExecutionPath);

            Assert.Equal(Environment.MachineName, EnvironmentHelper.Host);

            var ipAddressRegex = new Regex(
                @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.|$)){4}$",
                RegexOptions.Compiled
            );

            Assert.NotEmpty(EnvironmentHelper.IpAddress);
            Assert.Matches(ipAddressRegex, EnvironmentHelper.IpAddress);

            Assert.NotEmpty(EnvironmentHelper.IpAddressExternal);
            Assert.Matches(ipAddressRegex, EnvironmentHelper.IpAddressExternal);

            Assert.EndsWith(
                Environment.Is64BitOperatingSystem ? @"x64" : @"x86",
                EnvironmentHelper.OperationalSystemVersion
            );

            Assert.Equal(Environment.ProcessId, EnvironmentHelper.ProcessId);

            var userAgent =
                $"{EnvironmentHelper.ApplicationName}/{EnvironmentHelper.Version} (H:{EnvironmentHelper.Host}|P:{EnvironmentHelper.ProcessId}|T:{Thread.CurrentThread.ManagedThreadId})";

            Assert.Equal(userAgent, EnvironmentHelper.UserAgent);
            Assert.Equal(Environment.UserName, EnvironmentHelper.UserName);

            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

            var version = !string.IsNullOrWhiteSpace(assembly.Location)
                ? FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion
                : string.Empty;

            Assert.Equal(version, EnvironmentHelper.Version);

            var versionDate = new FileInfo(assembly.Location).LastWriteTime.ToString(
                @"dd/MM/yyyy HH:mm:ss",
                CultureInfo.InvariantCulture
            );

            Assert.Equal(versionDate, EnvironmentHelper.VersionDate);
        }
    }
}
