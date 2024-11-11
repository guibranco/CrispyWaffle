using System;
using System;
using System.Collections;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Globalization;
using CrispyWaffle.Composition;
using CrispyWaffle.Composition;
using CrispyWaffle.Serialization.Adapters;
using CrispyWaffle.Serialization.Adapters;
using CrispyWaffle.Serialization.NewtonsoftJson;
using CrispyWaffle.Serialization.NewtonsoftJson;
using Newtonsoft.Json;
using Newtonsoft.Json;

namespace CrispyWaffle.Serialization
{
    /// <summary>
    /// Provides a factory for creating serializers for various formats.
    /// </summary>
    public static class SerializerFactory
    {
        /// <summary>
        /// Retrieves the serializer converter for a given object based on its type.
        /// </summary>
        /// <typeparam name="T">The type of the object to be serialized.</typeparam>
        /// <param name="obj">The object to be serialized.</param>
        /// <returns>A <see cref="SerializerConverter{T}"/> for the specified object.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the <see cref="SerializerAttribute"/> is not found on the object type.
        /// </exception>
        [Pure]
        private static SerializerConverter<T> GetSerializerFromType<T>(T obj)
            where T : class
        {
            var type = obj.GetType();

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

            throw new InvalidOperationException(
                $"The {typeof(SerializerAttribute).FullName} attribute was not found in the object of type {type.FullName}"
            );
        }

        /// <summary>
        /// Determines whether a serializer can be obtained from array or inherited types.
        /// </summary>
        /// <typeparam name="T">The type of the object to be serialized.</typeparam>
        /// <param name="obj">The object to be serialized.</param>
        /// <param name="type">The type of the object, modified if necessary.</param>
        /// <param name="serializerConverter">
        /// When this method returns, contains the serializer converter if found; otherwise, <c>null</c>.
        /// </param>
        /// <returns><c>true</c> if a serializer is found; otherwise, <c>false</c>.</returns>
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
        /// Attempts to get a serializer from inherited types.
        /// </summary>
        /// <typeparam name="T">The type of the object to be serialized.</typeparam>
        /// <param name="obj">The object to be serialized.</param>
        /// <param name="type">The type of the object, modified if necessary.</param>
        /// <param name="serializerConverter">
        /// When this method returns, contains the serializer converter if found; otherwise, <c>null</c>.
        /// </param>
        /// <returns><c>true</c> if a serializer is found; otherwise, <c>false</c>.</returns>
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
        /// Attempts to get a serializer for an array type.
        /// </summary>
        /// <typeparam name="T">The type of the object to be serialized.</typeparam>
        /// <param name="obj">The object to be serialized.</param>
        /// <param name="type">The type of the object, modified if necessary.</param>
        /// <param name="serializerConverter">
        /// When this method returns, contains the serializer converter if found; otherwise, <c>null</c>.
        /// </param>
        /// <returns><c>true</c> if a serializer is found; otherwise, <c>false</c>.</returns>
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
        /// Retrieves the serializer converter for the specified object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be serialized.</typeparam>
        /// <param name="obj">The object to be serialized.</param>
        /// <returns>A <see cref="SerializerConverter{T}"/> for the specified object.</returns>
        [Pure]
        public static SerializerConverter<T> GetSerializer<T>(this T obj)
            where T : class
        {
            return GetSerializerFromType(obj);
        }

        /// <summary>
        /// Retrieves the serializer converter for a new instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object to be serialized.</typeparam>
        /// <returns>A <see cref="SerializerConverter{T}"/> for the specified object.</returns>
        [Pure]
        public static SerializerConverter<T> GetSerializer<T>()
            where T : class
        {
            var obj = Activator.CreateInstance<T>();
            return GetSerializerFromType(obj);
        }

        /// <summary>
        /// Retrieves the serializer converter using the specified serializer attribute.
        /// </summary>
        /// <typeparam name="T">The type of the object to be serialized.</typeparam>
        /// <param name="obj">The object to be serialized.</param>
        /// <param name="attribute">The serializer attribute defining the format and settings.</param>
        /// <returns>A <see cref="SerializerConverter{T}"/> for the specified object.</returns>
        [Pure]
        private static SerializerConverter<T> GetSerializer<T>(T obj, SerializerAttribute attribute)
            where T : class
        {
            switch (attribute.Format)
            {
                case SerializerFormat.Json when attribute.IsStrict:
                    return new SerializerConverter<T>(
                        obj,
                        ServiceLocator.Resolve<NewtonsoftJsonSerializerAdapter>()
                    );

                case SerializerFormat.Json when !attribute.IsStrict:
                    var settings = new JsonSerializerSettings
                    {
                        Converters = { new NotNullObserverConverter() },
                        Culture = CultureInfo.GetCultureInfo("pt-br"),
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore,
                    };
                    return new SerializerConverter<T>(
                        obj,
                        new NewtonsoftJsonSerializerAdapter(settings)
                    );

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
        /// Retrieves a custom serializer for the specified object and format.
        /// </summary>
        /// <typeparam name="T">The type of the object to be serialized.</typeparam>
        /// <param name="obj">The object to be serialized.</param>
        /// <param name="format">The desired serialization format.</param>
        /// <returns>A <see cref="SerializerConverter{T}"/> for the specified object.</returns>
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
        /// Retrieves a custom serializer for a new instance of the specified type and format.
        /// </summary>
        /// <typeparam name="T">The type of the object to be serialized.</typeparam>
        /// <param name="format">The desired serialization format.</param>
        /// <returns>A <see cref="SerializerConverter{T}"/> for the specified object.</returns>
        [Pure]
        public static SerializerConverter<T> GetCustomSerializer<T>(SerializerFormat format)
            where T : class
        {
            var obj = Activator.CreateInstance<T>();
            return GetCustomSerializer(obj, format);
        }
    }
}
