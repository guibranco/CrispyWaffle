using System;
using System.Globalization; // Add this namespace for CultureInfo
using System.Linq;
using CrispyWaffle.Scheduler;
using Xunit;

namespace CrispyWaffle.Tests.Scheduler;

public class CronSchedulerTests
{
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

    [Fact]
    public void ValidateInvalidExpression() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new CronScheduler("invalid expression"));

    [Fact]
    public void ValidateDivided()
    {
        var expected = new[] { 0, 3, 6, 9, 12, 15, 18, 21 };

        var scheduler = new CronScheduler("* */3");

        Assert.Equal(expected, scheduler.Hours);
    }

    [Fact]
    public void ValidateRange()
    {
        var expected = Enumerable.Range(10, 6).ToArray();

        var scheduler = new CronScheduler("* 10-15 * * *");

        Assert.Equal(expected, scheduler.Hours);

        scheduler = new CronScheduler("10-15");

        Assert.Equal(expected, scheduler.Minutes);

        expected = [10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30];

        scheduler = new CronScheduler("10-30/2");

        Assert.Equal(expected, scheduler.Minutes);
    }

    [Fact]
    public void ValidateWild()
    {
        var expectedMinutes = Enumerable.Range(0, 60);
        var expectedHours = Enumerable.Range(0, 24);

        var scheduler = new CronScheduler("*");

        Assert.Equal(expectedMinutes, scheduler.Minutes);

        scheduler = new CronScheduler("10-12 *");

        Assert.Equal([10, 11, 12], scheduler.Minutes);
        Assert.Equal(expectedHours, scheduler.Hours);
    }

    [Fact]
    public void ValidateMinutes()
    {
        var expected = Enumerable.Range(1, 10);

        var scheduler = new CronScheduler("1-10 * * * *");

        Assert.Equal(expected, scheduler.Minutes);

        expected = [1, 2, 3, 4, 5];

        scheduler = new CronScheduler("1,2,3,4,5 * * * *");

        Assert.Equal(expected, scheduler.Minutes);
    }

    [Fact]
    public void ValidateHours()
    {
        var expected = Enumerable.Range(1, 10);

        var scheduler = new CronScheduler("* 1-10 * * *");

        Assert.Equal(expected, scheduler.Hours);

        expected = [1, 2, 3, 4, 5];

        scheduler = new CronScheduler("* 1,2,3,4,5 * * *");

        Assert.Equal(expected, scheduler.Hours);
    }

    [Fact]
    public void ValidateDaysOfMonth()
    {
        var expected = Enumerable.Range(1, 10);

        var scheduler = new CronScheduler("* * 1-10 * *");

        Assert.Equal(expected, scheduler.DaysOfMonth);

        expected = [1, 2, 3, 4, 5];

        scheduler = new CronScheduler("* * 1,2,3,4,5 * *");

        Assert.Equal(expected, scheduler.DaysOfMonth);
    }

    [Fact]
    public void ValidateMonths()
    {
        var expected = Enumerable.Range(1, 10);

        var scheduler = new CronScheduler("* * * 1-10 *");

        Assert.Equal(expected, scheduler.Months);

        expected = [1, 2, 3, 4, 5];

        scheduler = new CronScheduler("* * * 1,2,3,4,5 *");

        Assert.Equal(expected, scheduler.Months);
    }

    [Fact]
    public void ValidateDaysOfWeek()
    {
        var expected = Enumerable.Range(1, 5);

        var scheduler = new CronScheduler("* * * 1-5 *");

        Assert.Equal(expected, scheduler.Months);

        expected = [0, 6];

        scheduler = new CronScheduler("* * * * 0,6");

        Assert.Equal(expected, scheduler.DaysOfWeek);
    }

    [Fact]
    public void ValidateIsTimeFixedValue()
    {
        var scheduler = new CronScheduler("0");

        // Use DateTime.Parse with InvariantCulture to avoid locale issues
        Assert.True(
            scheduler.IsTime(DateTime.Parse("2020-09-05 12:00:10", CultureInfo.InvariantCulture))
        );
        Assert.False(
            scheduler.IsTime(DateTime.Parse("2020-09-05 12:01:10", CultureInfo.InvariantCulture))
        );

        scheduler = new CronScheduler("19 * * 9 *");
        Assert.True(
            scheduler.IsTime(DateTime.Parse("2020-09-05 12:19:10", CultureInfo.InvariantCulture))
        );
        Assert.False(
            scheduler.IsTime(DateTime.Parse("2020-09-05 12:01:10", CultureInfo.InvariantCulture))
        );
        Assert.False(
            scheduler.IsTime(DateTime.Parse("2020-10-05 12:19:10", CultureInfo.InvariantCulture))
        );
    }

    [Fact]
    public void ValidateIsTimeDividedValue()
    {
        var scheduler = new CronScheduler("*/5");

        // Use DateTime.Parse with InvariantCulture to avoid locale issues
        Assert.True(
            scheduler.IsTime(DateTime.Parse("2020-09-05 12:45:45", CultureInfo.InvariantCulture))
        );
        Assert.True(
            scheduler.IsTime(DateTime.Parse("2020-09-05 12:00:34", CultureInfo.InvariantCulture))
        );
        Assert.True(
            scheduler.IsTime(DateTime.Parse("2020-09-05 12:30:59", CultureInfo.InvariantCulture))
        );
        Assert.True(
            scheduler.IsTime(DateTime.Parse("2020-09-05 12:55:00", CultureInfo.InvariantCulture))
        );

        Assert.False(
            scheduler.IsTime(DateTime.Parse("2020-09-05 12:01:00", CultureInfo.InvariantCulture))
        );
        Assert.False(
            scheduler.IsTime(DateTime.Parse("2020-09-05 12:59:45", CultureInfo.InvariantCulture))
        );
        Assert.False(
            scheduler.IsTime(DateTime.Parse("2020-09-05 12:59:59", CultureInfo.InvariantCulture))
        );
        Assert.False(
            scheduler.IsTime(DateTime.Parse("2020-09-05 12:31:50", CultureInfo.InvariantCulture))
        );
    }

    [Fact]
    public void ValidateIsTimeListValue()
    {
        var scheduler = new CronScheduler("* 12,13,14 * * *");

        // Use DateTime.Parse with InvariantCulture to avoid locale issues
        Assert.True(
            scheduler.IsTime(DateTime.Parse("2020-09-05 12:45:00", CultureInfo.InvariantCulture))
        );
        Assert.True(
            scheduler.IsTime(DateTime.Parse("2020-09-05 12:55:55", CultureInfo.InvariantCulture))
        );
        Assert.True(
            scheduler.IsTime(DateTime.Parse("2020-09-05 13:45:40", CultureInfo.InvariantCulture))
        );
        Assert.True(
            scheduler.IsTime(DateTime.Parse("2020-09-05 14:15:33", CultureInfo.InvariantCulture))
        );
        Assert.True(
            scheduler.IsTime(DateTime.Parse("2020-09-05 14:32:34", CultureInfo.InvariantCulture))
        );

        Assert.False(
            scheduler.IsTime(DateTime.Parse("2020-09-05 00:00", CultureInfo.InvariantCulture))
        );
        Assert.False(
            scheduler.IsTime(DateTime.Parse("2020-09-05 08:00:59", CultureInfo.InvariantCulture))
        );
        Assert.False(
            scheduler.IsTime(DateTime.Parse("2020-09-05 15:00:00", CultureInfo.InvariantCulture))
        );
        Assert.False(
            scheduler.IsTime(DateTime.Parse("2020-09-05 23:59:59", CultureInfo.InvariantCulture))
        );
    }
}
