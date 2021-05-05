// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 09-05-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace CrispyWaffle.Tests.Serialization
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class 
    /// </summary>
    public static class TestObjects
    {
        /// <summary>
        /// Gets the sample XML.
        /// </summary>
        /// <returns>SampleXmlClass.</returns>
        public static SampleXmlClass GetSampleXml()
        {
            var correlationId = Guid.NewGuid();
            return new SampleXmlClass
            {
                Code = new Random().Next(1, 99999),
                CorrelationId = correlationId,
                String = "Some text",
                StrongTyping = GetStrongTyping(correlationId)
            };
        }

        /// <summary>
        /// Gets the strong typing.
        /// </summary>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <returns>StrongTypingClass.</returns>
        public static StrongTypingClass GetStrongTyping(Guid correlationId) => new StrongTypingClass
        {
            CorrelationId = correlationId,
            Date = DateTime.Now,
            SomeText = DateTime.Now.ToString("O")
        };

        /// <summary>
        /// Gets the sample json.
        /// </summary>
        /// <returns>SampleJsonClass.</returns>
        public static SampleJsonClass GetSampleJson() =>
            new SampleJsonClass
            {
                Date = DateTime.Now,
                Id = Guid.NewGuid(),
                ListStrong = new List<StrongTypingClass>
                {
                    GetStrongTyping(Guid.Empty),
                    GetStrongTyping(Guid.NewGuid()),
                    GetStrongTyping(Guid.NewGuid())
                }
            };

        /// <summary>
        /// Gets the sample json not strict.
        /// </summary>
        /// <returns>SampleJsonNotStrictClass.</returns>
        public static SampleJsonNotStrictClass GetSampleJsonNotStrict() =>
            new SampleJsonNotStrictClass
            {
                Date = DateTime.Now
            };

        /// <summary>
        /// Gets the non serializable.
        /// </summary>
        /// <returns>SampleNonSerializableClass.</returns>
        internal static SampleNonSerializableClass GetNonSerializable()
            => new SampleNonSerializableClass
            {
                Date = DateTime.Now
            };

        /// <summary>
        /// Gets the enumerable json.
        /// </summary>
        /// <returns>IEnumerable&lt;SampleXmlClass&gt;.</returns>
        public static IEnumerable<SampleXmlClass> GetEnumerableJson()
        {
            for (var i = 0; i < 1; i++)
            {
                yield return GetSampleXml();
            }
        }
    }
}
