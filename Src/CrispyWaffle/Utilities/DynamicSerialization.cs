namespace CrispyWaffle.Utilities
{
    using Extensions;
    using Serialization;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Dynamic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    /// <summary>
    /// A dynamic serialization.
    /// </summary>
    /// <seealso cref="DynamicObject" />
    /// <seealso cref="ISerializable" />
    /// <seealso cref="IXmlSerializable" />
    /// <seealso cref="System.IEquatable{DynamicSerialization}" />
    [Serializable]
    [Serializer]
    public class DynamicSerialization : DynamicObject, ISerializable, IXmlSerializable, IEquatable<DynamicSerialization>
    {
        #region Protected fields

        /// <summary>
        /// The dicionario.
        /// </summary>
        protected readonly Dictionary<string, object> Dictionary = new Dictionary<string, object>();

        /// <summary>
        /// The key serialization filter,
        /// </summary>
        protected readonly DynamicSerializationOption SerializationKeyFilter;

        #endregion

        #region ~Ctors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DynamicSerialization()
        {
            SerializationKeyFilter = DynamicSerializationOption.None;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="keyFilter">The filter to serialize keys</param>

        public DynamicSerialization(DynamicSerializationOption keyFilter)
        {
            SerializationKeyFilter = keyFilter;
        }

        /// <summary>
        /// Specialized constructor for use only by derived classes.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>

        public DynamicSerialization(SerializationInfo info, StreamingContext context)
        {
            SerializationKeyFilter = DynamicSerializationOption.None;
            foreach (var entry in info)
            {
                Dictionary.Add(entry.Name, entry.Value);
            }
        }

        #endregion

        #region Private methdos

        /// <summary>
        /// Filter the keys using a DynamicSerializationOption
        /// </summary>
        /// <param name="key">The key to applly the filter</param>
        /// <returns>The key filtered</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>

        private string Filter(string key)
        {
            switch (SerializationKeyFilter)
            {
                case DynamicSerializationOption.None:
                    return key;
                case DynamicSerializationOption.Lowercase:
                    return key.ToLower();
                case DynamicSerializationOption.Uppercase:
                    return key.ToUpper();
                case DynamicSerializationOption.Camelcase:
                    return key.ToCamelCase();
                default:
                    return key;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Sets the member.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="DynamicSerializationException"></exception>
        public void SetMember(string key, object value)
        {
            try
            {
                Dictionary.Add(key, value);
            }
            catch (Exception e)
            {
                throw new DynamicSerializationException(key, value, e);
            }
        }

        /// <summary>
        /// Provides the implementation for operations that get member values. Classes derived from
        /// the <see cref="T:System.Dynamic.DynamicObject" /> class can override this method to
        /// specify dynamic behavior for operations such as getting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name
        /// property provides the name of the member on which the dynamic operation is performed. For
        /// example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where
        /// sampleObject is an instance of the class derived from the
        /// <see cref="T:System.Dynamic.DynamicObject" /> class, binder.Name returns
        /// "SampleProperty". The binder.IgnoreCase property specifies whether the member name is
        /// case-sensitive.</param>
        /// <param name="result">The result of the get operation. For example, if the method is called for a property, you
        /// can assign the property value to <paramref name="result" />.</param>
        /// <returns>true if the operation is successful; otherwise, false. If this method returns false, the
        /// run-time binder of the language determines the behavior. (In most cases, a run-time
        /// exception is thrown.)</returns>
        [Pure]
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Dictionary.TryGetValue(binder.Name, out result);
            return true;
        }

        /// <summary>
        /// Provides the implementation for operations that set member values. Classes derived from
        /// the <see cref="T:System.Dynamic.DynamicObject" /> class can override this method to
        /// specify dynamic behavior for operations such as setting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name
        /// property provides the name of the member to which the value is being assigned. For
        /// example, for the statement sampleObject.SampleProperty = "Test", where sampleObject is an
        /// instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject" />
        /// class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies
        /// whether the member name is case-sensitive.</param>
        /// <param name="value">The value to set to the member. For example, for sampleObject.SampleProperty = "Test",
        /// where sampleObject is an instance of the class derived from the
        /// <see cref="T:System.Dynamic.DynamicObject" /> class, the <paramref name="value" /> is
        /// "Test".</param>
        /// <returns>true if the operation is successful; otherwise, false. If this method returns false, the
        /// run-time binder of the language determines the behavior. (In most cases, a language-
        /// specific run-time exception is thrown.)</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Dictionary.Add(binder.Name, value);
            return true;
        }

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data
        /// needed to serialize the target object.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (var kvp in Dictionary)
            {
                info.AddValue(Filter(kvp.Key), kvp.Value);
            }
        }

        /// <summary>
        /// Gets the dictionary.
        /// </summary>
        /// <returns>The dictionary.</returns>
        [Pure]
        public Dictionary<string, object> GetDictionary()
        {
            return Dictionary;
        }

        #endregion

        #region Equality members

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Dictionary?.GetHashCode() ?? 0) * 397) ^ (int)SerializationKeyFilter;
            }
        }

        /// <summary>
        /// Equals operator
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(DynamicSerialization left, DynamicSerialization right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Different operator
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(DynamicSerialization left, DynamicSerialization right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object" /> is equal to the current
        /// <see cref="T:System.Object" />.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object" /> to compare with the current
        /// <see cref="T:System.Object" />.</param>
        /// <returns>true if the specified <see cref="T:System.Object" /> is equal to the current
        /// <see cref="T:System.Object" />; otherwise, false.</returns>

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((DynamicSerialization)obj);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object" /> is equal to the current
        /// <see cref="T:System.Object" />.
        /// </summary>
        /// <param name="other">The dynamic serialization to compare to this object.</param>
        /// <returns>true if the specified <see cref="T:System.Object" /> is equal to the current
        /// <see cref="T:System.Object" />; otherwise, false.</returns>
        public bool Equals(DynamicSerialization other)
        {
            return other != null &&
                    Dictionary.SequenceEqual(other.Dictionary) &&
                   SerializationKeyFilter == other.SerializationKeyFilter;
        }

        #endregion

        #region Implementation of IXmlSerializable

        /// <summary>
        /// This method is reserved and should not be used.When implementing the IXmlSerializable
        /// interface, you should return null (Nothing in Visual Basic) from this method, and instead,
        /// if specifying a custom schema is required, apply the
        /// <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>An <see cref="T:System.Xml.Schema.XmlSchema" /> that describes the XML representation of
        /// the object that is produced by the
        /// <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" />
        /// method and consumed by the
        /// <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" />
        /// method.</returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> stream from which the object is deserialized.</param>
        public void ReadXml(XmlReader reader)
        {
            if (reader.MoveToContent() != XmlNodeType.Element)
            {
                return;
            }

            reader.Read();
            while (!reader.EOF)
            {
                if (!reader.IsStartElement())
                {
                    reader.Read();
                    continue;
                }
                Dictionary.Add(reader.LocalName, reader.ReadElementContentAsString());
            }
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> stream to which the object is serialized.</param>

        public void WriteXml(XmlWriter writer)
        {
            foreach (var kvp in Dictionary)
            {
                writer.WriteStartElement(Filter(kvp.Key));
                writer.WriteValue(kvp.Value);
                writer.WriteEndElement();
            }
        }

        #endregion
    }
}
