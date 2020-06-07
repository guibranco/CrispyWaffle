namespace CrispyWaffle.Serialization
{
    using Adapters;
    using Composition;
    using Newtonsoft.Json;
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Globalization;

    /// <summary>
    /// The serializer factory class.
    /// </summary>
    [DebuggerStepThrough]
    public static class SerializerFactory
    {
        /// <summary>
        ///     Gets serializer from type.
        /// </summary>
        ///
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is invalid.
        /// </exception>
        ///
        /// <typeparam name="T">
        ///     Generic type parameter.
        /// </typeparam>
        /// <param name="obj">
        ///     The object.
        /// </param>
        ///
        /// <returns>
        ///     The serializer from type&lt; t&gt;
        /// </returns>
        /// 
        /// 
        [Pure]
        private static SerializerConverter<T> GetSerializerFromType<T>(T obj) where T : class
        {
            var type = obj.GetType();
            if (!type.IsClass)
                throw new InvalidOperationException($"Unable to serializer the object of type '{type.FullName}'");
            if (Attribute.GetCustomAttribute(type, typeof(SerializerAttribute)) is SerializerAttribute attribute)
                return GetSerializer(obj, attribute);
            if (type.IsArray)
            {
                type = type.GetElementType();
                attribute = Attribute.GetCustomAttribute(type ?? throw new InvalidOperationException(), typeof(SerializerAttribute)) as SerializerAttribute;
                if (attribute != null)
                    return GetSerializer(obj, attribute);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                type = type.GetGenericArguments()[0];
                attribute = Attribute.GetCustomAttribute(type, typeof(SerializerAttribute)) as SerializerAttribute;
                if (attribute != null)
                    return GetSerializer(obj, attribute);
            }
            if (type.IsSerializable)
                return GetSerializer(obj, new SerializerAttribute(SerializerFormat.BINARY));
            throw new InvalidOperationException(
                $"The {typeof(SerializerAttribute).FullName} attribute was not found in the object of type {type.FullName}");
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
        public static SerializerConverter<T> GetSerializer<T>(this T obj) where T : class
        {
            return GetSerializerFromType(obj);
        }

        /// <summary>
        ///     Gets the serializer.
        /// </summary>
        ///
        /// <typeparam name="T">
        ///     Generic type parameter.
        /// </typeparam>
        ///
        /// <returns>
        ///     The serializer&lt; t&gt;
        /// </returns>
        [Pure]
        public static SerializerConverter<T> GetSerializer<T>() where T : class
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
        private static SerializerConverter<T> GetSerializer<T>(T obj, SerializerAttribute attribute) where T : class
        {
            switch (attribute.Format)
            {
                case SerializerFormat.BINARY:
                    return new SerializerConverter<T>(obj, ServiceLocator.Resolve<BinarySerializerAdapter>());
                case SerializerFormat.JSON when attribute.IsStrict:
                    return new SerializerConverter<T>(obj, ServiceLocator.Resolve<JsonSerializerAdapter>());
                case SerializerFormat.JSON when !attribute.IsStrict:
                    var settings = new JsonSerializerSettings
                    {
                        Converters = { new NotNullObserverConverter() },
                        Culture = CultureInfo.GetCultureInfo("pt-br"),
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    };
                    return new SerializerConverter<T>(obj, new JsonSerializerAdapter(settings));
                case SerializerFormat.XML:
                    return new SerializerConverter<T>(obj, ServiceLocator.Resolve<XmlSerializerAdapter>());
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
        public static SerializerConverter<T> GetCustomSerializer<T>(this T obj, SerializerFormat format) where T : class
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
        public static SerializerConverter<T> GetCustomSerializer<T>(SerializerFormat format) where T : class
        {
            var obj = (T)Activator.CreateInstance(typeof(T));
            return GetCustomSerializer(obj, format);
        }
    }
}
