using System;
using System.Collections.Generic;
using System.Linq;
using CrispyWaffle.Attributes;

namespace CrispyWaffle.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="Enum"/> types, including retrieving enum values by human-readable and internal value attributes.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Retrieves the enum value associated with a specified human-readable string representation.
    /// </summary>
    /// <typeparam name="T">The enum type from which to retrieve the value.</typeparam>
    /// <param name="humanReadableValue">The human-readable string representation of the enum value.</param>
    /// <returns>The enum value of type <typeparamref name="T"/> that corresponds to the specified human-readable string.</returns>
    /// <remarks>
    /// This method checks if the provided type <typeparamref name="T"/> is an enum. If it is not, an
    /// <see cref="InvalidOperationException"/> is thrown. It then searches through the fields of the enum
    /// type to find a field that matches the provided human-readable value either by its custom attribute
    /// <see cref="HumanReadableAttribute"/> or by its name. If no matching field is found, an
    /// <see cref="ArgumentOutOfRangeException"/> is thrown indicating that the specified value does not
    /// correspond to any field in the enum. This method is useful for converting user-friendly strings
    /// into their corresponding enum values, enhancing the usability of enums in applications.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the type <typeparamref name="T"/> is not an enum type.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when no field in the enum matches the specified human-readable value.
    /// </exception>
    public static T GetEnumByHumanReadableAttribute<T>(string humanReadableValue)
    {
        var type = typeof(T);
        if (!type.IsEnum)
        {
            throw new InvalidOperationException($"The type {type.FullName} must be an enum type");
        }

        var field = type.GetFields()
            .Select(f => new
            {
                Field = f,
                Attr = Attribute.GetCustomAttribute(f, typeof(HumanReadableAttribute))
                    as HumanReadableAttribute,
            })
            .Where(item =>
                (
                    item.Attr != null
                    && item.Attr.StringValue.Equals(
                        humanReadableValue,
                        StringComparison.OrdinalIgnoreCase
                    )
                ) || item.Field.Name.Equals(humanReadableValue, StringComparison.OrdinalIgnoreCase)
            )
            .Select(item => item.Field)
            .SingleOrDefault();
        if (field == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(humanReadableValue),
                humanReadableValue,
                $"Unable to find the field for {type.FullName} with value {humanReadableValue} in the attribute of type {typeof(HumanReadableAttribute).FullName}"
            );
        }

        return (T)field.GetValue(null);
    }

    /// <summary>
    /// Retrieves an enumeration value based on the specified internal value attribute.
    /// </summary>
    /// <typeparam name="T">The enum type from which to retrieve the value.</typeparam>
    /// <param name="internalValue">The internal value associated with the desired enum field.</param>
    /// <returns>The enum value corresponding to the specified internal value attribute.</returns>
    /// <remarks>
    /// This method checks if the provided type <typeparamref name="T"/> is an enum. If it is not, an
    /// <see cref="InvalidOperationException"/> is thrown. If the <paramref name="internalValue"/> is null,
    /// the method returns the default value for the enum type. It then searches through the fields of the
    /// enum type for a field that has an <see cref="InternalValueAttribute"/> with a matching internal value
    /// or a field name that matches the provided internal value. If no matching field is found, an
    /// <see cref="ArgumentOutOfRangeException"/> is thrown. If a match is found, the corresponding enum
    /// value is returned.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when the type <typeparamref name="T"/> is not an enum type.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when no field with the specified internal value is found.</exception>
    public static T GetEnumByInternalValueAttribute<T>(string internalValue)
    {
        var type = typeof(T);
        if (!type.IsEnum)
        {
            throw new InvalidOperationException($"The type {type.FullName} must be an enum type");
        }

        if (internalValue == null)
        {
            return default;
        }

        var field = type.GetFields()
            .Select(f => new
            {
                Field = f,
                Attr = Attribute.GetCustomAttribute(f, typeof(InternalValueAttribute))
                    as InternalValueAttribute,
            })
            .Where(item =>
                (
                    item.Attr != null
                    && item.Attr.InternalValue.Equals(
                        internalValue,
                        StringComparison.OrdinalIgnoreCase
                    )
                ) || item.Field.Name.Equals(internalValue, StringComparison.OrdinalIgnoreCase)
            )
            .Select(item => item.Field)
            .SingleOrDefault();
        if (field == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(internalValue),
                internalValue,
                $"Unable to find the field for {type.FullName} with value {internalValue} in the attribute of type {typeof(InternalValueAttribute).FullName}"
            );
        }

        return (T)field.GetValue(null);
    }

    /// <summary>
    /// Gets the human-readable value for a given enum value, supporting flags-based enums.
    /// </summary>
    /// <param name="value">The enum value to retrieve the human-readable representation for.</param>
    /// <param name="flagsSeparator">The separator string used to separate multiple flag values (default is " | ").</param>
    /// <returns>A string representing the human-readable value(s) for the enum.</returns>
    /// <remarks>
    /// If the enum type is marked with the <see cref="FlagsAttribute"/> and contains multiple flags,
    /// this method will return a concatenated string of the human-readable values for each set flag.
    /// If no human-readable attribute is found, the method will return null.
    /// </remarks>
    public static string GetHumanReadableValue(this Enum value, string flagsSeparator = " | ")
    {
        var type = value.GetType();
        if (
            type.GetCustomAttributes(typeof(FlagsAttribute), false).Length != 0
            && value.GetFlagsCount() > 1
        )
        {
            var values = value.GetUniqueFlags<Enum>().ToList();
            var displayNames = values
                .Where(v => value.HasFlag(v) && v.GetHashCode() != 0)
                .Select(v => type.GetField(v.ToString()))
                .Select(f => f.GetCustomAttributes(typeof(HumanReadableAttribute), false)[0])
                .Cast<HumanReadableAttribute>()
                .Select(attribute => attribute.StringValue)
                .ToList();
            return displayNames.Count != 0 ? string.Join(flagsSeparator, displayNames) : "None";
        }

        var fieldInfo = type.GetField(value.ToString());
        if (fieldInfo == null)
        {
            throw new ArgumentNullException(nameof(value), "Input value cannot be null");
        }

        return
            fieldInfo.GetCustomAttributes(typeof(HumanReadableAttribute), false)
                is HumanReadableAttribute[] attributes
            && attributes.Length > 0
            ? attributes[0].StringValue
            : null;
    }

    /// <summary>
    /// Gets the internal value for a given enum value.
    /// </summary>
    /// <param name="value">The enum value to retrieve the internal value for.</param>
    /// <returns>The internal value associated with the enum field, or null if none exists.</returns>
    public static string GetInternalValue(this Enum value)
    {
        var type = value.GetType();
        var fieldInfo = type.GetField(value.ToString());
        return
            fieldInfo.GetCustomAttributes(typeof(InternalValueAttribute), false)
                is InternalValueAttribute[] attributes
            && attributes.Length > 0
            ? attributes[0].InternalValue
            : null;
    }

    /// <summary>
    /// Gets the count of unique flags set on an enum value.
    /// </summary>
    /// <param name="field">The enum value to calculate the number of unique flags for.</param>
    /// <returns>The number of unique flags set on the enum value.</returns>
    private static ulong GetFlagsCount(this Enum field)
    {
        var v = (ulong)field.GetHashCode();
        v -= v >> 1 & 0x55555555;
        v = (v & 0x33333333) + ((v >> 2) & 0x33333333);
        return ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
    }

    /// <summary>
    /// Gets the unique flags set on an enum value.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="flags">The enum value containing the flags.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of unique flags set on the enum value.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the generic type parameter is not an <see cref="Enum"/>.
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

            if (
                flag == bits
                && flags.HasFlag(
                    value as Enum
                        ?? throw new InvalidOperationException(
                            $"{nameof(value)} isn't a valid enum value"
                        )
                )
            )
            {
                yield return value;
            }
        }
    }
}
