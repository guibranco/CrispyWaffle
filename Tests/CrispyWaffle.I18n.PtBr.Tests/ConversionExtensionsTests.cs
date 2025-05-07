using System.Globalization;
using System.Threading;
using CrispyWaffle.GoodPractices;
using Xunit;

namespace CrispyWaffle.I18n.PtBr.Tests;

public class ConversionExtensionsTests
{
    public ConversionExtensionsTests()
    {
        var currentCulture = CultureInfo.GetCultureInfo("pt-Br").Name;
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

    [Theory]
    [InlineData("5511987654321", 55, 11, 987654321, true)]
    [InlineData("551187654321", 55, 11, 87654321, false)]
    [InlineData("11987654321", 55, 11, 987654321, true)]
    [InlineData("1187654321", 55, 11, 87654321, false)]
    public void ValidateParseBrazilianPhoneNumber(
        string phoneNumber,
        int countryCode,
        int regionCode,
        long number,
        bool isNinthDigit
    )
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
    public void ValidateInvalidParseBrazilianPhoneNumber(
        string phoneNumber,
        string cleanPhoneNumber
    )
    {
        var result = Assert.Throws<InvalidTelephoneNumberException>(() =>
            phoneNumber.ParseBrazilianPhoneNumber()
        );

        Assert.Equal(
            $"The value '{cleanPhoneNumber}' isn't a valid telephone number.",
            result.Message
        );
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
