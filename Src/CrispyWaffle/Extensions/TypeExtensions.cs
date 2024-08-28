using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CrispyWaffle.Attributes;
using CrispyWaffle.Utilities;

namespace CrispyWaffle.Extensions
{
    /// <summary>
    /// Class TypeExtensions.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets the name of the query string builder key.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>string.</returns>
        public static string GetQueryStringBuilderKeyName(this PropertyInfo propertyInfo)
        {
            return
                propertyInfo.GetCustomAttributes(typeof(QueryStringBuilderKeyNameAttribute), false)
                    is QueryStringBuilderKeyNameAttribute[] { Length: > 0 } attributes
                ? attributes[0].KeyName
                : null;
        }

        /// <summary>
        /// Queries the string builder ignore.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>Boolean.</returns>
        public static bool QueryStringBuilderIgnore(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(
                typeof(QueryStringBuilderIgnoreAttribute),
                false
            )
                is QueryStringBuilderIgnoreAttribute[] { Length: > 0 };
        }

        /// <summary>
        /// Updates the values.
        /// </summary>
        /// <typeparam name="T">The original type.</typeparam>
        /// <param name="currentObject">The current object.</param>
        /// <param name="newObject">The new object.</param>
        /// <returns>T.</returns>
        public static T UpdateValues<T>(this T currentObject, T newObject)
            where T : IUpdateable
        {
            var type = currentObject.GetType();
            var defaultObject = (T)Activator.CreateInstance(type);
            foreach (var propertyInfo in type.GetProperties())
            {
                UpdateValueInternal(currentObject, newObject, propertyInfo, defaultObject, type);
            }

            return currentObject;
        }

        /// <summary>
        /// Updates the value internal.
        /// </summary>
        /// <typeparam name="T">The original type.</typeparam>
        /// <param name="currentObject">The current object.</param>
        /// <param name="newObject">The new object.</param>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="defaultObject">The default object.</param>
        /// <param name="type">The type.</param>
        private static void UpdateValueInternal<T>(
            T currentObject,
            T newObject,
            PropertyInfo propertyInfo,
            T defaultObject,
            Type type
        )
            where T : IUpdateable
        {
            var defaultValue = propertyInfo.GetValue(defaultObject);
            var newValue = propertyInfo.GetValue(newObject);
            var currentValue = propertyInfo.GetValue(currentObject);
            var shouldSerialize = false;

            var shouldSerializeMethod = type.GetMethod(
                string.Concat("ShouldSerialize", propertyInfo.Name)
            );
            if (shouldSerializeMethod != null && shouldSerializeMethod.ReturnType == typeof(bool))
            {
                shouldSerialize = (bool)shouldSerializeMethod.Invoke(newObject, null);
            }

            if (!shouldSerialize && (newValue == null || newValue.Equals(defaultValue)))
            {
                return;
            }

            if (newValue == null || propertyInfo.PropertyType.IsSimpleType())
            {
                propertyInfo.SetValue(currentObject, newValue);
            }
            else if (typeof(IUpdateable).IsAssignableFrom(propertyInfo.PropertyType))
            {
                propertyInfo.SetValue(
                    currentObject,
                    ((IUpdateable)currentValue).UpdateValues((IUpdateable)newValue),
                    null
                );
            }
        }

        /// <summary>
        /// Check whenever the type <paramref name="source"/> implements the <typeparamref name="TInterface"/>.
        /// </summary>
        /// <typeparam name="TInterface">The interface to check.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns><c>true</c> if implements, false otherwise.</returns>
        public static bool Implements<TInterface>(this Type source)
            where TInterface : class
        {
            return typeof(TInterface).IsAssignableFrom(source);
        }

        /// <summary>
        /// Determines whether the specified type is a simple type.
        /// </summary>
        /// <param name="type">The type to evaluate.</param>
        /// <returns>True if the <paramref name="type"/> is a simple type; otherwise, false.</returns>
        /// <remarks>
        /// A simple type is defined as a value type, a primitive type, or one of the following types:
        /// string, decimal, DateTime, DateTimeOffset, TimeSpan, or Guid.
        /// Additionally, if the type's code is not TypeCode.Object, it is also considered a simple type.
        /// This method can be useful for determining how to handle types in serialization,
        /// data storage, or other operations where the complexity of the type may affect processing.
        /// </remarks>
        public static bool IsSimpleType(this Type type)
        {
            return type.IsValueType
                || type.IsPrimitive
                || new[]
                {
                    typeof(string),
                    typeof(decimal),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(Guid),
                }.Contains(type)
                || Convert.GetTypeCode(type) != TypeCode.Object;
        }

        /// <summary>
        /// The primitive numeric types.
        /// </summary>
        private static readonly HashSet<TypeCode> _primitiveNumericTypes =
            new()
            {
                TypeCode.Byte,
                TypeCode.SByte,
                TypeCode.UInt16,
                TypeCode.UInt32,
                TypeCode.UInt64,
                TypeCode.Int16,
                TypeCode.Int32,
                TypeCode.Int64,
                TypeCode.Decimal,
                TypeCode.Double,
                TypeCode.Single,
            };

        /// <summary>
        /// Determines whether [is numeric type] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Boolean.</returns>
        public static bool IsNumericType(this Type type)
        {
            while (true)
            {
                var typeCode = Type.GetTypeCode(type);

                if (_primitiveNumericTypes.Contains(typeCode))
                {
                    return true;
                }

                if (typeCode != TypeCode.Object)
                {
                    return false;
                }

                if (
                    type != null
                    && (
                        !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Nullable<>)
                    )
                )
                {
                    return false;
                }

                if (type != null)
                {
                    type = Nullable.GetUnderlyingType(type);
                }
            }
        }

        /// <summary>
        /// Gets the calling method.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="excludeBegin">if set to <c>true</c> [exclude begin].</param>
        /// <returns>String.</returns>
        public static string GetCallingMethod(int frame = 1, bool excludeBegin = true)
        {
            var stack = new StackTrace();
            var method = stack.GetFrame(frame).GetMethod();
            if (method == null)
            {
                return "CrispyWaffle";
            }

            var ns = method.DeclaringType?.FullName;
            if (string.IsNullOrWhiteSpace(ns))
            {
                return method.Name;
            }

            if (
                excludeBegin
                && ns.StartsWith("CrispyWaffle", StringComparison.InvariantCultureIgnoreCase)
            )
            {
                ns = ns.Substring(13);
            }

            return $"{ns}.{method.Name}";
        }
    }
}
