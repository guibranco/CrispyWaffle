using CrispyWaffle.Attributes;

namespace CrispyWaffle.Tests.Extensions;

public enum TestEnum
{
    [InternalValue("ноль")]
    [HumanReadable("Zero")]
    None = 0,

    [InternalValue("один")]
    [HumanReadable("Um")]
    One = 1,

    [InternalValue("два")]
    [HumanReadable("Dois")]
    Two = 2,

    [InternalValue("три")]
    [HumanReadable("Três")]
    Three = 3,
}
