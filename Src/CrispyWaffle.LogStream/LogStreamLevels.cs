using System;
using System.Linq;
using CrispyWaffle.Log;

namespace CrispyWaffle.LogStream;

/// <summary>
/// The levels a LogStream server accepts, and their mapping from CrispyWaffle <see cref="LogLevel"/>.
/// </summary>
public static class LogStreamLevels
{
    /// <summary>The <c>debug</c> level.</summary>
    public static string Debug { get; } = "debug";

    /// <summary>The <c>info</c> level.</summary>
    public static string Info { get; } = "info";

    /// <summary>The <c>notice</c> level.</summary>
    public static string Notice { get; } = "notice";

    /// <summary>The <c>warning</c> level.</summary>
    public static string Warning { get; } = "warning";

    /// <summary>The <c>error</c> level.</summary>
    public static string Error { get; } = "error";

    /// <summary>The <c>critical</c> level.</summary>
    public static string Critical { get; } = "critical";

    private static readonly string[] _all = { Debug, Info, Notice, Warning, Error, Critical };

    /// <summary>
    /// Determines whether the value is a LogStream level.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <returns><c>true</c> when the server accepts the level.</returns>
    public static bool IsValid(string level) =>
        level != null && _all.Contains(level, StringComparer.Ordinal);

    /// <summary>
    /// Maps a single CrispyWaffle level to a LogStream level.
    /// </summary>
    /// <param name="level">The CrispyWaffle level.</param>
    /// <param name="options">The options holding the configurable trace level.</param>
    /// <returns>The LogStream level.</returns>
    public static string From(LogLevel level, LogStreamOptions options) =>
        level switch
        {
            LogLevel.Fatal => Critical,
            LogLevel.Error => Error,
            LogLevel.Warning => Warning,
            LogLevel.Info => Info,
            LogLevel.Trace => options?.TraceLevel ?? Debug,
            _ => Debug,
        };
}
