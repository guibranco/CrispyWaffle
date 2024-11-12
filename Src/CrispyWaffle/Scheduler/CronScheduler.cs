using System;
using System.Collections.Generic;
using System.Linq;

namespace CrispyWaffle.Scheduler;

/// <summary>
/// Represents a Cron-based scheduler that parses a cron expression and determines if a given <see cref="DateTime"/> matches the cron schedule.
/// Implements the <see cref="CrispyWaffle.Scheduler.IScheduler" /> interface.
/// </summary>
/// <seealso cref="CrispyWaffle.Scheduler.IScheduler" />
public class CronScheduler : IScheduler
{
    /// <summary>
    /// The cron expression that defines the schedule.
    /// </summary>
    private readonly string _expression;

    /// <summary>
    /// A collection of days of the week that the cron expression matches.
    /// </summary>
    private readonly HashSet<int> _daysOfWeek = new HashSet<int>();

    /// <summary>
    /// A collection of months that the cron expression matches.
    /// </summary>
    private readonly HashSet<int> _months = new HashSet<int>();

    /// <summary>
    /// A collection of days of the month that the cron expression matches.
    /// </summary>
    private readonly HashSet<int> _daysOfMonth = new HashSet<int>();

    /// <summary>
    /// A collection of hours that the cron expression matches.
    /// </summary>
    private readonly HashSet<int> _hours = new HashSet<int>();

    /// <summary>
    /// A collection of minutes that the cron expression matches.
    /// </summary>
    private readonly HashSet<int> _minutes = new HashSet<int>();

    /// <summary>
    /// Initializes a new instance of the <see cref="CronScheduler"/> class with a default cron expression.
    /// </summary>
    public CronScheduler() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CronScheduler"/> class with a specified cron expression.
    /// The expression must be valid according to CRON syntax.
    /// </summary>
    /// <param name="expression">The cron expression to be parsed and used for scheduling.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the provided expression is not valid according to CRON syntax.</exception>
    public CronScheduler(string expression)
    {
        if (!IsValid(expression))
        {
            throw new ArgumentOutOfRangeException(
                nameof(expression),
                expression,
                "The expression isn't a valid CRON expression."
            );
        }

        _expression = expression;

        GenerateData();
    }

    /// <summary>
    /// Populates the internal collections (minutes, hours, days of month, months, days of week) based on the cron expression.
    /// </summary>
    private void GenerateData()
    {
        var matches = CronSchedulerValidations.ValidationRegex.Matches(_expression);

        GenerateData(matches[0].ToString(), 0, 60).ForEach(i => _minutes.Add(i));
        GenerateData(matches.Count > 1 ? matches[1].ToString() : "*", 0, 24)
            .ForEach(i => _hours.Add(i));
        GenerateData(matches.Count > 2 ? matches[2].ToString() : "*", 1, 32)
            .ForEach(i => _daysOfMonth.Add(i));
        GenerateData(matches.Count > 3 ? matches[3].ToString() : "*", 1, 13)
            .ForEach(i => _months.Add(i));
        GenerateData(matches.Count > 4 ? matches[4].ToString() : "*", 0, 7)
            .ForEach(i => _daysOfWeek.Add(i));
    }

    /// <summary>
    /// Generates a list of integers based on a cron expression value, accounting for special characters such as wildcards, ranges, and lists.
    /// </summary>
    /// <param name="value">The value of the cron field (minute, hour, day of month, month, or day of week).</param>
    /// <param name="start">The start value for the range (e.g., 0 for minutes, 1 for days of the month).</param>
    /// <param name="max">The maximum value for the range (e.g., 60 for minutes, 24 for hours).</param>
    /// <returns>A list of integers that correspond to the parsed cron expression value.</returns>
    private static List<int> GenerateData(string value, int start, int max)
    {
        if (CronSchedulerValidations.DividedRegex.IsMatch(value))
        {
            return GenerateDivided(value, start, max);
        }

        if (CronSchedulerValidations.RangeRegex.IsMatch(value))
        {
            return GenerateRange(value);
        }

        if (CronSchedulerValidations.WildRegex.IsMatch(value))
        {
            return GenerateWild(start, max);
        }

        return CronSchedulerValidations.ListRegex.IsMatch(value)
            ? GenerateList(value)
            : new List<int>();
    }

