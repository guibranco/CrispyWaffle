// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 09-06-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-06-2020
// ***********************************************************************
// <copyright file="EnumExtensionsTests.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using CrispyWaffle.Extensions;
using Xunit;

namespace CrispyWaffle.Tests.Extensions
{

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
            var enumItem = TestEnum.ONE;

            Assert.Equal("Um", enumItem.GetHumanReadableValue());

            Assert.Equal("Dois", TestEnum.TWO.GetHumanReadableValue());
        }

        /// <summary>
        /// Defines the test method ValidateGetEnumByHumanReadableValue.
        /// </summary>
        [Fact]
        public void ValidateGetEnumByHumanReadableValue()
        {
            Assert.Equal(TestEnum.THREE, EnumExtensions.GetEnumByHumanReadableAttribute<TestEnum>("Três"));
            Assert.Equal(TestEnum.THREE, EnumExtensions.GetEnumByHumanReadableAttribute<TestEnum>("três"));
        }

        /// <summary>
        /// Defines the test method ValidateGetInternalValue.
        /// </summary>
        [Fact]
        public void ValidateGetInternalValue()
        {
            var enumItem = TestEnum.ONE;

            Assert.Equal("один", enumItem.GetInternalValue());

            Assert.Equal("два", TestEnum.TWO.GetInternalValue());
        }

        /// <summary>
        /// Defines the test method ValidateGetEnumByInternalValue.
        /// </summary>
        [Fact]
        public void ValidateGetEnumByInternalValue()
        {
            Assert.Equal(TestEnum.THREE, EnumExtensions.GetEnumByInternalValueAttribute<TestEnum>("три"));
        }
    }
}
