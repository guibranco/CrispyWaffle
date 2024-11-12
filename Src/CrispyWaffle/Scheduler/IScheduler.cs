using System;
using System.Collections.Generic;

namespace CrispyWaffle.Scheduler;

/// <summary>
/// Defines the contract for a scheduler that manages scheduling logic based on time expressions.
/// </summary>
/// <remarks>
/// Implementations of this interface are responsible for evaluating time-based expressions and determining
/// whether specific times or schedules are valid. It provides properties to retrieve the components of a
/// schedule, such as days of the week, months, hours, and minutes.
/// </remarks>
public interface IScheduler
{
    /// <summary>
    /// Determines whether the specified time expression is valid according to the scheduler's rules.
    /// </summary>
    /// <param name="expression">The time expression to validate. Typically this could be a cron-like
    /// expression or other time-related format used to define schedules.</param>
    /// <returns><c>true</c> if the specified expression is valid; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method checks if the given expression is in the correct format and satisfies the conditions
    /// defined for valid time expressions. It could be used for validation before scheduling a task.
    /// </remarks>
    bool IsValid(string expression);

    /// <summary>
    /// Determines whether the specified date and time matches a valid scheduled time.
    /// </summary>
    /// <param name="dateTime">The date and time to check against the schedule.</param>
    /// <returns><c>true</c> if the specified date and time matches the schedule; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method checks whether the provided <paramref name="dateTime"/> matches any of the criteria
    /// defined in the schedule, such as specific days of the week, months, or times of day.
    /// </remarks>
    bool IsTime(DateTime dateTime);

    /// <summary>
    /// Gets the days of the week that are part of the schedule.
    /// </summary>
    /// <value>A collection of integers representing the days of the week (1 for Sunday, 7 for Saturday).</value>
    /// <remarks>
    /// This property contains a collection of day numbers that are part of the schedule. For example, if
    /// the schedule runs on Mondays and Wednesdays, this collection will contain 2 and 4.
    /// </remarks>
    ICollection<int> DaysOfWeek { get; }

    /// <summary>
    /// Gets the months that are part of the schedule.
    /// </summary>
    /// <value>A collection of integers representing the months (1 for January, 12 for December).</value>
    /// <remarks>
    /// This property contains a collection of months that are part of the schedule. For example, if the
    /// schedule runs every January and March, this collection will contain 1 and 3.
    /// </remarks>
    ICollection<int> Months { get; }

    /// <summary>
    /// Gets the days of the month that are part of the schedule.
    /// </summary>
    /// <value>A collection of integers representing the days of the month (1 to 31).</value>
    /// <remarks>
    /// This property contains a collection of days that are part of the schedule. For example, if the
    /// schedule runs on the 15th and 30th of the month, this collection will contain 15 and 30.
    /// </remarks>
    ICollection<int> DaysOfMonth { get; }

    /// <summary>
    /// Gets the hours of the day that are part of the schedule.
    /// </summary>
    /// <value>A collection of integers representing the hours (0 to 23).</value>
    /// <remarks>
    /// This property contains a collection of hours that are part of the schedule. For example, if the
    /// schedule runs at 9 AM and 5 PM, this collection will contain 9 and 17.
    /// </remarks>
    ICollection<int> Hours { get; }

    /// <summary>
    /// Gets the minutes of the hour that are part of the schedule.
    /// </summary>
    /// <value>A collection of integers representing the minutes (0 to 59).</value>
    /// <remarks>
    /// This property contains a collection of minutes that are part of the schedule. For example, if the
    /// schedule runs at the 15th and 45th minute of every hour, this collection will contain 15 and 45.
    /// </remarks>
    ICollection<int> Minutes { get; }
}
