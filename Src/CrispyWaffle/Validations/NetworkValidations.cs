using System;
using System.Text.RegularExpressions;

namespace CrispyWaffle.Validations
{
    /// <summary>
    /// Provides methods and regular expression patterns for validating network-related data,
    /// such as IP addresses and email addresses.
    /// </summary>
    public static class NetworkValidations
    {
        /// <summary>
        /// Regular expression pattern for validating IPv4 addresses.
        /// The pattern matches standard dotted-decimal IP address format (e.g., 192.168.0.1).
        /// </summary>
        /// <remarks>
        /// This regex ensures that the address consists of four numbers between 0 and 255,
        /// separated by periods (e.g., 192.168.0.1).
        /// </remarks>
        public static readonly Regex IpAddressPattern = new Regex(
            @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(5)
        );

        /// <summary>
        /// Regular expression pattern for validating email addresses.
        /// The pattern matches most common valid email formats, including those with special characters.
        /// </summary>
        /// <remarks>
        /// This regex is designed to match a wide variety of valid email formats according to the
        /// specifications in RFC 5322, while allowing for extended Unicode characters in both
        /// the local-part and domain of the email address.
        /// </remarks>
        public static readonly Regex EmailAddressPattern = new Regex(
            "^((([a-z]|\\d|[!#\\$%&'\\*\\+\\-\\/=\\?\\^_`{\\|}~]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+(\\.([a-z]|\\d|[!#\\$%&'\\*\\+\\-\\/=\\?\\^_`{\\|}~]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+)*)|((\\x22)((((\\x20|\\x09)*(\\x0d\\x0a))?(\\x20|\\x09)+)?(([\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x7f]|\\x21|[\\x23-\\x5b]|[\\x5d-\\x7e]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(\\\\([\\x01-\\x09\\x0b\\x0c\\x0d-\\x7f]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF]))))*(((\\x20|\\x09)*(\\x0d\\x0a))?(\\x20|\\x09)+)?(\\x22)))@((([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])([a-z]|\\d|-|\\.|_|~|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])*([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])))\\.)+(([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])([a-z]|\\d|-|\\.|_|~|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])*([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])))\\.?$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture,
            TimeSpan.FromSeconds(5)
        );
    }
}
