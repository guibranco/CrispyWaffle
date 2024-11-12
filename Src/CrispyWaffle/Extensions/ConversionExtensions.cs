using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace CrispyWaffle.Extensions
{
    /// <summary>
    /// Class ConversionExtensions.
    /// </summary>
    public static class ConversionExtensions
    {
        /// <summary>
        /// An IEnumerable&lt;String&gt; extension method that converts a binary representation
        /// string to the bytes.
        /// </summary>
        /// <param name="input">The string to act on.</param>
        /// <returns>string in binary representation as a Byte[].</returns>
        public static byte[] ToBytes(this IEnumerable<string> input) =>
            input.Select(s => Convert.ToByte(s, 2)).ToArray();

        /// <summary>
        /// A String extension method that converts this object to a boolean. This method assumes
        /// that <b>any</b> value other than stated in the optional parameter toTrue will be valid
        /// as false. It is useful for converting binary flags that can only take on two distinct
        /// values​​, or that only one value represents success and any other is invalid.
        /// </summary>
        /// <param name="str">The str to act on.</param>
        /// <param name="validValueForTrue">(Optional) the valid value for true.</param>
        /// <returns>The given data converted to a Boolean.</returns>
        public static bool ToBoolean(this string str, string validValueForTrue = "S") =>
            str?.Equals(validValueForTrue, StringComparison.OrdinalIgnoreCase) == true;

        /// <summary>
        /// A Boolean extension method that convert this object into a String representation.
        /// </summary>
        /// <param name="boolean">The boolean to act on.</param>
        /// <param name="trueValue">The value if true.</param>
        /// <param name="falseValue">The value if false.</param>
        /// <returns>The given data converted to a String.</returns>
        public static string ToString(this bool boolean, string trueValue, string falseValue) =>
            boolean ? trueValue : falseValue;

        /// <summary>
        /// Converts to datetime.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>DateTime.</returns>
        /// <exception cref="System.ArgumentNullException">input - Input value cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"> input - Unable to parse the string to a valid datetime. </exception>
        public static DateTime ToDateTime(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentNullException(nameof(input), "Input value cannot be null.");
            }

            if (input.TryToDateTime(out var result))
            {
                return result;
            }

            throw new ArgumentOutOfRangeException(
                nameof(input),
                input,
                "Unable to parse the string to a valid datetime."
            );
        }

        /// <summary>
        /// Tries to convert string to date time.
        /// </summary>
        /// <param name="input">The input string a valid DateTime format.</param>
        /// <param name="value">The DateTime value.</param>
        /// <returns><b>True</b> if success, <b>false</b> otherwise.</returns>
        public static bool TryToDateTime(this string input, out DateTime value)
        {
            value = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            input = input.ToLowerInvariant().Trim();
            switch (input)
            {
                case "now":

                    value = DateTime.Now;

                    return true;

                case "today":

                    value = DateTime.Today;

                    return true;

                case "yesterday":

                    value = DateTime.Today.AddDays(-1);

                    return true;

                case "tomorrow":

                    value = DateTime.Today.AddDays(1);

                    return true;

                default:

                    if (DateTime.TryParse(input, out value))
                    {
                        return true;
                    }

                    return input.Length == 10
                        && DateTime.TryParseExact(
                            input,
                            @"dd/MM/yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out value
                        );
            }
        }

        /// <summary>
        /// Converts to int32.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.Int32.</returns>
        public static int ToInt32(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return 0;
            }

            var success = int.TryParse(
                input,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out var result
            );

            return success ? result : 0;
        }

        /// <summary>
        /// To the int64.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Int64.</returns>
        public static long ToInt64(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return 0;
            }

            var success = long.TryParse(
                input,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out var result
            );

            return success ? result : 0;
        }

        /// <summary>
        /// A String extension method that converts a string to a decimal.
        /// </summary>
        /// <param name="input">The str to act on.</param>
        /// <returns>str as a Decimal.</returns>
        public static decimal ToDecimal(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return 0M;
            }

            return decimal.TryParse(
                input,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out var result
            )
                ? result
                : 0M;
        }

        /// <summary>
        /// An IEnumerable&lt;Byte&gt; extension method that converts the bytes to a binary string
        /// representation string.
        /// </summary>
        /// <param name="bytes">The bytes to act on.</param>
        /// <returns>bytes as a Binary String[] representation.</returns>
        public static string[] ToBinaryString(this IEnumerable<byte> bytes) =>
            bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')).ToArray();

        /// <summary>
        /// Converts to monetary.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <returns>System.String.</returns>
        public static string ToMonetary(this decimal input, CultureInfo cultureInfo = null)
        {
            return input == 0
                ? "No value"
                : string.Format(cultureInfo ?? CultureInfo.CurrentCulture, "{0:C}", input);
        }

        /// <summary>
        /// Converts a DateTime instance to Unix Timestamp (number of seconds that have elapsed
        /// since 00:00:00 Coordinated Universal Time (UTC), Thursday, 1 January 1970).
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>System.Int32.</returns>
        public static int ToUnixTimeStamp(this DateTime dateTime) =>
            (int)
                dateTime
                    .ToUniversalTime()
                    .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified))
                    .TotalSeconds;

        /// <summary>
        /// Converts a Unix Timestamp (number of seconds that have elapsed since 00:00:00
        /// Coordinated Universal Time (UTC), Thursday, 1 January 1970) to a DateTime instance.
        /// </summary>
        /// <param name="epochTime">The Unix Timestamp.</param>
        /// <returns>A DateTime instance of the epochTime.</returns>
        public static DateTime FromUnixTimeStamp(this int epochTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(Math.Round((double)epochTime / 1000))
                .ToLocalTime();
        }

        /// <summary>
        /// To the pretty string.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns>System.String.</returns>
        public static string ToPrettyString(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return string.Empty;
            }

            if (!json.IsValidJson())
            {
                return json;
            }

            var parsedJson = JToken.Parse(json);

            return parsedJson.ToString(Formatting.Indented);
        }

        /// <summary>
        /// Converts an XmlDocument to its string representation.
        /// </summary>
        /// <param name="document">The XmlDocument to be converted to a string.</param>
        /// <returns>A string representation of the <paramref name="document"/> if it is not null; otherwise, returns null.</returns>
        /// <remarks>
        /// This extension method takes an instance of <see cref="XmlDocument"/> and converts it into a formatted string.
        /// It uses an <see cref="XmlWriter"/> with specified settings to ensure that the output is indented and properly formatted.
        /// If the provided <paramref name="document"/> is null, the method will return null without attempting to process it.
        /// The resulting string can be useful for debugging or logging purposes, allowing for a human-readable format of the XML content.
        /// </remarks>
        public static string ToIdentString(this XmlDocument document)
        {
            if (document == null)
            {
                return null;
            }

            var builder = new StringBuilder();

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace,
            };

            using (var writer = XmlWriter.Create(builder, settings))
            {
                document.Save(writer);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Calculates the module-10 of a string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.Int32.</returns>
        public static int ToModule10(this string input)
        {
            var number = Regex.Replace(
                input,
                "[^0-9]",
                string.Empty,
                RegexOptions.Compiled,
                TimeSpan.FromSeconds(0.5)
            );

            var sum = 0;

            var weight = 2;

            var counter = number.Length - 1;

            while (counter >= 0)
            {
                var multiplication = number.Substring(counter, 1).ToInt32() * weight;

                if (multiplication >= 10)
                {
                    multiplication = 1 + (multiplication - 10);
                }

                sum += multiplication;

                weight = weight == 2 ? 1 : 2;

                counter--;
            }

            var digit = 10 - (sum % 10);

            if (digit == 10)
            {
                digit = 0;
            }

            return digit;
        }

        /// <summary>
        /// The ordinal suffix.
        /// </summary>
        private static readonly Dictionary<int, string> _ordinalSuffix = new Dictionary<int, string>
        {
            { 1, "st" },
            { 2, "nd" },
            { 3, "rd" },
        };

        /// <summary>
        /// To the ordinal.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns>System.String.</returns>
        public static string ToOrdinal(this long number)
        {
            if (number < 0)
            {
                return number.ToString(CultureInfo.CurrentCulture);
            }

            var rem = number % 100;

            if (rem >= 11 && rem <= 13)
            {
                return $"{number}th";
            }

            var key = (int)number % 10;
            if (_ordinalSuffix.TryGetValue(key, out var value))
            {
                return $"{number}{value}";
            }

            return $"{number}th";
        }

        /// <summary>
        /// To the ordinal.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns>System.String.</returns>
        public static string ToOrdinal(this int number) => ((long)number).ToOrdinal();

        /// <summary>
        /// Deeps the clone.
        /// </summary>
        /// <typeparam name="T">The type parameter.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="useNonPublic">if set to <c>true</c> [use non public].</param>
        /// <returns>T.</returns>
        public static T DeepClone<T>(this T instance, bool useNonPublic = true)
        {
            var type = typeof(T);

            var constructors = type.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length);
            var ctor = constructors.FirstOrDefault();

            if (ctor == null)
            {
                return default;
            }

            var arguments = new List<object>();
            var parameters = ctor.GetParameters();

            foreach (var parameter in parameters)
            {
                ParseParameters(instance, useNonPublic, type, parameter, arguments);
            }

            return (T)ctor.Invoke(arguments.ToArray());
        }

        /// <summary>
        /// Parses the parameters.
        /// </summary>
        /// <typeparam name="T">The type parameter.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="useNonPublic">if set to <c>true</c> [use non public].</param>
        /// <param name="type">The type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="arguments">The arguments.</param>
        private static void ParseParameters<T>(
            T instance,
            bool useNonPublic,
            Type type,
            ParameterInfo parameter,
            List<object> arguments
        )
        {
            var property = type.GetProperty(
                parameter.Name,
                BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance
            );

            if (property == null && !useNonPublic)
            {
                return;
            }

            if (property == null)
            {
                property = type.GetProperty(
                    parameter.Name,
                    BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Instance
                );

                if (property == null)
                {
                    return;
                }
            }

            if (property.PropertyType != parameter.ParameterType)
            {
                return;
            }

            arguments.Add(property.GetValue(instance));
        }
    }
}
