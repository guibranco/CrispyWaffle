using System;
using System.Text.RegularExpressions;

namespace CrispyWaffle.Scheduler;

/// <summary>
/// A static class that contains regular expressions used to validate different parts of a cron expression.
/// These validations handle specific cron syntax features, such as ranges, wildcards, lists, and divisions.
/// </summary>
public static class CronSchedulerValidations
{
    /// <summary>
    /// A regular expression that matches cron syntax with the division operator (e.g., "*/5").
    /// This expression is used to match fields that represent intervals (e.g., every 5th minute).
    /// </summary>
    public static readonly Regex DividedRegex =
        new(@"(\*/\d+)", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    /// <summary>
    /// A regular expression that matches cron syntax with a range and optional divisor (e.g., "1-5/2").
    /// This expression is used to match fields with ranges (e.g., from 1 to 5) and optionally a step value (e.g., every second value).
    /// </summary>
    public static readonly Regex RangeRegex =
        new(@"(\d+\-\d+)\/?(\d+)?", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    /// <summary>
    /// A regular expression that matches the wildcard character (*) in cron expressions.
    /// The wildcard is used to match all values in a field (e.g., every minute, every day of the month).
    /// </summary>
    public static readonly Regex WildRegex =
        new(@"(\*)", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    /// <summary>
    /// A regular expression that matches lists of values separated by commas (e.g., "1,5,10").
    /// This expression is used to match fields that specify multiple discrete values.
    /// </summary>
    public static readonly Regex ListRegex =
        new(@"(((\d+,)*\d+)+)", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    /// <summary>
    /// A combined validation regular expression that can match any of the individual cron syntax features.
    /// This includes division, ranges, wildcards, and lists, and is used to validate entire cron expressions.
    /// </summary>
    public static readonly Regex ValidationRegex =
        new(
            DividedRegex + "|" + RangeRegex + "|" + WildRegex + "|" + ListRegex,
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(5)
        );
}
