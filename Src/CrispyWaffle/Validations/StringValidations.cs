using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CrispyWaffle.Validations;

/// <summary>
/// Provides string validations for common patterns.
/// </summary>
public static class StringValidations
{
    /// <summary>
    /// The invalid path characters, escaped for use in regex.
    /// </summary>
    private static readonly string _invalidPathChars = Regex.Escape(
        new string(Path.GetInvalidFileNameChars())
    );

    /// <summary>
    /// The Portuguese preposition pattern.
    /// </summary>
    public static readonly Regex PortuguesePrepositionPattern =
        new(
            "^(da|de|do|das|dos|no|na|nos|nas|-|etapa)$",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(5)
        );

    /// <summary>
    /// The parentheses pattern (matches any content inside parentheses).
    /// </summary>
    public static readonly Regex ParenthesesPattern =
        new(
            @"\((.+?)\)",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(5)
        );

    /// <summary>
    /// The non-alphanumeric pattern (matches characters that are not letters, digits, or .@-).
    /// </summary>
    public static readonly Regex NonAlphanumericPattern =
        new(
            @"[^\w\.@-]",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(5)
        );

    /// <summary>
    /// The non-numeric pattern (matches any character that is not a digit).
    /// </summary>
    public static readonly Regex NonNumericPattern =
        new(
            "[^0-9]",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(5)
        );

    /// <summary>
    /// The spaces pattern (matches any space character).
    /// </summary>
    public static readonly Regex SpacesPattern =
        new(
            @"\s+",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(5)
        );

    /// <summary>
    /// The multiple spaces pattern (matches sequences of two or more spaces or tabs).
    /// </summary>
    public static readonly Regex MultipleSpacesPattern =
        new(
            @"[\t|\s]{2,}",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(5)
        );

    /// <summary>
    /// The invalid file name pattern (matches invalid file name characters).
    /// </summary>
    public static readonly Regex InvalidFileName =
        new(
            $@"([{_invalidPathChars}]*\.+$)|([{_invalidPathChars}]+)",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(5)
        );
}
