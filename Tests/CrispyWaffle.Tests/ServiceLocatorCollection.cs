// ***********************************************************************
// Assembly         : CrispyWaffle.Tests
// Author           : Guilherme Branco Stracini
// Created          : 09-04-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-04-2020
// ***********************************************************************
// <copyright file="ServiceLocatorCollection.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Xunit;

namespace CrispyWaffle.Tests
{
    /// <summary>
    /// Class ServiceLocatorCollection.
    /// Implements the <see cref="Xunit.ICollectionFixture{CrispyWaffle.Tests.}BootstrapFixture}" />
    /// </summary>
    /// <seealso cref="Xunit.ICollectionFixture{BootstrapFixture}" />
    [CollectionDefinition("ServiceLocator collection")]
    public class ServiceLocatorCollection : ICollectionFixture<BootstrapFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.

    }
}
