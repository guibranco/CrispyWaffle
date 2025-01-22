using CrispyWaffle.Extensions;
using Xunit;

namespace CrispyWaffle.Tests.Extensions;

public class EnumExtensionsTests
{
    [Fact]
    public void ValidateGetHumanReadableValue()
    {
        var enumItem = TestEnum.One;

        Assert.Equal("Um", enumItem.GetHumanReadableValue());

        Assert.Equal("Dois", TestEnum.Two.GetHumanReadableValue());
    }

    [Fact]
    public void ValidateGetEnumByHumanReadableValue()
    {
        Assert.Equal(
            TestEnum.Three,
            EnumExtensions.GetEnumByHumanReadableAttribute<TestEnum>("Três")
        );
        Assert.Equal(
            TestEnum.Three,
            EnumExtensions.GetEnumByHumanReadableAttribute<TestEnum>("três")
        );
    }

    [Fact]
    public void ValidateGetInternalValue()
    {
        var enumItem = TestEnum.One;

        Assert.Equal("один", enumItem.GetInternalValue());

        Assert.Equal("два", TestEnum.Two.GetInternalValue());
    }

    [Fact]
    public void ValidateGetEnumByInternalValue()
    {
        Assert.Equal(
            TestEnum.Three,
            EnumExtensions.GetEnumByInternalValueAttribute<TestEnum>("три")
        );
    }
}
