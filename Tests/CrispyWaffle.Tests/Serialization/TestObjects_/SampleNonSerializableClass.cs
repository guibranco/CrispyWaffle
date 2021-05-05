// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 09-05-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="SampleNonSerializableClass.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.Tests.Serialization
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class SampleNonSerializableClass.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SampleNonSerializableClass
    {
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        public DateTime Date { get; set; }
    }
}