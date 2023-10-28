// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 09-05-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="CronSchedulerTests.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Linq;
using CrispyWaffle.Scheduler;
using Xunit;

namespace CrispyWaffle.Tests.Scheduler;

/// <summary>
/// Class CronSchedulerTests.
/// </summary>
public class CronSchedulerTests
{
    /// <summary>
    /// Defines the test method ValidateExpressions.
    /// </summary>
    [Fact]
    public void ValidateExpressions()
    {
        var scheduler = new CronScheduler();

        Assert.True(scheduler.IsValid("*/5"));
        Assert.True(scheduler.IsValid("* * * * *"));
        Assert.True(scheduler.IsValid("0 * * * *"));
        Assert.True(scheduler.IsValid("0,1,2 * * * *"));
        Assert.True(scheduler.IsValid("*/5 * * * *"));
        Assert.True(scheduler.IsValid("1-30 * * * *"));
        Assert.True(scheduler.IsValid("1-30/3 * * * *"));
        Assert.True(scheduler.IsValid("* 10-20 * * *"));
    }

    /// <summary>
    /// Defines the test method ValidateInvalidExpression.
    /// </summary>
    [Fact]
    public void ValidateInvalidExpression()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CronScheduler("invalid expression"));
    }

    /// <summary>
    /// Defines the test method ValidateDivided.
    /// </summary>
    [Fact]
    public void ValidateDivided()
    {
        var expected = new[] { 0, 3, 6, 9, 12, 15, 18, 21 };

        var scheduler = new CronScheduler("* */3");

        Assert.Equal(expected, scheduler.Hours);
    }

    /// <summary>
    /// Defines the test method ValidateRange.
    /// </summary>
    [Fact]
    public void ValidateRange()
    {
        var expected = Enumerable.Range(10, 6).ToArray();

        var scheduler = new CronScheduler("* 10-15 * * *");

        Assert.Equal(expected, scheduler.Hours);

        scheduler = new CronScheduler("10-15");

        Assert.Equal(expected, scheduler.Minutes);

        expected = new[] { 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30 };

        scheduler = new CronScheduler("10-30/2");

        Assert.Equal(expected, scheduler.Minutes);
    }

    /// <summary>
    /// Defines the test method ValidateWild.
    /// </summary>
    [Fact]
    public void ValidateWild()
    {
        var expectedMinutes = Enumerable.Range(0, 60);
        var expectedHours = Enumerable.Range(0, 24);

        var scheduler = new CronScheduler("*");

        Assert.Equal(expectedMinutes, scheduler.Minutes);

        scheduler = new CronScheduler("10-12 *");

        Assert.Equal(new[] { 10, 11, 12 }, scheduler.Minutes);
        Assert.Equal(expectedHours, scheduler.Hours);
    }

    /// <summary>
    /// Defines the test method ValidateMinutes.
    /// </summary>
    [Fact]
    public void ValidateMinutes()
    {
        var expected = Enumerable.Range(1, 10);

        var scheduler = new CronScheduler("1-10 * * * *");

        Assert.Equal(expected, scheduler.Minutes);

        expected = new[] { 1, 2, 3, 4, 5 };

        scheduler = new CronScheduler("1,2,3,4,5 * * * *");

        Assert.Equal(expected, scheduler.Minutes);
    }

    /// <summary>
    /// Defines the test method ValidateHours.
    /// </summary>
    [Fact]
    public void ValidateHours()
    {
        var expected = Enumerable.Range(1, 10);

        var scheduler = new CronScheduler("* 1-10 * * *");

        Assert.Equal(expected, scheduler.Hours);

        expected = new[] { 1, 2, 3, 4, 5 };

        scheduler = new CronScheduler("* 1,2,3,4,5 * * *");

        Assert.Equal(expected, scheduler.Hours);
    }

    /// <summary>
    /// Defines the test method ValidateDaysOfMonth.
    /// </summary>
    [Fact]
    public void ValidateDaysOfMonth()
    {
        var expected = Enumerable.Range(1, 10);

        var scheduler = new CronScheduler("* * 1-10 * *");

        Assert.Equal(expected, scheduler.DaysOfMonth);

        expected = new[] { 1, 2, 3, 4, 5 };

        scheduler = new CronScheduler("* * 1,2,3,4,5 * *");

        Assert.Equal(expected, scheduler.DaysOfMonth);
    }

    /// <summary>
    /// Defines the test method ValidateMonths.
    /// </summary>
    [Fact]
    public void ValidateMonths()
    {
        var expected = Enumerable.Range(1, 10);

        var scheduler = new CronScheduler("* * * 1-10 *");

        Assert.Equal(expected, scheduler.Months);

        expected = new[] { 1, 2, 3, 4, 5 };

        scheduler = new CronScheduler("* * * 1,2,3,4,5 *");

        Assert.Equal(expected, scheduler.Months);
    }

    /// <summary>
    /// Defines the test method ValidateDaysOfWeek.
    /// </summary>
    [Fact]
    public void ValidateDaysOfWeek()
    {
        var expected = Enumerable.Range(1, 5);

        var scheduler = new CronScheduler("* * * 1-5 *");

        Assert.Equal(expected, scheduler.Months);

        expected = new[] { 0, 6 };

        scheduler = new CronScheduler("* * * * 0,6");

        Assert.Equal(expected, scheduler.DaysOfWeek);
    }

    /// <summary>
    /// Defines the test method ValidateIsTimeFixedValue.
    /// </summary>
    [Fact]
    public void ValidateIsTimeFixedValue()
    {
        var scheduler = new CronScheduler("0");

        Assert.True(scheduler.IsTime(DateTime.Parse("2020-09-05 12:00:10")));
        Assert.False(scheduler.IsTime(DateTime.Parse("2020-09-05 12:01:10")));

        scheduler = new CronScheduler("19 * * 9 *");
        Assert.True(scheduler.IsTime(DateTime.Parse("2020-09-05 12:19:10")));
        Assert.False(scheduler.IsTime(DateTime.Parse("2020-09-05 12:01:10")));
        Assert.False(scheduler.IsTime(DateTime.Parse("2020-10-05 12:19:10")));
    }

    /// <summary>
    /// Defines the test method ValidateIsTimeDividedValue.
    /// </summary>
    [Fact]
    public void ValidateIsTimeDividedValue()
    {
        var scheduler = new CronScheduler("*/5");

        Assert.True(scheduler.IsTime(DateTime.Parse("2020-09-05 12:45:45")));
        Assert.True(scheduler.IsTime(DateTime.Parse("2020-09-05 12:00:34")));
        Assert.True(scheduler.IsTime(DateTime.Parse("2020-09-05 12:30:59")));
        Assert.True(scheduler.IsTime(DateTime.Parse("2020-09-05 12:55:00")));

        Assert.False(scheduler.IsTime(DateTime.Parse("2020-09-05 12:01:00")));
        Assert.False(scheduler.IsTime(DateTime.Parse("2020-09-05 12:59:45")));
        Assert.False(scheduler.IsTime(DateTime.Parse("2020-09-05 12:59:59")));
        Assert.False(scheduler.IsTime(DateTime.Parse("2020-09-05 12:31:50")));
    }

    /// <summary>
    /// Defines the test method ValidateIsTimeListValue.
    /// </summary>
    [Fact]
    public void ValidateIsTimeListValue()
    {
        var scheduler = new CronScheduler("* 12,13,14 * * *");

        Assert.True(scheduler.IsTime(DateTime.Parse("2020-09-05 12:45:00")));
        Assert.True(scheduler.IsTime(DateTime.Parse("2020-09-05 12:55:55")));
        Assert.True(scheduler.IsTime(DateTime.Parse("2020-09-05 13:45:40")));
        Assert.True(scheduler.IsTime(DateTime.Parse("2020-09-05 14:15:33")));
        Assert.True(scheduler.IsTime(DateTime.Parse("2020-09-05 14:32:34")));

        Assert.False(scheduler.IsTime(DateTime.Parse("2020-09-05 00:00")));
        Assert.False(scheduler.IsTime(DateTime.Parse("2020-09-05 08:00:59")));
        Assert.False(scheduler.IsTime(DateTime.Parse("2020-09-05 15:00:00")));
        Assert.False(scheduler.IsTime(DateTime.Parse("2020-09-05 23:59:59")));
    }
}
