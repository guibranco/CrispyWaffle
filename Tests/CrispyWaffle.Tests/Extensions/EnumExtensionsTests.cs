﻿using CrispyWaffle.Extensions;
using Xunit;

namespace CrispyWaffle.Tests.Extensions;

/// <summary>
/// Class EnumExtensionsTests.
/// </summary>
public class EnumExtensionsTests
{
    /// <summary>
    /// Defines the test method ValidateGetHumanReadableValue.
    /// </summary>
    [Fact]
    public void ValidateGetHumanReadableValue()
    {
        var enumItem = TestEnum.One;

        Assert.Equal("Um", enumItem.GetHumanReadableValue());

        Assert.Equal("Dois", TestEnum.Two.GetHumanReadableValue());
    }

    /// <summary>
    /// Defines the test method ValidateGetEnumByHumanReadableValue.
    /// </summary>
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

    /// <summary>
    /// Defines the test method ValidateGetInternalValue.
    /// </summary>
    [Fact]
    public void ValidateGetInternalValue()
    {
        var enumItem = TestEnum.One;

        Assert.Equal("один", enumItem.GetInternalValue());

        Assert.Equal("два", TestEnum.Two.GetInternalValue());
    }

    /// <summary>
    /// Defines the test method ValidateGetEnumByInternalValue.
    /// </summary>
    [Fact]
    public void ValidateGetEnumByInternalValue()
    {
        Assert.Equal(
            TestEnum.Three,
            EnumExtensions.GetEnumByInternalValueAttribute<TestEnum>("три")
        );
    }
}
