// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 09-05-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="SampleXmlClass.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using CrispyWaffle.Serialization;
using System;

namespace CrispyWaffle.Tests.Serialization
{
    /// <summary>
    /// Class SampleXmlClass.
    /// Implements the <see cref="System.IEquatable{CrispyWaffle.Tests.Serialization.SampleXmlClass}" />
    /// </summary>
    /// <seealso cref="System.IEquatable{CrispyWaffle.Tests.Serialization.SampleXmlClass}" />
    [Serializer]
    public class SampleXmlClass : IEquatable<SampleXmlClass>
    {
        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(SampleXmlClass other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Code == other.Code && string.Equals(String, other.String, StringComparison.InvariantCultureIgnoreCase) && CorrelationId.Equals(other.CorrelationId) && Equals(StrongTyping, other.StrongTyping);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((SampleXmlClass)obj);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Code);
            hashCode.Add(String, StringComparer.InvariantCultureIgnoreCase);
            hashCode.Add(CorrelationId);
            hashCode.Add(StrongTyping);
            return hashCode.ToHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="T:SampleClass" /> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(SampleXmlClass left, SampleXmlClass right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="T:SampleClass" /> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(SampleXmlClass left, SampleXmlClass right)
        {
            return !Equals(left, right);
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleXmlClass" /> class.
        /// </summary>
        public SampleXmlClass()
        { }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>The code.</value>
        public int Code { get; set; }

        /// <summary>
        /// Gets or sets the string.
        /// </summary>
        /// <value>The string.</value>
        public string String { get; set; }

        /// <summary>
        /// Gets or sets the correlation identifier.
        /// </summary>
        /// <value>The correlation identifier.</value>
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the strong typing.
        /// </summary>
        /// <value>The strong typing.</value>
        public StrongTypingClass StrongTyping { get; set; }

    }
}