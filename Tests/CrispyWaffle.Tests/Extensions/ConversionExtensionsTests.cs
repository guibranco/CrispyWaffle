using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using CrispyWaffle.Extensions;
using Xunit;

namespace CrispyWaffle.Tests.Extensions;

public class ConversionExtensionsTests
{
    public ConversionExtensionsTests()
    {
        var currentCulture = CultureInfo.GetCultureInfo("en-US").Name;
        var ci = new CultureInfo(currentCulture)
        {
            NumberFormat =
            {
                NumberDecimalSeparator = ",",
                NumberGroupSeparator = " ",
                CurrencySymbol = "$",
            },
            DateTimeFormat = { DateSeparator = "/" },
        };
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
    }

    [Fact]
    public void ValidateStringToBytes()
    {
        var expected = new byte[1];
        expected[0] = byte.Parse("1");
        var result = new[] { "1" }.ToBytes();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ValidateStringToBoolean()
    {
        var result = "0".ToBoolean();
        Assert.False(result);

        result = "S".ToBoolean();
        Assert.True(result);

        result = "s".ToBoolean();
        Assert.True(result);

        result = "Sim".ToBoolean("Sim");
        Assert.True(result);

        result = "S".ToBoolean("Sim");
        Assert.False(result);
    }

    [Fact]
    public void StringToBooleanInvalidInputReturnsFalse()
    {
        var result = "".ToBoolean();
        Assert.False(result);
    }

    [Fact]
    public void ValidateBooleanToString()
    {
        const string expectedTrue = "S";
        const string expectedFalse = "N";

        var result = true.ToString(expectedTrue, expectedFalse);
        Assert.Equal(expectedTrue, result);

        result = false.ToString(expectedTrue, expectedFalse);
        Assert.Equal(expectedFalse, result);
    }

    [Fact]
    public void ValidateNowToDateTime()
    {
        var date = DateTime.UtcNow;

        var result = "now".ToDateTime();

        Assert.Equal(date.ToLocalTime().Date, result.Date);
        Assert.Equal(date.ToLocalTime().Hour, result.Hour);
        Assert.Equal(date.Minute, result.Minute);
        Assert.Equal(date.Second, result.Second);
    }

    [Fact]
    public void ValidateTodayToDateTime()
    {
        var result = "today".ToDateTime();

        Assert.Equal(DateTime.Today, result);
    }

    [Fact]
    public void ValidateYesterdayToDateTime()
    {
        var result = "yesterday".ToDateTime();

        Assert.Equal(DateTime.Today.AddDays(-1), result);
    }

    [Fact]
    public void ValidateTomorrowToDateTime()
    {
        var result = "tomorrow".ToDateTime();

        Assert.Equal(DateTime.Today.AddDays(1), result);
    }

    [Theory]
    [InlineData("2020-09-07", 2020, 9, 7, 0, 0, 0)]
    [InlineData("2020-09-07 15:45", 2020, 9, 7, 15, 45, 0)]
    [InlineData("2020-09-07 7:50pm", 2020, 9, 7, 19, 50, 0)]
    [InlineData("01/01/2020", 2020, 1, 1, 0, 0, 0)]
    [InlineData("31/12/2020", 2020, 12, 31, 0, 0, 0)]
    [SuppressMessage(
        "ReSharper",
        "TooManyArguments",
        Justification = "This is needed to validate each part of DateTime object"
    )]
    public void ValidateStringDateToDateTime(
        string input,
        int year,
        int month,
        int day,
        int hour,
        int minute,
        int seconds
    )
    {
        var expected = new DateTime(year, month, day, hour, minute, seconds, DateTimeKind.Local);

        var result = input.ToDateTime();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ValidateEmptyStringToDateTime()
    {
        var result = Assert.Throws<ArgumentNullException>(() => string.Empty.ToDateTime());

        Assert.Equal("Input value cannot be null. (Parameter 'input')", result.Message);
    }

    [Theory]
    [InlineData("1234567890")]
    public void ValidateInvalidStringToDateTime(string input)
    {
        var result = Assert.Throws<ArgumentOutOfRangeException>(() => input.ToDateTime());

        Assert.Equal(
            $"Unable to parse the string to a valid datetime. (Parameter 'input'){Environment.NewLine}Actual value was {input}.",
            result.Message
        );
    }

    [Fact]
    public void TryToDateTimeEmptyInputReturnsFalse()
    {
        var result = string.Empty.TryToDateTime(out _);
        Assert.False(result);
    }

    [Fact]
    public void ToInt32EmptyInputReturnsZero()
    {
        var result = string.Empty.ToInt32();
        Assert.Equal(0, result);
    }

    [Fact]
    public void ToInt32InvalidInputReturnsZero()
    {
        var result = "some-string".ToInt32();
        Assert.Equal(0, result);
    }

    [Fact]
    public void ToInt64EmptyInputReturnsZero()
    {
        var result = string.Empty.ToInt64();
        Assert.Equal(0, result);
    }

    [Fact]
    public void ToInt64InvalidInputReturnsZero()
    {
        var result = "some-string".ToInt64();
        Assert.Equal(0, result);
    }

    [Fact]
    public void ToDecimalEmptyInputReturnsZero()
    {
        var result = string.Empty.ToDecimal();
        Assert.Equal(0, result);
    }

    [Fact]
    public void ToDecimalInvalidInputReturnsZero()
    {
        var result = "some-string".ToDecimal();
        Assert.Equal(0, result);
    }

    [Theory]
    [InlineData(1, "$1.00")]
    [InlineData(1000, "$1,000.00")]
    [InlineData(11.33, "$11.33")]
    [InlineData(6547654.477, "$6,547,654.48")]
    [InlineData(0, "No value")]
    public void ToMonetaryValidInputReturnsValidString(decimal input, string expected)
    {
        var result = input.ToMonetary();

        Assert.Equal(expected, result);
    }
}
