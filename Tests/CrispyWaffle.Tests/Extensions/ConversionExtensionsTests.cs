// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 07-29-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 07-29-2020
// ***********************************************************************
// <copyright file="ConversionExtensionsTests.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace CrispyWaffle.Tests.Extensions
{
    using CrispyWaffle.Extensions;
    using CrispyWaffle.GoodPractices;
    using System;
    using Xunit;

    /// <summary>
    /// Class ConversionExtensionsTests.
    /// </summary>
    public class ConversionExtensionsTests
    {
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
        /// Defines the test method ValidateParseBrazilianPhoneNumber.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="countryCode">The country code.</param>
        /// <param name="regionCode">The region code.</param>
        /// <param name="number">The number.</param>
        /// <param name="isNinthDigit">if set to <c>true</c> [is ninth digit].</param>
        [Theory]
        [InlineData("5511987654321", 55, 11, 987654321, true)]
        [InlineData("551187654321", 55, 11, 87654321, false)]
        [InlineData("11987654321", 55, 11, 987654321, true)]
        [InlineData("1187654321", 55, 11, 87654321, false)]
        public void ValidateParseBrazilianPhoneNumber(string phoneNumber, int countryCode, int regionCode, long number, bool isNinthDigit)
        {
            var result = phoneNumber.ParseBrazilianPhoneNumber();

            Assert.Equal(countryCode, result.CountryCode);
            Assert.Equal(regionCode, result.RegionCode);
            Assert.Equal(number, result.Number);
            Assert.Equal(isNinthDigit, result.IsNinthDigit);
        }

        [Theory]
        [InlineData("0", "0")]
        [InlineData("123456789123456789", "123456789123456789")]
        [InlineData("abc123def456", "123456")]
        [InlineData("190", "190")]
        public void ValidateInvalidParseBrazilianPhoneNumber(string phoneNumber, string cleanPhoneNumber)
        {

            var result = Assert.Throws<InvalidTelephoneNumberException>(() => phoneNumber.ParseBrazilianPhoneNumber());

            Assert.Equal($"The value '{cleanPhoneNumber}' isn't a valid telephone number", result.Message);
        }

        [Fact]
        public void ValidateNowToDateTime()
        {
            var date = DateTime.Now;

            var result = "now".ToDateTime();

            Assert.Equal(date.Date, result.Date);
            Assert.Equal(date.Hour, result.Hour);
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
        public void ValidateStringDateToDateTime(string input, int year, int month, int day, int hour, int minute, int seconds)
        {
            var expected = new DateTime(year, month, day, hour, minute, seconds);

            var result = input.ToDateTime();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ValidateEmptyStringToDateTime()
        {

            var result = Assert.Throws<ArgumentNullException>(() => string.Empty.ToDateTime());

            Assert.Equal("Input value cannot be null (Parameter 'input')", result.Message);
        }

        [Theory]
        [InlineData("1234567890")]
        public void ValidateInvalidStringToDateTime(string input)
        {
            var result = Assert.Throws<ArgumentOutOfRangeException>(() => input.ToDateTime());

            Assert.Equal($"Unable to parse the string to a valid datetime (Parameter 'input')\r\nActual value was {input}.", result.Message);
        }

        [Theory]
        [InlineData("12345678900", "123.456.789-00")]
        [InlineData("12345678901234", "12.345.678/9012-34")]
        public void ValidateFormatBrazilianDocument(string input, string output)
        {

            var result = input.FormatBrazilianDocument();

            Assert.Equal(output, result);
        }

        [Fact]
        public void ValidateInvalidFormatBrazilianDocument()
        {

            var result = string.Empty.FormatBrazilianDocument();

            Assert.Equal("Invalid document", result);
        }
    }
}
