using CrispyWaffle.Extensions;
using Xunit;

namespace CrispyWaffle.Tests.Extensions;

public class MathExtensionsTests
{
    [Fact]
    public void ValidateRoundDown()
    {
        var result = 10.RoundDown();
        Assert.Equal(10, result);

        result = 11.RoundDown();
        Assert.Equal(10, result);

        result = 15.RoundDown();
        Assert.Equal(10, result);

        result = 19.RoundDown();
        Assert.Equal(10, result);

        result = 109.RoundDown();
        Assert.Equal(100, result);
    }

    [Fact]
    public void ValidateRoundDownMultipleOf100()
    {
        var result = 100.RoundDown(100);
        Assert.Equal(100, result);

        result = 110.RoundDown(100);
        Assert.Equal(100, result);

        result = 150.RoundDown(100);
        Assert.Equal(100, result);

        result = 190.RoundDown(100);
        Assert.Equal(100, result);

        result = 109.RoundDown(100);
        Assert.Equal(100, result);
    }

    [Fact]
    public void ValidateRoundUp()
    {
        var result = 10.RoundUp();
        Assert.Equal(10, result);

        result = 11.RoundUp();
        Assert.Equal(20, result);

        result = 15.RoundUp();
        Assert.Equal(20, result);

        result = 19.RoundUp();
        Assert.Equal(20, result);

        result = 109.RoundUp();
        Assert.Equal(110, result);
    }

    [Fact]
    public void ValidateRoundUpMultipleOf100()
    {
        var result = 100.RoundUp(100);
        Assert.Equal(100, result);

        result = 110.RoundUp(100);
        Assert.Equal(200, result);

        result = 150.RoundUp(100);
        Assert.Equal(200, result);

        result = 190.RoundUp(100);
        Assert.Equal(200, result);

        result = 109.RoundUp(100);
        Assert.Equal(200, result);
    }

    [Fact]
    public void ValidateRoundBest()
    {
        var result = 10.RoundBest();
        Assert.Equal(10, result);

        result = 11.RoundBest();
        Assert.Equal(10, result);

        result = 15.RoundBest();
        Assert.Equal(20, result);

        result = 19.RoundBest();
        Assert.Equal(20, result);

        result = 109.RoundBest();
        Assert.Equal(110, result);
    }

    [Fact]
    public void ValidateRoundBestMultipleOf100()
    {
        var result = 100.RoundBest(100);
        Assert.Equal(100, result);

        result = 110.RoundBest(100);
        Assert.Equal(100, result);

        result = 150.RoundBest(100);
        Assert.Equal(200, result);

        result = 190.RoundBest(100);
        Assert.Equal(200, result);

        result = 109.RoundBest(100);
        Assert.Equal(100, result);
    }
}
