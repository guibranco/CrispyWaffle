using CrispyWaffle.Utilities;
using Xunit;

namespace CrispyWaffle.Tests.Utilities;

/// <summary>
/// Class PhoneNumberTests.
/// </summary>
public class PhoneNumberTests
{
    /// <summary>
    /// Validates the phone number.
    /// </summary>
    [Fact]
    public void ValidatePhoneNumber()
    {
        const string expectedString = "5511123456789";
        var phone = new PhoneNumber(55, 11, 123456789);

        Assert.Equal(55, phone.CountryCode);
        Assert.Equal(11, phone.RegionCode);
        Assert.Equal(123456789, phone.Number);
        Assert.True(phone.IsNinthDigit);
        Assert.Equal(expectedString, phone.ToString());
    }

    /// <summary>
    /// Validates the fixed phone number.
    /// </summary>
    [Fact]
    public void ValidateFixedPhoneNumber()
    {
        const string expectedString = "551112345678";
        var phone = new PhoneNumber(55, 11, 12345678);

        Assert.Equal(55, phone.CountryCode);
        Assert.Equal(11, phone.RegionCode);
        Assert.Equal(12345678, phone.Number);
        Assert.False(phone.IsNinthDigit);
        Assert.Equal(expectedString, phone.ToString());
    }
}