    /// <summary>
    /// Generates a list of integers based on a divided cron value (e.g., "*/5" for every 5th minute).
    /// </summary>
    /// <param name="value">The cron value with a divisor (e.g., "*/5").</param>
    /// <param name="start">The start value for the range.</param>
    /// <param name="max">The maximum value for the range.</param>
    /// <returns>A list of integers that are divisible by the specified divisor.</returns>
    private static List<int> GenerateDivided(string value, int start, int max)
    {
        var result = new List<int>();

        var split = value.Split('/');
        var divisor = int.Parse(split[1]);

        for (var i = start; i < max; i++)
        {
            if (i % divisor == 0)
            {
                result.Add(i);
            }
        }

        return result;
    }

    /// <summary>
    /// Generates a list of integers based on a range cron value (e.g., "1-5" for the range 1 to 5).
    /// </summary>
    /// <param name="value">The cron value with a range (e.g., "1-5").</param>
    /// <returns>A list of integers in the specified range.</returns>
    private static List<int> GenerateRange(string value)
    {
        var result = new List<int>();

        var split = value.Split('-');
        var start = int.Parse(split[0]);
        int end;

        if (split[1].Contains("/"))
        {
            split = split[1].Split('/');
            end = int.Parse(split[0]);
            var divisor = int.Parse(split[1]);

            for (var i = start; i <= end; i++)
            {
                if (i % divisor == 0)
                {
                    result.Add(i);
                }
            }

            return result;
        }

        end = int.Parse(split[1]);

        for (var i = start; i <= end; i++)
        {
            result.Add(i);
        }

        return result;
    }

    /// <summary>
    /// Generates a list of integers for a wildcard cron value (e.g., "*" for all values in the range).
    /// </summary>
    /// <param name="start">The start value for the range (e.g., 0 for minutes, 1 for days of the month).</param>
    /// <param name="max">The maximum value for the range (e.g., 60 for minutes, 24 for hours).</param>
    /// <returns>A list of integers representing all possible values in the range.</returns>
    private static List<int> GenerateWild(int start, int max) =>
        Enumerable.Range(start, max).ToList();

    /// <summary>
    /// Generates a list of integers from a comma-separated list of values (e.g., "1,5,10").
    /// </summary>
    /// <param name="value">A comma-separated list of values.</param>
    /// <returns>A list of integers parsed from the provided comma-separated string.</returns>
    private static List<int> GenerateList(string value) =>
        value.Split(',').Select(int.Parse).ToList();

    /// <summary>
    /// Gets the collection of days of the week that match the cron expression.
    /// </summary>
    /// <value>A collection of integers representing the days of the week (0-6, where 0 is Sunday and 6 is Saturday).</value>
    public ICollection<int> DaysOfWeek => _daysOfWeek;

    /// <summary>
    /// Gets the collection of months that match the cron expression.
    /// </summary>
    /// <value>A collection of integers representing the months (1-12, where 1 is January and 12 is December).</value>
    public ICollection<int> Months => _months;

    /// <summary>
    /// Gets the collection of days of the month that match the cron expression.
    /// </summary>
    /// <value>A collection of integers representing the days of the month (1-31).</value>
    public ICollection<int> DaysOfMonth => _daysOfMonth;

    /// <summary>
    /// Gets the collection of hours that match the cron expression.
    /// </summary>
    /// <value>A collection of integers representing the hours (0-23).</value>
    public ICollection<int> Hours => _hours;

    /// <summary>
    /// Gets the collection of minutes that match the cron expression.
    /// </summary>
    /// <value>A collection of integers representing the minutes (0-59).</value>
    public ICollection<int> Minutes => _minutes;

    /// <summary>
    /// Validates whether the specified cron expression is valid.
    /// </summary>
    /// <param name="expression">The cron expression to validate.</param>
    /// <returns><c>true</c> if the specified cron expression is valid; otherwise, <c>false</c>.</returns>
    public bool IsValid(string expression) =>
        CronSchedulerValidations.ValidationRegex.Matches(expression).Count > 0;

    /// <summary>
    /// Determines whether the specified <see cref="DateTime"/> matches the cron schedule.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> to check against the cron schedule.</param>
    /// <returns><c>true</c> if the specified date and time match the cron schedule; otherwise, <c>false</c>.</returns>
    public bool IsTime(DateTime dateTime)
    {
        return _minutes.Contains(dateTime.Minute)
            && _hours.Contains(dateTime.Hour)
            && _daysOfMonth.Contains(dateTime.Day)
            && _months.Contains(dateTime.Month)
            && _daysOfWeek.Contains((int)dateTime.DayOfWeek);
    }
}
