using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CrispyWaffle.Validations;
using Newtonsoft.Json.Linq;

namespace CrispyWaffle.Extensions
{
    /// <summary>
    /// Class StringExtensions.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Replaces the non-alphanumeric.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="replaceWith">The replacement text.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string ReplaceNonAlphanumeric(this string input, string replaceWith)
        {
            return string.IsNullOrWhiteSpace(input)
                ? input
                : StringValidations.NonAlphanumericPattern.Replace(input, replaceWith);
        }

        /// <summary>
        /// Replaces the first.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="search">The search.</param>
        /// <param name="replaceWith">The replacement text.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string ReplaceFirst(this string input, string search, string replaceWith)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            var pos = input.IndexOf(search, StringComparison.Ordinal);
            return pos < 0
                ? input
                : input.Substring(0, pos) + replaceWith + input.Substring(pos + search.Length);
        }

        /// <summary>
        /// Removes the non numeric.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string RemoveNonNumeric(this string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? input
                : StringValidations.NonNumericPattern.Replace(input, string.Empty);
        }

        /// <summary>
        /// Removes the diacritics.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string RemoveDiacritics(this string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? input
                : Encoding.ASCII.GetString(Encoding.GetEncoding(1251).GetBytes(input));
        }

        /// <summary>
        /// Removes the spaces.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string RemoveSpaces(this string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? input
                : StringValidations.SpacesPattern.Replace(input, string.Empty);
        }

        /// <summary>
        /// Removes the excess spaces.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string RemoveExcessSpaces(this string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? input
                : StringValidations.MultipleSpacesPattern.Replace(input, " ");
        }

        /// <summary>
        /// Removes the non-alphanumeric.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string RemoveNonAlphanumeric(this string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? input
                : StringValidations.NonAlphanumericPattern.Replace(input, string.Empty);
        }

        /// <summary>
        /// Abbreviates the specified maximum characters.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="maxCharacters">The maximum characters.</param>
        /// <param name="addEllipsis">if set to <c>true</c> [add ellipsis].</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string Abbreviate(
            this string input,
            int maxCharacters,
            bool addEllipsis = true
        )
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            if (input.Length < maxCharacters)
            {
                return input;
            }

            return string.Concat(
                input.Substring(0, addEllipsis ? maxCharacters - 4 : maxCharacters),
                addEllipsis ? "..." : string.Empty
            );
        }

        /// <summary>
        /// Converts to camelcase.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string ToCamelCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var words = input.Trim().Split(' ');
            var sb = new StringBuilder();
            foreach (var w in words.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                var c = w.ToLower().ToCharArray();
                c[0] = char.ToUpper(c[0]);
                sb.Append(c).Append(' ');
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Converts to uppercase each word in the text.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string ToUcWords(this string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? string.Empty
                : Regex.Replace(
                    input.ToLower(),
                    @"(?:^|\s|/|[0-9])[a-z]",
                    m => m.Value.ToUpperInvariant(),
                    RegexOptions.Compiled,
                    TimeSpan.FromSeconds(5)
                );
        }

        /// <summary>
        /// Converts to uppercase each word in the text.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="toUpper">To upper.</param>
        /// <param name="toLower">To lower.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string UcWords(this string[] input, string[] toUpper, string[] toLower)
        {
            var result = new StringBuilder();

            if (input.Length == 0)
            {
                return string.Empty;
            }

            foreach (var s in input)
            {
                result.Append(' ');

                if (toUpper.Contains(s.ToUpper()))
                {
                    result.Append(s.ToUpper());
                }
                else if (toLower.Contains(s.ToLower()))
                {
                    result.Append(s.ToLower());
                }
                else
                {
                    result.Append(s.ToCamelCase());
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Converts to safe-url.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string ToSafeUrl(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            var bytes = Encoding.GetEncoding("ISO-8859-8").GetBytes(input);
            return Encoding
                .UTF8.GetString(bytes)
                .Replace("-", string.Empty)
                .Replace(".", string.Empty)
                .RemoveExcessSpaces()
                .Trim()
                .ReplaceNonAlphanumeric("-")
                .Trim('-');
        }

        /// <summary>
        /// Converts to base64.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string ToBase64(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var utf8Bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(utf8Bytes);
        }

        /// <summary>
        /// Decode text from base 64.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="encoding">The encoding. Default is ISO-8859-1.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string FromBase64(this string input, string encoding = "ISO-8859-1")
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var base64Bytes = Convert.FromBase64String(input);
            return Encoding.GetEncoding(encoding).GetString(base64Bytes);
        }

        /// <summary>
        /// Convert text from ISO-8859-1 to UTF-8.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string FromISO2UTF8(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var iso = Encoding.GetEncoding("ISO-8859-1");
            var utf = Encoding.UTF8;

            var bytes = iso.GetBytes(input);
            var utf8Bytes = Encoding.Convert(iso, utf, bytes);
            return utf.GetString(utf8Bytes);
        }

        /// <summary>
        /// Converts to valid filename.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string ToValidFileName(this string fileName) =>
            StringValidations.InvalidFileName.Replace(fileName, "_");

        /// <summary>
        /// Calculates the Levenshtein distance between two strings.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="inputToCompare">The input to compare.</param>
        /// <returns>System.Int32.</returns>
        [Pure]
        public static int Levenshtein(this string input, string inputToCompare)
        {
            var n = input.Length;

            var m = inputToCompare.Length;

            var d = new int[n + 1, m + 1];

            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            for (var i = 0; i <= n; i++)
            {
                d[i, 0] = i;
            }

            for (var j = 0; j <= m; j++)
            {
                d[0, j] = j;
            }

            for (var i = 1; i <= n; i++)
            {
                for (var j = 1; j <= m; j++)
                {
                    var cost =
                        inputToCompare.Substring(j - 1, 1) == input.Substring(i - 1, 1) ? 0 : 1;
                    var min = Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1);
                    d[i, j] = Math.Min(min, d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }

        /// <summary>
        /// Calculates the Levenshtein distance between two strings removing diacritics in the string and converting to lowercase.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="inputToCompare">The input to compare.</param>
        /// <returns>System.Int32.</returns>
        [Pure]
        public static int LevenshteinInvariantCulture(this string input, string inputToCompare)
        {
            input = input.RemoveDiacritics().ToLower();
            return input.Levenshtein(inputToCompare.RemoveDiacritics().ToLower());
        }

        /// <summary>
        /// Gets the name of the path or file.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string GetPathOrFileName(this string url)
        {
            var uri = new Uri(url);
            return string.IsNullOrWhiteSpace(uri.GetFileExtension())
                ? uri.AbsolutePath
                : uri.GetFileName();
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string GetFileName(this Uri uri) =>
            string.IsNullOrWhiteSpace(uri.LocalPath)
                ? string.Empty
                : Path.GetFileName(uri.LocalPath);

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string GetFileExtension(this Uri uri) => uri.LocalPath.GetFileExtension();

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string GetFileExtension(this string fileName) =>
            string.IsNullOrWhiteSpace(fileName) ? string.Empty : Path.GetExtension(fileName);

        /// <summary>
        /// Converts to center.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="spacer">The spacer.</param>
        /// <param name="lineSize">Size of the line.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string ToCenter(this string input, char spacer, int lineSize)
        {
            var half = lineSize - (input.Length / 2);
            return $"{new string(spacer, half)}{input}{new string(spacer, input.Length % 2 == 0 ? half : half + 1)}";
        }

        /// <summary>
        /// Determines whether [is valid json] [the specified json raw].
        /// </summary>
        /// <param name="jsonRaw">The json raw.</param>
        /// <returns><c>true</c> if [is valid json] [the specified json raw]; otherwise, <c>false</c>.</returns>
        [Pure]
        public static bool IsValidJson(this string jsonRaw)
        {
            try
            {
                jsonRaw = jsonRaw.Trim();

                var isObject = jsonRaw.StartsWith("{") && jsonRaw.EndsWith("}");
                var isArray = jsonRaw.StartsWith("[") && jsonRaw.EndsWith("]");

                if (!isObject && !isArray)
                {
                    return false;
                }

                JToken.Parse(jsonRaw);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Splits the by.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="chunkLength">Length of the chunk.</param>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        /// <exception cref="ArgumentException">Invalid chuck length - chunkLength.</exception>
        [Pure]
        public static IEnumerable<string> SplitBy(this string str, int chunkLength)
        {
            if (chunkLength < 1)
            {
                throw new ArgumentException("Invalid chuck length", nameof(chunkLength));
            }

            if (string.IsNullOrWhiteSpace(str))
            {
                yield return string.Empty;
                yield break;
            }

            for (var i = 0; i < str.Length; i += chunkLength)
            {
                yield return str.Substring(
                    i,
                    chunkLength + i > str.Length ? str.Length - i : chunkLength
                );
            }
        }

        /// <summary>
        /// Strips the tags.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>System.String.</returns>
        [Pure]
        public static string StripTags(this string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return string.Empty;
            }

            var array = new char[source.Length];
            var arrayIndex = 0;
            var inside = false;

            foreach (var let in source)
            {
                inside = StripTagInternal(let, inside, array, ref arrayIndex);
            }

            return new string(array, 0, arrayIndex);
        }

        /// <summary>
        /// Strips the tag internal.
        /// </summary>
        /// <param name="let">The let.</param>
        /// <param name="inside">if set to <c>true</c> [inside].</param>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        /// <returns><c>true</c> if inside tag, <c>false</c> otherwise.</returns>
        private static bool StripTagInternal(
            char let,
            bool inside,
            char[] array,
            ref int arrayIndex
        )
        {
            switch (let)
            {
                case '<':
                    return true;
                case '>':
                    return false;
                default:
                    if (inside)
                    {
                        return true;
                    }

                    array[arrayIndex] = let;
                    arrayIndex++;

                    break;
            }

            return false;
        }
    }
}
