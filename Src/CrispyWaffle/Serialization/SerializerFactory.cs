using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using CrispyWaffle.Composition;
using CrispyWaffle.Serialization.Adapters;
using CrispyWaffle.Serialization.SystemTextJson;

namespace CrispyWaffle.Serialization
{
    /// <summary>
    /// The serializer factory class.
    /// </summary>
    public static class SerializerFactory
    {
        /// <summary>
        /// Gets the type of the serializer from.
        /// </summary>
        /// <typeparam name="T">The type of the class.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>SerializerConverter&lt;T&gt;.</returns>
        /// <exception cref="InvalidOperationException">Invalid array subtype.</exception>
        /// <exception cref="InvalidOperationException">The {typeof(SerializerAttribute).FullName} attribute was not found in the object of type {type.FullName}</exception>
        [Pure]
        private static SerializerConverter<T> GetSerializerFromType<T>(T obj)
            where T : class
        {
            var type = obj.GetType();

            //if (!type.IsClass)
            //{
            //    throw new InvalidOperationException($"Unable to serializer the object of type '{type.FullName}'");
            //}

            if (
                Attribute.GetCustomAttribute(type, typeof(SerializerAttribute))
                is SerializerAttribute attribute
            )
            {
                return GetSerializer(obj, attribute);
            }

            if (GetSerializerFromArrayOrInherit(obj, ref type, out var serializerConverter))
            {
                return serializerConverter;
            }

            if (type.IsSerializable)
            {
                return GetSerializer(obj, new SerializerAttribute(SerializerFormat.Binary));
            }

            throw new InvalidOperationException(
                $"The {typeof(SerializerAttribute).FullName} attribute was not found in the object of type {type.FullName}"
            );
        }

        /// <summary>
        /// Gets the serializer from array or inherit.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="type">The type.</param>
        /// <param name="serializerConverter">The serializer converter.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <exception cref="InvalidOperationException">Invalid array subtype</exception>
        private static bool GetSerializerFromArrayOrInherit<T>(
            T obj,
            ref Type type,
            out SerializerConverter<T> serializerConverter
        )
            where T : class
        {
            serializerConverter = null;

            if (type.IsArray)
            {
                return GetSerializerFromArray(obj, ref type, ref serializerConverter);
            }

            return typeof(IEnumerable).IsAssignableFrom(type)
                && GetSerializerFromInherit(obj, ref type, ref serializerConverter);
        }

        /// <summary>
        /// Gets the serializer from inherit.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="type">The type.</param>
        /// <param name="serializerConverter">The serializer converter.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool GetSerializerFromInherit<T>(
            T obj,
            ref Type type,
            ref SerializerConverter<T> serializerConverter
        )
            where T : class
        {
            type = type.GetGenericArguments()[0];

            var attribute =
                Attribute.GetCustomAttribute(type, typeof(SerializerAttribute))
                as SerializerAttribute;

            if (attribute == null)
            {
                return false;
            }

            serializerConverter = GetSerializer(obj, attribute);

            return true;
        }

        /// <summary>
        /// Gets the serializer from array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="type">The type.</param>
        /// <param name="serializerConverter">The serializer converter.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <exception cref="InvalidOperationException">Invalid array subtype</exception>
        private static bool GetSerializerFromArray<T>(
            T obj,
            ref Type type,
            ref SerializerConverter<T> serializerConverter
        )
            where T : class
        {
            type = type.GetElementType();

            var attribute =
                Attribute.GetCustomAttribute(
                    type ?? throw new InvalidOperationException("Invalid array subtype"),
                    typeof(SerializerAttribute)
                ) as SerializerAttribute;

            if (attribute == null)
            {
                return false;
            }

            serializerConverter = GetSerializer(obj, attribute);

            return true;
        }

        /// <summary>
        ///     Gets the serializer.
        /// </summary>
        ///
        /// <typeparam name="T">
        ///     Generic type parameter.
        /// </typeparam>
        /// <param name="obj">
        ///     The object.
        /// </param>
        ///
        /// <returns>
        ///     The serializer&lt; t&gt;
        /// </returns>
        [Pure]
        public static SerializerConverter<T> GetSerializer<T>(this T obj)
            where T : class
        {
            return GetSerializerFromType(obj);
        }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>SerializerConverter&lt;T&gt;.</returns>
        [Pure]
        public static SerializerConverter<T> GetSerializer<T>()
            where T : class
        {
            var obj = (T)Activator.CreateInstance(typeof(T));
            return GetSerializerFromType(obj);
        }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">format</exception>
        [Pure]
        private static SerializerConverter<T> GetSerializer<T>(T obj, SerializerAttribute attribute)
            where T : class
        {
            switch (attribute.Format)
            {
                case SerializerFormat.Binary:
                    return new SerializerConverter<T>(
                        obj,
                        ServiceLocator.Resolve<BinarySerializerAdapter>()
                    );
                case SerializerFormat.Json when attribute.IsStrict:
                    return new SerializerConverter<T>(
                        obj,
                        ServiceLocator.Resolve<JsonSerializerAdapter>()
                    );
                case SerializerFormat.Json when !attribute.IsStrict:
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new NotNullObserverConverter() },
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    };
                    return new SerializerConverter<T>(obj, new JsonSerializerAdapter(options));
                case SerializerFormat.Xml:
                    return new SerializerConverter<T>(
                        obj,
                        ServiceLocator.Resolve<XmlSerializerAdapter>()
                    );
                default:
                    throw new InvalidOperationException(nameof(attribute.Format));
            }
        }

        /// <summary>
        ///     A T extension method that gets custom serializer.
        /// </summary>
        ///
        /// <typeparam name="T">
        ///     Generic type parameter.
        /// </typeparam>
        /// <param name="obj">
        ///     The object.
        /// </param>
        /// <param name="format">
        ///     Describes the format to use.
        /// </param>
        ///
        /// <returns>
        ///     The custom serializer&lt; t&gt;
        /// </returns>
        [Pure]
        public static SerializerConverter<T> GetCustomSerializer<T>(
            this T obj,
            SerializerFormat format
        )
            where T : class
        {
            return GetSerializer(obj, new SerializerAttribute(format));
        }

        /// <summary>
        /// 	A T extension method that gets custom serializer.
        /// </summary>
        ///
        /// <typeparam name="T">
        /// 	Generic type parameter.
        /// </typeparam>
        /// <param name="format">
        /// 	Describes the format to use.
        /// </param>
        ///
        /// <returns>
        /// 	The custom serializer&lt; t&gt;
        /// </returns>
        [Pure]
        public static SerializerConverter<T> GetCustomSerializer<T>(SerializerFormat format)
            where T : class
        {
            var obj = (T)Activator.CreateInstance(typeof(T));
            return GetCustomSerializer(obj, format);
        }
    }
}
