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

using CrispyWaffle.Tests.Fixtures;
using Xunit;

namespace CrispyWaffle.Tests.Collections
{
    /// <summary>
    /// Class ServiceLocatorCollection.
    /// Implements the <see cref="Xunit.ICollectionFixture{BootstrapFixture}" />
    /// </summary>
    /// <seealso cref="Xunit.ICollectionFixture{BootstrapFixture}" />
    [CollectionDefinition("Logged collection")]
    public class LoggedCollection : ICollectionFixture<LoggingFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
