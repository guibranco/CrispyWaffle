// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 06-07-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 07-29-2020
// ***********************************************************************
// <copyright file="StringExtensionsTests.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ************************************************************************
using CrispyWaffle.Extensions;
using System.Text;
using Xunit;

namespace CrispyWaffle.Tests.Extensions
{
    /// <summary>
    /// Class StringExtensionsTests.
    /// </summary>
    public class StringExtensionsTests
    {
        /// <summary>
        /// Defines the test method ValidateEncodeBase64.
        /// </summary>
        [Fact]
        public void ValidateEncodeBase64()
        {
            const string input = "1 2 3 input string";
            const string expected = "MSAyIDMgaW5wdXQgc3RyaW5n";

            var result = input.ToBase64();

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Defines the test method ValidateDecodeBase64.
        /// </summary>
        [Fact]
        public void ValidateDecodeBase64()
        {
            const string input = "MSAyIDMgaW5wdXQgc3RyaW5n";
            const string expected = "1 2 3 input string";

            var result = input.FromBase64();

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Defines the test method ValidateBase64Checks.
        /// </summary>
        [Fact]
        public void ValidateBase64Checks()
        {

            const string empty = "";

            var result = empty.ToBase64();

            Assert.Equal(string.Empty, result);

            result = empty.FromBase64();

            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Defines the test method ValidateLevenshteinEmpty.
        /// </summary>
        [Fact]
        public void ValidateLevenshteinEmpty()
        {
            const string empty = "";
            const string different = "different";

            var result = empty.Levenshtein(different);

            Assert.Equal(different.Length, result);

            result = different.Levenshtein(empty);

            Assert.Equal(different.Length, result);
        }


        /// <summary>
        /// Defines the test method ValidateLevenshteinZero.
        /// </summary>
        [Fact]
        public void ValidateLevenshteinZero()
        {
            const string word = "Validate Levenshtein Zero";

            var result = word.Levenshtein(word);

            Assert.Equal(0, result);
        }

        /// <summary>
        /// Defines the test method ValidateLevenshteinCaseSensitive.
        /// </summary>
        [Fact]
        public void ValidateLevenshteinCaseSensitive()
        {
            const string three = "Three";
            const string tree = "tree";

            var result = three.Levenshtein(tree);

            Assert.Equal(2, result);
        }

        /// <summary>
        /// Defines the test method ValidateLevenshteinInvariantCulture.
        /// </summary>
        [Fact]
        public void ValidateLevenshteinInvariantCulture()
        {
            const string three = "Three";
            const string tree = "tree";

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var result = three.LevenshteinInvariantCulture(tree);

            Assert.Equal(1, result);
        }


        /// <summary>
        /// Defines the test method ValidateRemoveSpaces.
        /// </summary>
        [Fact]
        public void ValidateRemoveSpaces()
        {
            const string input = "Some string with spaces in the middle";
            const string expected = "Somestringwithspacesinthemiddle";

            var result = input.RemoveSpaces();

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Defines the test method ValidateToCamelCase.
        /// </summary>
        [Fact]
        public void ValidateToCamelCase()
        {
            const string input = "republica federativa do brasil";
            const string expected = "Republica Federativa Do Brasil";


            var result = input.ToCamelCase();

            Assert.Equal(expected, result);

            result = string.Empty.ToCamelCase();

            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Defines the test method ValidateValidJson.
        /// </summary>
        [Fact]
        public void ValidateValidJson()
        {
            const string validJson = "{ key: 'value', otherKey: 'some value', }";

            var result = validJson.IsValidJson();

            Assert.True(result);
        }

        /// <summary>
        /// Defines the test method ValidateInvalidJsonFormat.
        /// </summary>
        [Fact]
        public void ValidateInvalidJsonFormat()
        {
            const string invalidJson = "some string";

            var result = invalidJson.IsValidJson();

            Assert.False(result);
        }

        /// <summary>
        /// Defines the test method ValidateInvalidJson.
        /// </summary>
        [Fact]
        public void ValidateInvalidJson()
        {
            const string invalidJson = "{ key = error }";

            var result = invalidJson.IsValidJson();

            Assert.False(result);
        }

    }
}
