using System;
using System.Text.RegularExpressions;
using CrispyWaffle.Extensions;
using CrispyWaffle.GoodPractices;
using CrispyWaffle.Utilities;

namespace CrispyWaffle.I18n.PtBr;

/// <summary>
/// Class ConversionExtensions.
/// </summary>
public static class ConversionExtensions
{
    /// <summary>
    /// Parses the brazilian phone number.
    /// </summary>
    /// <param name="number">The number.</param>
    /// <returns>PhoneNumber.</returns>
    /// <exception cref="InvalidTelephoneNumberException">Invalid telephone number format exception.</exception>
    public static PhoneNumber ParseBrazilianPhoneNumber(this string number)
    {
        var result = new PhoneNumber(0, 0, 0);

        if (number.TryParseBrazilianPhoneNumber(ref result))
        {
            return result;
        }

        throw new InvalidTelephoneNumberException(number.RemoveNonNumeric());
    }

    /// <summary>
    /// Tries the parse brazilian phone number.
    /// </summary>
    /// <param name="number">The number.</param>
    /// <param name="result">The result.</param>
    /// <returns><c>true</c> if is valid phone number, <c>false</c> otherwise.</returns>
    public static bool TryParseBrazilianPhoneNumber(this string number, ref PhoneNumber result)
    {
        var dirty = number.RemoveNonNumeric();
        var dirtyLength = dirty.Length;

        if (
            dirty.StartsWith("55", StringComparison.OrdinalIgnoreCase)
            && dirtyLength is > 11 and < 15
        )
        {
            dirty = dirty.Remove(0, 2);
            dirtyLength -= 2;
        }

        if (dirtyLength < 10 || dirtyLength > 12)
        {
            return false;
        }

        var prefix =
            dirty.Substring(0, 1).Equals(@"0", StringComparison.OrdinalIgnoreCase)
            && (dirtyLength == 11 || dirtyLength == 12)
                ? dirty.Substring(1, 2)
                : dirty.Substring(0, 2);

        var hasNineDigits = dirty
            .Substring(dirtyLength - 9, 1)
            .Equals(@"9", StringComparison.OrdinalIgnoreCase);

        var allowedDigits = hasNineDigits ? 9 : 8;

        var telephoneNumber = dirty.Substring(dirtyLength - allowedDigits, allowedDigits);

        result = new PhoneNumber(55, prefix.ToInt32(), telephoneNumber.ToInt64());

        return true;
    }

    /// <summary>
    /// Formats the document.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <returns>System.String.</returns>
    public static string FormatBrazilianDocument(this string document)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            return "Invalid document";
        }

        var documentPattern =
            document.Length == 14 ? @"{0:00\.000\.000/0000-00}" : @"{0:000\.000\.000-00}";
        return string.Format(documentPattern, document.RemoveNonNumeric().ToInt64());
    }

    /// <summary>
    /// Formats the zip code.
    /// </summary>
    /// <param name="zipCode">The zip code.</param>
    /// <returns>System.String.</returns>
    public static string FormatBrazilianZipCode(this string zipCode)
    {
        if (string.IsNullOrWhiteSpace(zipCode))
        {
            return "Invalid zipcode";
        }

        return Regex.Replace(
            zipCode.RemoveNonNumeric(),
            @"(\d{5})(\d{3})",
            "$1-$2",
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(10)
        );
    }
}
