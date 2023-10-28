// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 09-06-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-06-2020
// ***********************************************************************
// <copyright file="ConfigurationCollection.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using CrispyWaffle.Tests.Fixtures;
using Xunit;

namespace CrispyWaffle.Tests.Collections;

/// <summary>
/// Class ConfigurationCollection.
/// Implements the <see cref="Xunit.ICollectionFixture{TFixture}" />
/// </summary>
/// <seealso cref="Xunit.ICollectionFixture{TFixture}" />
[CollectionDefinition("Configuration collection")]
public class ConfigurationCollection : ICollectionFixture<ConfigurationFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
