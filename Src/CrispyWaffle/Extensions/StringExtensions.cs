namespace CrispyWaffle.Extensions
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Validations;

    /// <summary>
    /// Class StringExtensions.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Replaces the non alphanumeric.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="replaceWith">The replace with.</param>
        /// <returns>String.</returns>
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
        /// <param name="replace">The replace.</param>
        /// <returns>String.</returns>
        [Pure]
        public static string ReplaceFirst(this string input, string search, string replace)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            var pos = input.IndexOf(search, StringComparison.Ordinal);
            return pos < 0 ? input : input.Substring(0, pos) + replace + input.Substring(pos + search.Length);
        }

        /// <summary>
        /// Removes the non numeric.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
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
        /// <returns>String.</returns>
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
        /// <returns>String.</returns>
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
        /// <returns>String.</returns>
        [Pure]
        public static string RemoveExcessSpaces(this string input)
        {
            return string.IsNullOrWhiteSpace(input)
                       ? input
                       : StringValidations.MultipleSpacesPattern.Replace(input, " ");
        }

        /// <summary>
        /// Removes the non alphanumeric.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>String.</returns>

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
        /// <returns></returns>
        [Pure]
        public static string Abbreviate(this string input, int maxCharacters, bool addEllipsis = true)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            if (input.Length < maxCharacters)
            {
                return input;
            }

            return string.Concat(input.Substring(0, addEllipsis ? maxCharacters - 4 : maxCharacters), addEllipsis ? @"..." : string.Empty);
        }

        /// <summary>
        /// To the camel case.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>String.</returns>

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
                sb.Append(c).Append(@" ");
            }
            return sb.ToString().Trim();
        }

        /// <summary>
        /// To the uc words.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>String.</returns>
        [Pure]
        public static string ToUcWords(this string input)
        {
            return string.IsNullOrWhiteSpace(input)
                       ? string.Empty
                       : Regex.Replace(input.ToLower(), @"(?:^|\s|/|[0-9])[a-z]", m => m.Value.ToUpper());
        }

        /// <summary>
        /// Upper case the words.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="toUpper">To upper.</param>
        /// <param name="toLower">To lower.</param>
        /// <returns></returns>
        [Pure]
        public static string UcWords(this string[] input, string[] toUpper, string[] toLower)
        {
            var result = new StringBuilder();

            if (!input.Any())
            {
                return string.Empty;
            }

            foreach (var s in input)
            {
                result.Append(@" ");

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
        /// A String extension method that takes a string safely for inclusion in a Uri.
        /// </summary>
        /// <param name="input">The input String to act on.</param>
        /// <returns>str as a String.</returns>
        [Pure]
        public static string ToSafeUrl(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            var bytes = Encoding.GetEncoding("ISO-8859-8").GetBytes(input);
            return Encoding.UTF8.GetString(bytes).Replace(@"-", string.Empty).Replace(@".", string.Empty).RemoveExcessSpaces().Trim().ReplaceNonAlphanumeric(@"-").Trim('-');

        }


        /// <summary>
        /// Encodes a string to base64 using UTF-8
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
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
        /// Decodes a base64 string using the specified encoding set
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
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
        /// Converts a string from ISO-8859-2 encoding to UTF-8 encoding
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>String.</returns>
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
        /// To the name of the valid file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        [Pure]
        public static string ToValidFileName(this string fileName)
        {
            return StringValidations.InvalidFileName.Replace(fileName, "_");
        }

        /// <summary>
        /// A String extension method that calculates Levenshtein distance.
        /// </summary>
        /// <param name="input">The string to be compared.</param>
        /// <param name="inputToCompare">The string to compare.</param>
        /// <returns>An Int32.</returns>
        [Pure]
        // ReSharper disable once MethodTooLong
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
                    var cost = inputToCompare.Substring(j - 1, 1) == input.Substring(i - 1, 1) ? 0 : 1;
                    var min = Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1);
                    d[i, j] = Math.Min(min, d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }

        /// <summary>
        /// A String extension method that calculates levenshtein distance with invariant culture and insensitive case.
        /// </summary>
        /// <param name="input">The string to be compared.</param>
        /// <param name="inputToCompare">The string to compare.</param>
        /// <returns>An Int32.</returns>
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
        /// <returns></returns>
        [Pure]
        public static string GetPathOrFileName(this string url)
        {
            var uri = new Uri(url);
            return string.IsNullOrWhiteSpace(uri.GetFileExtension()) ? uri.AbsolutePath : uri.GetFileName();
        }

        /// <summary>
        /// An URI extension method that gets file name.
        /// </summary>
        /// <param name="uri">The URI to act on.</param>
        /// <returns>The file name.</returns>
        [Pure]
        public static string GetFileName(this Uri uri)
        {
            return string.IsNullOrWhiteSpace(uri.LocalPath) ? string.Empty : Path.GetFileName(uri.LocalPath);
        }

        /// <summary>
        /// An URI extension method that gets file extension.
        /// </summary>
        /// <param name="uri">The URI to act on.</param>
        /// <returns>The file extension.</returns>
        [Pure]
        public static string GetFileExtension(this Uri uri)
        {
            return uri.LocalPath.GetFileExtension();
        }

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>String.</returns>

        [Pure]
        public static string GetFileExtension(this string fileName)
        {
            return string.IsNullOrWhiteSpace(fileName)
                       ? string.Empty
                       : Path.GetExtension(fileName);
        }

        /// <summary>
        /// To the center.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="spacer">The spacer.</param>
        /// <param name="lineSize">Size of the line.</param>
        /// <returns>String.</returns>
        [Pure]
        public static string ToCenter(this string input, char spacer, int lineSize)
        {
            var half = lineSize - input.Length / 2;
            return $@"{new string(spacer, half)}{input}{new string(spacer, input.Length % 2 == 1 ? ++half : half)}";
        }

        /// <summary>
        /// Determines whether the string is a valid JSON object
        /// </summary>
        /// <param name="jsonRaw">The raw json string.</param>
        /// <returns>Boolean.</returns>
        [Pure]
        public static bool IsValidJson(this string jsonRaw)
        {
            try
            {
                jsonRaw = jsonRaw.Trim();

                var isObject = jsonRaw.StartsWith(@"{") && jsonRaw.EndsWith(@"}");
                var isArray = jsonRaw.StartsWith(@"[") && jsonRaw.EndsWith(@"]");

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
        /// Split the string by chuck length,
        /// </summary>
        /// <param name="str"></param>
        /// <param name="chunkLength"></param>
        /// <returns></returns>
        [Pure]
        public static IEnumerable<string> SplitBy(this string str, int chunkLength)
        {
            if (!ValidateSplitBy(str, chunkLength))
            {
                yield return string.Empty;
                yield break;
            }

            for (var i = 0; i < str.Length; i += chunkLength)
            {
                yield return str.Substring(i, chunkLength + i > str.Length ? str.Length - i : chunkLength);
            }
        }

        private static bool ValidateSplitBy(string str, int chunkLength)
        {
            if (chunkLength < 1)
            {
                throw new ArgumentException(@"Invalid chuck length", nameof(chunkLength));
            }

            return !string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// Strips the tags.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
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
                inside = StripTagInternal(@let, inside, array, ref arrayIndex);
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
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool StripTagInternal(char @let, bool inside, char[] array, ref int arrayIndex)
        {
            switch (@let)
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

                    array[arrayIndex] = @let;
                    arrayIndex++;

                    break;
            }

            return false;
        }
    }
}
