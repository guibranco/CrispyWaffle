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
        private static readonly string _invalidPathChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));

        /// <summary>
        /// The portuguese preposition pattern
        /// </summary>
        public static readonly Regex PortuguesePrepositionPattern = new Regex("^(da|de|do|das|dos|no|na|nos|nas|-|etapa)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// The parentheses pattern
        /// </summary>
        public static readonly Regex ParenthesesPattern = new Regex(@"\((.+?)\)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// The non alphanumeric pattern
        /// </summary>
        public static readonly Regex NonAlphanumericPattern = new Regex(@"[^\w\.@-]", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// The non numeric pattern
        /// </summary>
        public static readonly Regex NonNumericPattern = new Regex("[^0-9]", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        /// <summary>
        /// The spaces pattern
        /// </summary>
        public static readonly Regex SpacesPattern = new Regex(@"\s+", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        /// <summary>
        /// The multiple spaces pattern
        /// </summary>
        public static readonly Regex MultipleSpacesPattern = new Regex(@"[\t|\s]{2,}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// The invalid file name
        /// </summary>
        public static readonly Regex InvalidFileName = new Regex($@"([{_invalidPathChars}]*\.+$)|([{_invalidPathChars}]+)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    }
}
