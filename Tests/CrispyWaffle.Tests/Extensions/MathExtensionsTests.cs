// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 07-29-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 07-29-2020
// ***********************************************************************
// <copyright file="MathExtensionsTests.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using CrispyWaffle.Extensions;
using Xunit;

namespace CrispyWaffle.Tests.Extensions
{
    /// <summary>
    /// Class MathExtensionsTests.
    /// </summary>
    public class MathExtensionsTests
    {
        /// <summary>
        /// Validates the round down.
        /// </summary>
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

        /// <summary>
        /// Validates the round down multiple of100.
        /// </summary>
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

        /// <summary>
        /// Validates the round up.
        /// </summary>
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

        /// <summary>
        /// Validates the round up multiple of100.
        /// </summary>
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

        /// <summary>
        /// Validates the round best.
        /// </summary>
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

        /// <summary>
        /// Validates the round best multiple of100.
        /// </summary>
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
}
