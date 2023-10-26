// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 09-05-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="SampleJsonNotStrictClass.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace CrispyWaffle.Tests.Serialization
{
    using CrispyWaffle.Serialization;
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class SampleJsonNotStrictClass.
    /// Implements the <see cref="SampleJsonClass" />
    /// Implements the <see cref="System.IEquatable{SampleJsonNotStrictClass}" />
    /// </summary>
    /// <seealso cref="SampleJsonClass" />
    /// <seealso cref="System.IEquatable{SampleJsonNotStrictClass}" />
    [Serializer(SerializerFormat.Json, false)]
    [ExcludeFromCodeCoverage]
    public sealed class SampleJsonNotStrictClass
        : SampleJsonClass,
        IEquatable<SampleJsonNotStrictClass>
    {
        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(SampleJsonNotStrictClass other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Text, other.Text, StringComparison.InvariantCultureIgnoreCase)
                && Equals(StrongTyping, other.StrongTyping);
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

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((SampleJsonNotStrictClass)obj);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Text, StringComparer.InvariantCultureIgnoreCase);
            hashCode.Add(StrongTyping);
            return hashCode.ToHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="T:SampleJsonNotStrictClass" /> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(
            SampleJsonNotStrictClass left,
            SampleJsonNotStrictClass right
        )
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="T:SampleJsonNotStrictClass" /> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(
            SampleJsonNotStrictClass left,
            SampleJsonNotStrictClass right
        )
        {
            return !Equals(left, right);
        }

        #endregion

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the strong typing.
        /// </summary>
        /// <value>The strong typing.</value>
        public StrongTypingClass StrongTyping { get; set; }
    }
}
