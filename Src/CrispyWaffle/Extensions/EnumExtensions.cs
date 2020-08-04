namespace CrispyWaffle.Extensions
{
    using Attributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A Enum extensions class.
    /// </summary>
    public static class EnumExtensions
    {

        /// <summary>
        /// Gets the enum by human readable attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="humanReadableValue">The human readable value.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentOutOfRangeException">humanReadableValue</exception>
        public static T GetEnumByHumanReadableAttribute<T>(string humanReadableValue)
        {
            var type = typeof(T);
            if (!type.IsEnum)
            {
                throw new InvalidOperationException($"The type {type.FullName} must be a enum type");
            }

            var field = type.GetFields().Select(f =>
                    new
                    {
                        Field = f,
                        Attr =
                            Attribute.GetCustomAttribute(f, typeof(HumanReadableAttribute)) as
                                HumanReadableAttribute
                    })
                .Where(item =>
                    item.Attr != null &&
                    item.Attr.StringValue.Equals(humanReadableValue, StringComparison.InvariantCultureIgnoreCase) ||
                    item.Field.Name.Equals(humanReadableValue, StringComparison.InvariantCultureIgnoreCase))
                .Select(item => item.Field)
                .SingleOrDefault();
            if (field == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(humanReadableValue),
                    humanReadableValue,
                    $"Unable to find the field for {type.FullName} with value {humanReadableValue} in the attribute of type {typeof(HumanReadableAttribute).FullName}");
            }

            return (T)field.GetValue(null);

        }

        /// <summary>
        /// Gets the enum by internal value attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="internalValue">The internal value.</param>
        /// <returns></returns>
        public static T GetEnumByInternalValueAttribute<T>(string internalValue)
        {
            var type = typeof(T);
            if (!type.IsEnum)
            {
                throw new InvalidOperationException($"The type {type.FullName} must be a enum type");
            }

            if (internalValue == null)
            {
                return default;
            }

            var field = type.GetFields().Select(f =>
                    new
                    {
                        Field = f,
                        Attr =
                            Attribute.GetCustomAttribute(f, typeof(InternalValueAttribute)) as
                                InternalValueAttribute
                    })
                .Where(item =>
                    item.Attr != null &&
                    item.Attr.InternalValue.Equals(internalValue, StringComparison.InvariantCultureIgnoreCase) ||
                    item.Field.Name.Equals(internalValue, StringComparison.InvariantCultureIgnoreCase))
                .Select(item => item.Field)
                .SingleOrDefault();
            if (field == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(internalValue),
                    internalValue,
                    $"Unable to find the field for {type.FullName} with value {internalValue} in the attribute of type {typeof(InternalValueAttribute).FullName}");
            }

            return (T)field.GetValue(null);
        }

        /// <summary>
        /// Gets the human readable value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="flagsSeparator">The flags separator.</param>
        /// <returns></returns>
        public static string GetHumanReadableValue(this Enum value, string flagsSeparator = " | ")
        {
            var type = value.GetType();
            if (type.GetCustomAttributes(typeof(FlagsAttribute), false).Any() &&
                value.GetFlagsCount() > 1)
            {
                var values = value.GetUniqueFlags<Enum>().ToList();
                var displayNames = values
                    .Where(v => value.HasFlag(v) && v.GetHashCode() != 0)
                    .Select(v => type.GetField(v.ToString()))
                    .Select(f => f.GetCustomAttributes(typeof(HumanReadableAttribute), false)[0])
                    .Cast<HumanReadableAttribute>()
                    .Select(attribute => attribute.StringValue)
                    .ToList();
                return displayNames.Any()
                           ? string.Join(flagsSeparator, displayNames)
                           : "None";
            }
            var fieldInfo = type.GetField(value.ToString());
            if (fieldInfo == null)
            {
                throw new ArgumentNullException(nameof(value), "Input value cannot be null");
            }

            return fieldInfo.GetCustomAttributes(typeof(HumanReadableAttribute), false) is HumanReadableAttribute[]
                       attributes && attributes.Length > 0
                       ? attributes[0].StringValue
                       : null;
        }
        /// <summary>
        /// Gets the internal value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string GetInternalValue(this Enum value)
        {
            var type = value.GetType();
            var fieldInfo = type.GetField(value.ToString());
            return fieldInfo.GetCustomAttributes(typeof(InternalValueAttribute), false) is InternalValueAttribute[]
                       attributes && attributes.Length > 0
                       ? attributes[0].InternalValue
                       : null;
        }

        /// <summary>
        /// Gets the flags count.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>Int64.</returns>
        private static ulong GetFlagsCount(this Enum field)
        {
            var v = (ulong)field.GetHashCode();
            v = v - ((v >> 1) & 0x55555555);
            v = (v & 0x33333333) + ((v >> 2) & 0x33333333);
            return ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
        }

        /// <summary>
        /// Gets the unique flags.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="flags">The flags.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// The generic type parameter must be an Enum.
        /// or
        /// The generic type parameter does not match the target type.
        /// </exception>
        public static IEnumerable<T> GetUniqueFlags<T>(this Enum flags)
        {
            ulong flag = 1;
            foreach (var value in Enum.GetValues(flags.GetType()).Cast<T>())
            {
                var bits = Convert.ToUInt64(value);
                while (flag < bits)
                {
                    flag <<= 1;
                }

                if (flag == bits && flags.HasFlag(value as Enum ?? throw new InvalidOperationException($"{nameof(value)} isn't a valid enum value")))
                {
                    yield return value;
                }
            }
        }
    }
}
