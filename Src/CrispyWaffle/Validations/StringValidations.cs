using System;

namespace CrispyWaffle.Validations
{
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides strings validations for commons patterns.
    /// </summary>
    public static class StringValidations
    {
        /// <summary>
        /// The invalid path chars
        /// </summary>
        private static readonly string _invalidPathChars = Regex.Escape(
            new string(Path.GetInvalidFileNameChars())
        );

        /// <summary>
        /// The portuguese preposition pattern
        /// </summary>
        public static readonly Regex PortuguesePrepositionPattern = new("^(da|de|do|das|dos|no|na|nos|nas|-|etapa)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5));

        /// <summary>
        /// The parentheses pattern
        /// </summary>
        public static readonly Regex ParenthesesPattern = new(@"\((.+?)\)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5));

        /// <summary>
        /// The non alphanumeric pattern
        /// </summary>
        public static readonly Regex NonAlphanumericPattern = new(@"[^\w\.@-]", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5));

        /// <summary>
        /// The non numeric pattern
        /// </summary>
        public static readonly Regex NonNumericPattern = new("[^0-9]", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5));

        /// <summary>
        /// The spaces pattern
        /// </summary>
        public static readonly Regex SpacesPattern = new(@"\s+", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5));

        /// <summary>
        /// The multiple spaces pattern
        /// </summary>
        public static readonly Regex MultipleSpacesPattern = new(@"[\t|\s]{2,}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5));

        /// <summary>
        /// The invalid file name
        /// </summary>
        public static readonly Regex InvalidFileName = new($@"([{_invalidPathChars}]*\.+$)|([{_invalidPathChars}]+)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5));
    }
}
