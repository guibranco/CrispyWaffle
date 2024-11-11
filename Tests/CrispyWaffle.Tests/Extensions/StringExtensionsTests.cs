using System.Text;
using CrispyWaffle.Extensions;
using Xunit;

namespace CrispyWaffle.Tests.Extensions;

public class StringExtensionsTests
{
    [Fact]
    public void ValidateEncodeBase64()
    {
        const string input = "1 2 3 input string";
        const string expected = "MSAyIDMgaW5wdXQgc3RyaW5n";

        var result = input.ToBase64();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ValidateDecodeBase64()
    {
        const string input = "MSAyIDMgaW5wdXQgc3RyaW5n";
        const string expected = "1 2 3 input string";

        var result = input.FromBase64();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ValidateBase64Checks()
    {
        const string empty = "";

        var result = empty.ToBase64();

        Assert.Equal(string.Empty, result);

        result = empty.FromBase64();

        Assert.Equal(string.Empty, result);
    }

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

    [Fact]
    public void ValidateLevenshteinZero()
    {
        const string word = "Validate Levenshtein Zero";

        var result = word.Levenshtein(word);

        Assert.Equal(0, result);
    }

    [Fact]
    public void ValidateLevenshteinCaseSensitive()
    {
        const string three = "Three";
        const string tree = "tree";

        var result = three.Levenshtein(tree);

        Assert.Equal(2, result);
    }

    [Fact]
    public void ValidateLevenshteinInvariantCulture()
    {
        const string three = "Three";
        const string tree = "tree";

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var result = three.LevenshteinInvariantCulture(tree);

        Assert.Equal(1, result);
    }

    [Fact]
    public void ValidateRemoveSpaces()
    {
        const string input = "Some string with spaces in the middle";
        const string expected = "Somestringwithspacesinthemiddle";

        var result = input.RemoveSpaces();

        Assert.Equal(expected, result);
    }

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

    [Fact]
    public void ValidateValidJson()
    {
        const string validJson = "{ key: 'value', otherKey: 'some value', }";

        var result = validJson.IsValidJson();

        Assert.True(result);
    }

    [Fact]
    public void ValidateInvalidJsonFormat()
    {
        const string invalidJson = "some string";

        var result = invalidJson.IsValidJson();

        Assert.False(result);
    }

    [Fact]
    public void ValidateInvalidJson()
    {
        const string invalidJson = "{ key = error }";

        var result = invalidJson.IsValidJson();

        Assert.False(result);
    }
}
