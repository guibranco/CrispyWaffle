// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 09-06-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-06-2020
// ***********************************************************************
// <copyright file="TestEnum.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using CrispyWaffle.Attributes;

namespace CrispyWaffle.Tests.Extensions;

/// <summary>
/// Enum TestEnum
/// </summary>
public enum TestEnum
{
    /// <summary>
    /// The none
    /// </summary>
    [InternalValue("ноль")]
    [HumanReadable("Zero")]
    None = 0,

    /// <summary>
    /// The one
    /// </summary>
    [InternalValue("один")]
    [HumanReadable("Um")]
    One = 1,

    /// <summary>
    /// The two
    /// </summary>
    [InternalValue("два")]
    [HumanReadable("Dois")]
    Two = 2,

    /// <summary>
    /// The three
    /// </summary>
    [InternalValue("три")]
    [HumanReadable("Três")]
    Three = 3
}
