namespace CrispyWaffle.Validations
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Class exception validations.
    /// </summary>
    public static class ExceptionValidations
    {
        /// <summary>
        /// The stack trace pattern regex for capture multiple stack traces
        /// </summary>
        public static readonly Regex StackTracePattern =
            new Regex(
                @"^\s+(?:at|em)\s(?<method>.+?)\s(?:in|na)\s(?<path>.+?)\:(?:line|linha)\s(?<line>\d+)(?:\s*|\r\n)?$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);
    }
}
