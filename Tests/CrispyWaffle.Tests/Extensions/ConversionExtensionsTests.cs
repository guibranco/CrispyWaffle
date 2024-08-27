using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using CrispyWaffle.Extensions;
using Xunit;

namespace CrispyWaffle.Tests.Extensions;

/// <summary>
/// Class ConversionExtensionsTests.
/// </summary>
public class ConversionExtensionsTests
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConversionExtensionsTests"/> class.
    /// </summary>
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

    /// <summary>
    /// Validates the string to bytes.
    /// </summary>
    [Fact]
    public void ValidateStringToBytes()
    {
        var expected = new byte[1];
        expected[0] = byte.Parse("1");
        var result = new[] { "1" }.ToBytes();
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Validates the string to boolean.
    /// </summary>
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

    /// <summary>
    /// Defines the test method StringToBoolean_InvalidInput_ReturnsFalse.
    /// </summary>
    [Fact]
    public void StringToBooleanInvalidInputReturnsFalse()
    {
        var result = "".ToBoolean();
        Assert.False(result);
    }

    /// <summary>
    /// Validates the boolean to string.
    /// </summary>
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

    /// <summary>
    /// Defines the test method ValidateNowToDateTime.
    /// </summary>
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

    /// <summary>
    /// Defines the test method ValidateTodayToDateTime.
    /// </summary>
    [Fact]
    public void ValidateTodayToDateTime()
    {
        var result = "today".ToDateTime();

        Assert.Equal(DateTime.Today, result);
    }

    /// <summary>
    /// Defines the test method ValidateYesterdayToDateTime.
    /// </summary>
    [Fact]
    public void ValidateYesterdayToDateTime()
    {
        var result = "yesterday".ToDateTime();

        Assert.Equal(DateTime.Today.AddDays(-1), result);
    }

    /// <summary>
    /// Defines the test method ValidateTomorrowToDateTime.
    /// </summary>
    [Fact]
    public void ValidateTomorrowToDateTime()
    {
        var result = "tomorrow".ToDateTime();

        Assert.Equal(DateTime.Today.AddDays(1), result);
    }

    /// <summary>
    /// Defines the test method ValidateStringDateToDateTime.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="year">The year.</param>
    /// <param name="month">The month.</param>
    /// <param name="day">The day.</param>
    /// <param name="hour">The hour.</param>
    /// <param name="minute">The minute.</param>
    /// <param name="seconds">The seconds.</param>
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

    /// <summary>
    /// Defines the test method ValidateEmptyStringToDateTime.
    /// </summary>
    [Fact]
    public void ValidateEmptyStringToDateTime()
    {
        var result = Assert.Throws<ArgumentNullException>(() => string.Empty.ToDateTime());

        Assert.Equal("Input value cannot be null. (Parameter 'input')", result.Message);
    }

    /// <summary>
    /// Defines the test method ValidateInvalidStringToDateTime.
    /// </summary>
    /// <param name="input">The input.</param>
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

    /// <summary>
    /// Defines the test method TryToDateTime_EmptyInput_ReturnsFalse.
    /// </summary>
    [Fact]
    public void TryToDateTimeEmptyInputReturnsFalse()
    {
        var result = string.Empty.TryToDateTime(out _);
        Assert.False(result);
    }

    /// <summary>
    /// Defines the test method ToInt32_EmptyInput_ReturnsZero.
    /// </summary>
    [Fact]
    public void ToInt32EmptyInputReturnsZero()
    {
        var result = string.Empty.ToInt32();
        Assert.Equal(0, result);
    }

    /// <summary>
    /// Defines the test method ToInt32_InvalidInput_ReturnsZero.
    /// </summary>
    [Fact]
    public void ToInt32InvalidInputReturnsZero()
    {
        var result = "some-string".ToInt32();
        Assert.Equal(0, result);
    }

    /// <summary>
    /// Defines the test method ToInt64_EmptyInput_ReturnsZero.
    /// </summary>
    [Fact]
    public void ToInt64EmptyInputReturnsZero()
    {
        var result = string.Empty.ToInt64();
        Assert.Equal(0, result);
    }

    /// <summary>
    /// Defines the test method ToInt64_InvalidInput_ReturnsZero.
    /// </summary>
    [Fact]
    public void ToInt64InvalidInputReturnsZero()
    {
        var result = "some-string".ToInt64();
        Assert.Equal(0, result);
    }

    /// <summary>
    /// Defines the test method ToDecimal_EmptyInput_ReturnsZero.
    /// </summary>
    [Fact]
    public void ToDecimalEmptyInputReturnsZero()
    {
        var result = string.Empty.ToDecimal();
        Assert.Equal(0, result);
    }

    /// <summary>
    /// Defines the test method ToDecimal_InvalidInput_ReturnsZero.
    /// </summary>
    [Fact]
    public void ToDecimalInvalidInputReturnsZero()
    {
        var result = "some-string".ToDecimal();
        Assert.Equal(0, result);
    }

    /// <summary>
    /// Defines the test method ToMonetary_ValidInput_ReturnsValidString.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="expected">The expected.</param>
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
