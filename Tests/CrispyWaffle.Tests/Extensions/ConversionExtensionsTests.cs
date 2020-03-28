using CrispyWaffle.Extensions;
using Xunit;

namespace CrispyWaffle.Tests.Extensions
{
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
    }
}
